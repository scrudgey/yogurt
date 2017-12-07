using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
// using System.Text.RegularExpressions;
public class MySaver {
	public static PersistentContainer objectDataBase;
	static int idIndex;
	private static Dictionary<int, List<int>> referenceTree = new Dictionary<int, List<int>>();
	public static Dictionary<int, GameObject> loadedObjects = new Dictionary<int, GameObject>();
	public static Dictionary<GameObject, int> savedObjects = new Dictionary<GameObject, int>();
	public static List<GameObject> disabledPersistents = new List<GameObject>();
	public static Dictionary<Type, SaveHandler> Handlers = new Dictionary<Type, SaveHandler>{
		{typeof(Inventory), 						new InventoryHandler()},
		{typeof(PhysicalBootstrapper), 				new PhysicalBootStrapperHandler()},
		{typeof(Eater),								new EaterHandler()},
		{typeof(AdvancedAnimation),					new AdvancedAnimationHandler()},
		{typeof(Flammable),							new FlammableHandler()},
		{typeof(Destructible),						new DestructibleHandler()},
		{typeof(LiquidContainer),					new LiquidContainerHandler()},
		{typeof(Container),							new ContainerHandler()},
		{typeof(Toilet),							new ContainerHandler()},
		{typeof(Blender),							new BlenderHandler()},
		{typeof(Head),								new HeadHandler()},
		{typeof(Outfit),							new OutfitHandler()},
		{typeof(Cabinet),							new CabinetHandler()},
		{typeof(Intrinsics),						new IntrinsicsHandler()},
		{typeof(Package),							new PackageHandler()},
		{typeof(Trader),							new TraderHandler()},
		{typeof(DecisionMaker),						new DecisionMakerHandler()},
		{typeof(Awareness),							new AwarenessHandler()},
		{typeof(Hurtable),							new HurtableHandler()},
		{typeof(HeadAnimation),						new HeadAnimationHandler()},
		{typeof(Speech),							new SpeechHandler()},
		{typeof(Humanoid),							new HumanoidHandler()},
		{typeof(Stain),								new StainHandler()}, 
		{typeof(DropDripper),						new DropDripperHandler()},
		{typeof(VideoCamera),						new VideoCameraHandler()}
	};
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
		idIndex = 0;
		// loadedIds = new HashSet<int>();
		string path = Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName);
		if (!System.IO.Directory.Exists(path))
			return;
		DirectoryInfo info = new DirectoryInfo(path);
		FileInfo[] fileInfo = info.GetFiles();
		foreach(FileInfo file in fileInfo){
			if (file.Name == "game.xml")
				continue;
			if (file.FullName == GameManager.Instance.data.lastSavedPlayerPath)
				continue;
			if (file.Name != "house_state.xml")
				File.Delete(file.FullName);
		}
	}
	public static void Save(){
		
		savedObjects = new Dictionary<GameObject,int>();
		var listSerializer = new XmlSerializer(typeof(List<int>));
		var persistentSerializer = new XmlSerializer(typeof(PersistentContainer));
		string objectsPath = GameManager.Instance.ObjectsSavePath();
		string scenePath = GameManager.Instance.LevelSavePath();
		string playerPath = GameManager.Instance.PlayerSavePath();
		Debug.Log("saving "+objectsPath+" ...");
		if (File.Exists(objectsPath)){
			System.IO.Stream objectsStream = new FileStream(objectsPath, FileMode.Open);
			objectDataBase = persistentSerializer.Deserialize(objectsStream) as PersistentContainer;
			objectsStream.Close();
			// TODO: update the persistent object parameters (position, transform, etc)
		} else {
			objectDataBase = new PersistentContainer();
		}
		FileStream sceneStream = File.Create(scenePath);
		FileStream playerStream = File.Create(playerPath);
		FileStream objectStream = File.Create(objectsPath);
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
			if (objectDataBase.PersistentObjects.ContainsKey(marker.id)){
				persistent = objectDataBase.PersistentObjects[marker.id];
			} else {
				persistent = new PersistentObject(gameObject);
				idIndex++;
				while (objectDataBase.PersistentObjects.ContainsKey(idIndex)){
					idIndex++;
				}
				persistent.id = idIndex;
				marker.id = idIndex;
				objectDataBase.PersistentObjects[idIndex] = persistent;
			}
			persistents[gameObject] = persistent;
			objectIDs.Add(gameObject, persistent.id);
			savedIDs.Add(persistent.id);
			savedObjects[gameObject] = persistent.id;
		}
		// invoke the data handling here - this will populate all the component data, and assign a unique id to everything.
		foreach (KeyValuePair<GameObject, PersistentObject> kvp in persistents){
			kvp.Value.HandleSave(kvp.Key);
		}
		// separate lists of persistent objects for the scene and the player
		HashSet<int> playerTree = new HashSet<int>();
		RecursivelyAddTree(playerTree, objectIDs[GameManager.Instance.playerObject]);
		// save the persistent object container
		persistentSerializer.Serialize(objectStream, objectDataBase);
		listSerializer.Serialize(sceneStream, savedIDs.ToList().Except(playerTree.ToList()).ToList());
		listSerializer.Serialize(playerStream, playerTree.ToList());
		// close the XML serialization stream
		sceneStream.Close();
		playerStream.Close();
		objectStream.Close();
		GameManager.Instance.SaveGameData();
	}
	public static GameObject LoadScene(){
		UINew.Instance.ClearWorldButtons();
		GameObject playerObject = null;
		loadedObjects = new Dictionary<int, GameObject>();
		
		string objectsPath = GameManager.Instance.ObjectsSavePath();
		Debug.Log("loading "+objectsPath+" ...");
		string scenePath = GameManager.Instance.LevelSavePath();
		string playerPath = GameManager.Instance.data.lastSavedPlayerPath;

		if (File.Exists(objectsPath)){
			var persistentSerializer = new XmlSerializer(typeof(PersistentContainer));
			System.IO.Stream objectsStream = new FileStream(objectsPath, FileMode.Open);
			objectDataBase = persistentSerializer.Deserialize(objectsStream) as PersistentContainer;
			objectsStream.Close();
		} else {
			Debug.Log("WEIRD: no existing object database on Load!");
			objectDataBase = new PersistentContainer();
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
			if (objectDataBase.PersistentObjects.TryGetValue(idn, out persistent)){
				// TODO: handle update instead of replacement
				persistent.HandleLoad(loadedObjects[idn]);
			}
		}
	}
	public static GameObject LoadObjects(List<int> ids){
		Debug.Log("loading "+ids.Count.ToString());
		GameObject rootObject = null;
		foreach(int idn in ids){
			Debug.Log("loading "+idn.ToString());
			PersistentObject persistent = null;
			if (objectDataBase.PersistentObjects.TryGetValue(idn, out persistent)){

				Debug.Log("found object "+idn.ToString()+" in database");
				GameObject go = null;
				if (persistent.noPrefab){
					// Debug.Log("finding object with name "+persistent.name);
					go = GameObject.Find(persistent.name);
				} else {
					go = GameObject.Instantiate(
					Resources.Load(persistent.prefabPath),
					persistent.transformPosition,
					persistent.transformRotation) as GameObject;
				}
				if (go == null)
					continue;
				loadedObjects.Add(persistent.id, go);
				go.BroadcastMessage("LoadInit", SendMessageOptions.DontRequireReceiver);
				go.name = Toolbox.Instance.ScrubText(go.name);
				if (!rootObject)
					rootObject = go;
				MyMarker marker = go.GetComponent<MyMarker>();
				if (marker){
					marker.id = persistent.id;
				}
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
		if (!referenceTree.ContainsKey(parent))
			referenceTree.Add(parent, new List<int>());
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
	public static void UpdateGameObjectReference(GameObject referent, PersistentComponent data, string key){
		int i = -1;
		if (referent == null)
			return;
		if (savedObjects.TryGetValue(referent, out i)){
			data.ints[key] = i;
		}
	}
	public static GameObject IDToGameObject(int idn){
		if (idn == -1)
			return null;
		GameObject returnObject;
		loadedObjects.TryGetValue(idn, out returnObject);
		return returnObject;
	}
}
