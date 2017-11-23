using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
using System.Text.RegularExpressions;
public class MySaver {
	// public static ReferenceResolver resolver;
	private static Dictionary<PersistentObject, List<PersistentObject>> referenceTree = new Dictionary<PersistentObject, List<PersistentObject>>();
	public static Dictionary<PersistentObject, GameObject> persistentObjects = new Dictionary<PersistentObject, GameObject>();
	public static Dictionary<GameObject, int> objectIDs = new Dictionary<GameObject ,int>();
	static int idIndex;
	public static List<GameObject> disabledPersistents = new List<GameObject>();
	public static Dictionary<int, GameObject> loadedObjects = new Dictionary<int, GameObject>();
	// NOTE: this system will not support having more than 2,147,483,648 saved objects.
	public static HashSet<int> loadedIds = new HashSet<int>();
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
		typeof(Intrinsics)
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
		loadedIds = new HashSet<int>();
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
		// resolver = new ReferenceResolver();
		objectIDs = new Dictionary<GameObject, int>();
		persistentObjects = new Dictionary<PersistentObject, GameObject>();

		var serializer = new XmlSerializer(typeof(PersistentContainer));
		string scenePath = GameManager.Instance.LevelSavePath();
		string playerPath = GameManager.Instance.PlayerSavePath();
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
		// create a persistent for each gameobject with the appropriate data
		foreach (GameObject gameObject in objectList){
			PersistentObject persistent = new PersistentObject(gameObject);
			MyMarker marker = gameObject.GetComponent<MyMarker>();
			if (marker.id != - 1){
				// TODO: put more logic in here if the index is taken
				persistent.id = marker.id;
				loadedIds.Add(marker.id);
			} else {
				idIndex++;
				while (loadedIds.Contains(idIndex)){
					idIndex++;
				}
				persistent.id = idIndex;
			}
			persistents[gameObject] = persistent;
			objectIDs.Add(gameObject, persistent.id);
			persistentObjects.Add(persistent, gameObject);
		}
		// invoke the data handling here - this will populate all the component data, and assign a unique id to everything.
		foreach (KeyValuePair<GameObject, PersistentObject> kvp in persistents){
			kvp.Value.HandleSave(kvp.Key);
		}
		// separate lists of persistent objects for the scene and the player
		List<PersistentObject> playerTree = RetrieveReferenceTree(GameManager.Instance.playerObject);
		List<PersistentObject> sceneTree = persistents.Values.Except(playerTree).ToList();
		PersistentContainer sceneContainer = new PersistentContainer(sceneTree);
		PersistentContainer playerContainer = new PersistentContainer(playerTree);
		// save the persistent object container
		serializer.Serialize(sceneStream, sceneContainer);
		serializer.Serialize(playerStream, playerContainer);
		// close the XML serialization stream
		sceneStream.Close();
		playerStream.Close();
		GameManager.Instance.SaveGameData();
	}
	public static GameObject LoadScene(){
		UINew.Instance.ClearWorldButtons();
		GameObject playerObject = null;
		string scenePath = GameManager.Instance.LevelSavePath();
		string playerPath = GameManager.Instance.data.lastSavedPlayerPath;
		var serializer = new XmlSerializer(typeof(PersistentContainer));
		// destroy any currently existing permanent object
		// this should only be done if there exists a savestate for the level.
		// otherwise the default unity editor scene should be loaded as is.
		if (File.Exists(scenePath)){
			List<MyMarker> marks = new List<MyMarker>(GameObject.FindObjectsOfType<MyMarker>());
			for (int i = 0; i < marks.Count; i++){
				if (marks[i] != null){
					GameObject.DestroyImmediate(marks[i].gameObject);
				}
			}
			foreach (GameObject disabledPersistent in disabledPersistents){
				GameObject.DestroyImmediate(disabledPersistent);
			}
		}
		disabledPersistents = new List<GameObject>();
		loadedObjects = new Dictionary<int, GameObject>();
		PersistentContainer sceneContainer = null;
		PersistentContainer playerContainer = null;
		if (File.Exists(scenePath)){
			var sceneStream = new FileStream(scenePath, FileMode.Open);
			sceneContainer = serializer.Deserialize(sceneStream) as PersistentContainer;
			sceneStream.Close();
			LoadPersistentContainer(sceneContainer);
		}
		if (File.Exists(playerPath)){
			var playerStream = new FileStream(playerPath, FileMode.Open);
			playerContainer = serializer.Deserialize(playerStream) as PersistentContainer;
			playerStream.Close();
			playerObject = LoadPersistentContainer(playerContainer);
		} else {
			playerObject = GameObject.Instantiate(Resources.Load("prefabs/Tom")) as GameObject;
		}
		if (sceneContainer != null)
			HandleLoadedPersistents(sceneContainer.PersistentObjects);
		if (playerContainer != null)
			HandleLoadedPersistents(playerContainer.PersistentObjects);	
		return playerObject;
	}
	public static void HandleLoadedPersistents(List<PersistentObject> persistents){
		foreach (PersistentObject persistent in persistents){
			GameObject gameObject = loadedObjects[persistent.id];
			persistent.HandleLoad(gameObject);
		}
	}
	public static GameObject LoadPersistentContainer(PersistentContainer container){
		GameObject rootObject = null;
		Regex reg =  new Regex("\\s+", RegexOptions.Multiline);
		foreach(PersistentObject persistent in container.PersistentObjects){
			loadedIds.Add(persistent.id);
			string path = @"prefabs/"+persistent.name;
			path = reg.Replace(path, "_");
			GameObject go = GameObject.Instantiate(
				Resources.Load(path),
				persistent.transformPosition,
				persistent.transformRotation) as GameObject;
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
		return rootObject;
	}
	public static void AddToReferenceTree(GameObject treeParent, GameObject child){
		if (child == null || treeParent == null)
			return;
		PersistentObject parentPersistentObject = persistentObjects.FindKeyByValue(treeParent);
		PersistentObject childPersistentObject = persistentObjects.FindKeyByValue(child);
		if (parentPersistentObject == null || childPersistentObject == null)
			return;
		if (!referenceTree.ContainsKey(parentPersistentObject))
			referenceTree.Add(parentPersistentObject, new List<PersistentObject>());
		referenceTree[parentPersistentObject].Add(childPersistentObject);
	}
	public static List<PersistentObject> RetrieveReferenceTree(GameObject target){
		PersistentObject targetPersistent = null;
		HashSet<PersistentObject> tree = new HashSet<PersistentObject>();
		if (persistentObjects.ContainsValue(target))
			targetPersistent = persistentObjects.FindKeyByValue(target);
		if (targetPersistent == null){
			Debug.Log("no entry in persistentobjects for "+target.name);
			return tree.ToList();
		}
		RecursivelyAddTree(tree, targetPersistent);
		tree.Add(targetPersistent);
		return tree.ToList();
	}
	public static void RecursivelyAddTree(HashSet<PersistentObject> tree, PersistentObject node){
		tree.Add(node);
		if (referenceTree.ContainsKey(node)){
			foreach(PersistentObject obj in referenceTree[node]){
				if (!tree.Contains(obj)){
					RecursivelyAddTree(tree, obj);
				}
			}
		}
	}
	public static int GameObjectToID(GameObject referent){
		int returnID = -1;
		if (referent == null)
			return -1;
		objectIDs.TryGetValue(referent, out returnID);
		// if (objectIDs.ContainsKey(referent))
		// 	returnID = objectIDs[referent];
		// if (returnID == -1){
		// 	Debug.Log("reference not resolved!");
		// 	Debug.Log("tried to resolve reference to "+referent.name);
		// }
		return returnID;
	}
	// public static GameObject IDToGameObject(int id){
	// 	GameObject returnObj = null;
	// 	loadedObjects.TryGetValue(id, out returnObj);
	// 	return returnObj;
	// 	// if (loadedObjects.ContainsKey(id)){
	// 	// 	returnObj = loadedObjects[]
	// 	// }
	// } 
}
// public class ReferenceResolver{
// 	public  Dictionary<PersistentObject, GameObject> persistentObjects = new Dictionary<PersistentObject, GameObject>();
// 	public  Dictionary<GameObject, int> objectIDs = new Dictionary<GameObject ,int>();
// 	public int ResolveReference(GameObject referent){
// 		int returnID = -1;
// 		if (referent == null)
// 			return -1;
// 		if (objectIDs.ContainsKey(referent))
// 			returnID = objectIDs[referent];
// 		// if (returnID == -1){
// 		// 	Debug.Log("reference not resolved!");
// 		// 	Debug.Log("tried to resolve reference to "+referent.name);
// 		// }
// 		return returnID;
// 	}
// }