using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

public class MySaver {
	public static SerializableDictionary<int, PersistentObject> objectDataBase;
	private static Dictionary<int, List<int>> referenceTree = new Dictionary<int, List<int>>();
	public static Dictionary<int, GameObject> loadedObjects = new Dictionary<int, GameObject>();
	public static Dictionary<GameObject, int> savedObjects = new Dictionary<GameObject, int>();
	public static List<GameObject> disabledPersistents = new List<GameObject>();
	public static List<Type> LoadOrder = new List<Type>{
		typeof(Intrinsics),
		typeof(Outfit)
	};
	public static int CompareComponent(Component x, Component y)
    {
		int xIndex = 0;
		int yIndex = 0;
		if (LoadOrder.Contains(x.GetType()))
			xIndex = LoadOrder.IndexOf(x.GetType());
		if (LoadOrder.Contains(y.GetType()))
			yIndex = LoadOrder.IndexOf(y.GetType());
		return xIndex - yIndex;
    }
	public static void CleanupSaves(){
		string path = Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName);
		if (!System.IO.Directory.Exists(path))
			return;
		DirectoryInfo info = new DirectoryInfo(path);
		FileInfo[] fileInfo = info.GetFiles();
		foreach(FileInfo file in fileInfo){
			// don't delete the object database, the gamedata, or the house state
			if (file.Name == GameManager.Instance.saveGameName+".xml")
				continue;
			if (file.Name == "gameData.xml")
				continue;
			if (file.FullName == GameManager.Instance.data.lastSavedPlayerPath)
				continue;
			if (file.Name != "house_state.xml")
				File.Delete(file.FullName);
		}
		// GARBAGE COLLECTION
		// TODO: maybe a smarter algorithm here?
		var listSerializer = new XmlSerializer(typeof(List<int>));
		List<int> playerIDs = new List<int>();
		string playerPath = GameManager.Instance.data.lastSavedPlayerPath;
		if (File.Exists(playerPath)){
			var playerStream = new FileStream(playerPath, FileMode.Open);
			playerIDs = listSerializer.Deserialize(playerStream) as List<int>;
			playerStream.Close();
		}
		Stack<int> removeEntries = new Stack<int>();
		if (objectDataBase != null){
			foreach(KeyValuePair<int, PersistentObject> kvp in objectDataBase){
				if (kvp.Value.sceneName != "house"){
					if (!playerIDs.Contains(kvp.Key))
						removeEntries.Push(kvp.Key);
				}
			}
		}
		while(removeEntries.Count > 0){
			int entry = removeEntries.Pop();
			objectDataBase.Remove(entry);
		}
	}
	public static void Save(){
		referenceTree = new Dictionary<int, List<int>>();
		savedObjects = new Dictionary<GameObject, int>();
		var listSerializer = new XmlSerializer(typeof(List<int>));
		string objectsPath = GameManager.Instance.ObjectsSavePath();
		string scenePath = GameManager.Instance.LevelSavePath();
		string playerPath = GameManager.Instance.PlayerSavePath();
		if (File.Exists(objectsPath)){
			if (objectDataBase == null){
				var persistentSerializer = new XmlSerializer(typeof(SerializableDictionary<int, PersistentObject>));
				Debug.Log("loading existing "+objectsPath+" ...");
				System.IO.Stream objectsStream = new FileStream(objectsPath, FileMode.Open);
				objectDataBase = persistentSerializer.Deserialize(objectsStream) as SerializableDictionary<int, PersistentObject>;
				objectsStream.Close();
				// Debug.Log(objectDataBase.Count.ToString() +" entries found");
			}
		} else {
			Debug.Log("NOTE: creating new object database!");
			objectDataBase = new SerializableDictionary<int, PersistentObject>();
		}
		FileStream sceneStream = File.Create(scenePath);
		FileStream playerStream = File.Create(playerPath);
		// retrieve all persistent objects
		HashSet<GameObject> objectList = new HashSet<GameObject>();
		Dictionary<GameObject, PersistentObject> persistents = new Dictionary<GameObject, PersistentObject>();
		// add those objects which are disabled and would therefore not be found by our first search
		foreach(GameObject disabledPersistent in disabledPersistents){
			objectList.Add(disabledPersistent);
		}
		foreach (MyMarker mark in GameObject.FindObjectsOfType<MyMarker>()){
			objectList.Add(mark.gameObject);
		}
		Dictionary<GameObject, int> objectIDs = new Dictionary<GameObject, int>();
		HashSet<int> savedIDs = new HashSet<int>();
		// create a persistent for each gameobject with the appropriate data
		foreach (GameObject gameObject in objectList){
			MyMarker marker = gameObject.GetComponent<MyMarker>();
			PersistentObject persistent;
			// either get the existing persistent in the database, or make a new one
			if (objectDataBase.ContainsKey(marker.id)){
				persistent = objectDataBase[marker.id];
				persistent.Update(gameObject);
			} else {
				persistent = new PersistentObject(gameObject);
				marker.id = persistent.id;
			}
			persistents[gameObject] = persistent;
			objectIDs.Add(gameObject, persistent.id);
			savedIDs.Add(persistent.id);
			savedObjects[gameObject] = persistent.id;
		}
		// make sure children are referenced under their parents in the referencetree
		// foreach (KeyValuePair<GameObject, PersistentObject> kvp in persistents){
		// 	foreach(PersistentObject childObject in kvp.Value.persistentChildren){
		// 		// Debug.Log("child "+childObject.parentObject+ " "+childObject.id);
		// 		AddToReferenceTree(kvp.Key, childObject.id);
		// 	}
		// }
		// invoke the data handling here - this will populate all the component data, and assign a unique id to everything.
		foreach (KeyValuePair<GameObject, PersistentObject> kvp in persistents){
			kvp.Value.HandleSave(kvp.Key);
		}
		// separate lists of persistent objects for the scene and the player
		HashSet<int> playerTree = new HashSet<int>();
		RecursivelyAddTree(playerTree, objectIDs[GameManager.Instance.playerObject]);
		listSerializer.Serialize(sceneStream, savedIDs.ToList().Except(playerTree.ToList()).ToList());
		// remove all children objects from player tree. they are included in prefab.
		// note: the order of operations here means that child objects aren't in the scene or player trees.
		Stack<int> playerChildObjects = new Stack<int>();
		if (playerTree == null)
			Debug.Log("null player tree! wtf");
		foreach(int idn in playerTree){
			if (objectDataBase.ContainsKey(idn)){
				if (objectDataBase[idn].childObject){
					playerChildObjects.Push(idn);
				} 
			} else {
				Debug.Log("couldn't find "+idn.ToString()+" in objectdatabase??");
			}
		}
		while(playerChildObjects.Count > 0){
			playerTree.Remove(playerChildObjects.Pop());
		}
		listSerializer.Serialize(playerStream, playerTree.ToList());
		// close the XML serialization stream
		sceneStream.Close();
		playerStream.Close();
		GameManager.Instance.SaveGameData();
		if (!File.Exists(objectsPath)){
			SaveObjectDatabase();
		}
	}
	public static void SaveObjectDatabase(){
		var persistentSerializer = new XmlSerializer(typeof(SerializableDictionary<int, PersistentObject>));
		string objectsPath = GameManager.Instance.ObjectsSavePath();
		FileStream objectStream = File.Create(objectsPath);
		// Debug.Log("saving "+objectsPath+" ...");
		// Debug.Log(objectDataBase.Count);
		persistentSerializer.Serialize(objectStream, objectDataBase);
		objectStream.Close();
	}
	public static GameObject LoadScene(){
		UINew.Instance.ClearWorldButtons();
		GameObject playerObject = null;
		loadedObjects = new Dictionary<int, GameObject>();
		
		string objectsPath = GameManager.Instance.ObjectsSavePath();
		string scenePath = GameManager.Instance.LevelSavePath();
		string playerPath = GameManager.Instance.data.lastSavedPlayerPath;

		if (File.Exists(objectsPath)){
			if (objectDataBase == null){
				Debug.Log("loading "+objectsPath+" ...");
				var persistentSerializer = new XmlSerializer(typeof(SerializableDictionary<int, PersistentObject>));
				System.IO.Stream objectsStream = new FileStream(objectsPath, FileMode.Open);
				objectDataBase = persistentSerializer.Deserialize(objectsStream) as SerializableDictionary<int, PersistentObject>;
				objectsStream.Close();
				Debug.Log(objectDataBase.Count);
			}
		} else {
			Debug.Log("WEIRD: no existing object database on Load!");
			objectDataBase = new SerializableDictionary<int, PersistentObject>();
		}
		// destroy any currently existing permanent object
		// this should only be done if there exists a savestate for the level.
		// otherwise the default unity editor scene should be loaded as is.
		if (File.Exists(scenePath)){
			List<MyMarker> marks = new List<MyMarker>(GameObject.FindObjectsOfType<MyMarker>());
			Stack<GameObject> gameObjectsToDestroy = new Stack<GameObject>();
			for (int i = 0; i < marks.Count; i++){
				if (marks[i] != null){
					if (marks[i].staticObject)
						continue;
					gameObjectsToDestroy.Push(marks[i].gameObject);
				}
			}
			foreach (GameObject disabledPersistent in disabledPersistents){
				gameObjectsToDestroy.Push(disabledPersistent);
			}
			while (gameObjectsToDestroy.Count > 0){
				GameObject.DestroyImmediate(gameObjectsToDestroy.Pop());
			}
		}
		disabledPersistents = new List<GameObject>();
		List<int> sceneIDs = new List<int>();
		List<int> playerIDs = new List<int>();
		var listSerializer = new XmlSerializer(typeof(List<int>));
		if (File.Exists(scenePath)){
			var sceneStream = new FileStream(scenePath, FileMode.Open);
			sceneIDs = listSerializer.Deserialize(sceneStream) as List<int>;
			sceneStream.Close();
			LoadObjects(sceneIDs);
		}
		if (File.Exists(playerPath)){
			var playerStream = new FileStream(playerPath, FileMode.Open);
			playerIDs = listSerializer.Deserialize(playerStream) as List<int>;
			playerStream.Close();
			playerObject = LoadObjects(playerIDs);
		} else {
			playerObject = GameObject.Instantiate(Resources.Load("prefabs/Tom")) as GameObject;
		}
		HandleLoadedPersistents(sceneIDs);
		HandleLoadedPersistents(playerIDs);	
		return playerObject;
	}
	public static void HandleLoadedPersistents(List<int> ids){
		// TODO: smarter check?
		foreach(int idn in ids){
			PersistentObject persistent = null;
			if (objectDataBase.TryGetValue(idn, out persistent)){
				// TODO: handle update instead of replacement
				persistent.HandleLoad(loadedObjects[idn]);
			}
		}
	}
	public static GameObject LoadObjects(List<int> ids){
		GameObject rootObject = null;
		foreach(int idn in ids){
			PersistentObject persistent = null;
			if (objectDataBase.TryGetValue(idn, out persistent)){
				// TODO: do something smarter to find the child object
				// if (persistent.childObject)
				// 	continue;
				GameObject go = null;
				if (persistent.noPrefab){
					go = GameObject.Find(persistent.name);
				} else {
					go = GameObject.Instantiate(
					Resources.Load(persistent.prefabPath),
					persistent.transformPosition,
					persistent.transformRotation) as GameObject;
				}
				if (go == null)
					continue;
				loadedObjects[persistent.id] = go;
				go.BroadcastMessage("LoadInit", SendMessageOptions.DontRequireReceiver);
				go.name = Toolbox.Instance.ScrubText(go.name);
				if (!rootObject)
					rootObject = go;
				MyMarker marker = go.GetComponent<MyMarker>();
				if (marker){
					marker.id = persistent.id;
				}
			} else {
				Debug.Log("ERROR: object "+idn.ToString()+" not found in database!");
			}
		}
		return rootObject;
	}
	public static void AddToReferenceTree(GameObject parent, GameObject child){
		if (parent == null || child == null)
			return;
		if (savedObjects.ContainsKey(child) && savedObjects.ContainsKey(parent)){
			AddToReferenceTree(savedObjects[parent], savedObjects[child]);
		}
	}
	public static void AddToReferenceTree(GameObject parent, int child){
		if (child == -1 || parent == null)
			return;
		if (savedObjects.ContainsKey(parent)){
			AddToReferenceTree(savedObjects[parent], child);
		}
	}
	public static void AddToReferenceTree(int parent, GameObject child){
		if (parent == -1 || child == null)
			return;
		if (savedObjects.ContainsKey(child)){
			AddToReferenceTree(parent, savedObjects[child]);
		}
	}
	public static void AddToReferenceTree(int parent, int child){
		if (child == -1 || parent == -1)
			return;
		// Debug.Log("adding "+child.ToString()+" under "+parent.ToString());
		if (!referenceTree.ContainsKey(parent))
			referenceTree[parent] = new List<int>();
		referenceTree[parent].Add(child);
	}
	public static void RecursivelyAddTree(HashSet<int> tree, int node){
		tree.Add(node);
		if (referenceTree.ContainsKey(node)){
			foreach(int idn in referenceTree[node]){
				if (!tree.Contains(idn)){
					RecursivelyAddTree(tree, idn);
				}
			}
		}
	}
	public static void UpdateGameObjectReference(GameObject referent, PersistentComponent data, string key, bool overWriteWithNull=true){
		// is the target object null?
		if (referent == null){
			if (overWriteWithNull){
				data.ints[key] = -1;
				return;
			} else {
				return;
			}
		}
		int i = -1;
		// is the non-null object in the saved objects?
		if (savedObjects.TryGetValue(referent, out i)){
			data.ints[key] = i;
		} else {
			if (overWriteWithNull){
				data.ints[key] = -1;
			}
		}
	}
	public static GameObject IDToGameObject(int idn){
		if (idn == -1)
			return null;
		GameObject returnObject;
		loadedObjects.TryGetValue(idn, out returnObject);
		return returnObject;
	}
	public static int NextIDNumber(){
		IEnumerable<int> ids = Enumerable.Range(0, objectDataBase.Count);
		foreach(int idn in ids){
			if(!objectDataBase.ContainsKey(idn))
				return idn;
		}
		return objectDataBase.Count+1;
	}
}
