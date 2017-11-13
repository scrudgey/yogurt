﻿using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
using System.Text.RegularExpressions;
public class MySaver {
	static int idIndex;
	public static List<GameObject> disabledPersistents = new List<GameObject>();
	public static Dictionary<int, GameObject> loadedObjects = new Dictionary<int, GameObject>();
	// NOTE: this system will not support having more than 2,147,483,648 saved objects.
	public static HashSet<int> loadedIds = new HashSet<int>();
	public static Dictionary<Type, Func<SaveHandler> > Handlers = new Dictionary<Type, Func<SaveHandler>>{
		{typeof(Inventory), 						() => new InventoryHandler() },
		{typeof(PhysicalBootstrapper), 				() => new PhysicalBootStrapperHandler() },
		{typeof(Eater),								() => new EaterHandler() },
		{typeof(AdvancedAnimation),					() => new AdvancedAnimationHandler() },
		{typeof(Flammable),							() => new FlammableHandler() },
		{typeof(Destructible),						() => new DestructibleHandler() },
		{typeof(LiquidContainer),					() => new LiquidContainerHandler() },
		{typeof(Container),							() => new ContainerHandler()},
		{typeof(Toilet),							() => new ContainerHandler()},
		{typeof(Blender),							() => new BlenderHandler()},
		{typeof(Head),								() => new HeadHandler() },
		{typeof(Outfit),							() => new OutfitHandler() },
		{typeof(Cabinet),							() => new CabinetHandler() },
		{typeof(Intrinsics),						() => new IntrinsicsHandler() },
		{typeof(Package),							() => new PackageHandler() },
		{typeof(Trader),							() => new TraderHandler() },
		{typeof(DecisionMaker),						() => new DecisionMakerHandler() },
		{typeof(Awareness),							() => new AwarenessHandler() },
		{typeof(Hurtable),							() => new HurtableHandler() },
		{typeof(HeadAnimation),						() => new HeadAnimationHandler() },
		{typeof(Speech),							() => new SpeechHandler() },
		{typeof(Humanoid),							() => new HumanoidHandler() },
		{typeof(Stain),								() => new StainHandler() }, 
		{typeof(DropDripper),						() => new DropDripperHandler() }
	};
	public static List<Type> LoadOrder = new List<Type>{
		typeof(Intrinsics)
	};
	static int CompareComponent(Component x, Component y)
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
		ReferenceResolver resolver = new ReferenceResolver();
		var serializer = new XmlSerializer(typeof(PersistentContainer));
		string scenePath = GameManager.Instance.LevelSavePath();
		string playerPath = GameManager.Instance.PlayerSavePath();
		FileStream sceneStream = File.Create(scenePath);
		FileStream playerStream = File.Create(playerPath);
		// retrieve all persistent objects
		HashSet<GameObject> objectList = new HashSet<GameObject>();
		List<Persistent> persistents = new List<Persistent>();
		// add those objects which are disabled and would therefore not be found by our first search
		foreach(GameObject disabledPersistent in disabledPersistents){
			objectList.Add(disabledPersistent);
		}
		foreach (MyMarker mark in GameObject.FindObjectsOfType<MyMarker>()){
			objectList.Add(mark.gameObject);
		}
		// create a persistent for each gameobject with the appropriate data
		foreach (GameObject gameObject in objectList){
			Persistent persistent = new Persistent(gameObject);
			MyMarker marker = gameObject.GetComponent<MyMarker>();
			if (marker.id != - 1){
				persistent.id = marker.id;
				loadedIds.Add(marker.id);
			} else {
				idIndex++;
				while (loadedIds.Contains(idIndex)){
					idIndex++;
				}
				persistent.id = idIndex;
				//TODO: check if next index is taken!
			}
			persistents.Add(persistent);
			resolver.objectIDs.Add(gameObject, persistent.id);
			resolver.persistentObjects.Add(persistent, gameObject);
		}
		// invoke the data handling here - this will populate all the component data, and assign a unique id to everything.
		foreach (Persistent persistent in persistents){
			persistent.HandleSave(resolver);
		}
		List<Persistent> playerTree = resolver.RetrieveReferenceTree(GameManager.Instance.playerObject);
		List<Persistent> sceneTree = persistents.Except(playerTree).ToList();
		// lastly we need to clean up any references the scene objects have to the player objects
		// Dictionary<GameObject, int> sceneIDs = new Dictionary<GameObject, int>();
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
	public static void HandleLoadedPersistents(List<Persistent> persistents){
		foreach (Persistent persistent in persistents){
			List<Component> loadedComponents = new List<Component>(loadedObjects[persistent.id].GetComponents<Component>());
			loadedComponents.Sort(CompareComponent);
			foreach (Component component in loadedComponents){
				Func<SaveHandler> getHandler;
				if (MySaver.Handlers.TryGetValue(component.GetType(), out getHandler)){
					var handler = getHandler();
					// PersistentComponent data = persistent.persistentComponents[component.GetType().ToString()];
					handler.LoadData(component, persistent.persistentComponents[component.GetType().ToString()]);
				}
			}
			// handle child objects
			foreach (PersistentComponent persistentChild in persistent.persistentChildComponents.Values){
				GameObject childObject = loadedObjects[persistent.id].transform.Find(persistentChild.parentObject).gameObject;
				Component childComponent = childObject.GetComponent(persistentChild.type);
				if (childObject && childComponent){
					// string lastChildComponent = childComponent.GetType().ToString();
					Func<SaveHandler> get;
					if (MySaver.Handlers.TryGetValue(childComponent.GetType(), out get)){
						var handler = get();
						PersistentComponent data = new PersistentComponent();
						if (persistent.persistentChildComponents.TryGetValue(childComponent.GetType().ToString(), out data ))
							handler.LoadData(childComponent, data);
					}
				} else {
					Debug.Log("could not resolve child or component on load");
					Debug.Log(childComponent.GetType().ToString() + " " + persistentChild.parentObject);
				}
			}
		}
	}
	public static GameObject LoadPersistentContainer(PersistentContainer container){
		GameObject rootObject = null;
		Regex reg =  new Regex("\\s+", RegexOptions.Multiline);
		foreach(Persistent persistent in container.PersistentObjects){
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
}
public class ReferenceResolver{
	public  Dictionary<Persistent, GameObject> persistentObjects = new Dictionary<Persistent, GameObject>();
	public  Dictionary<GameObject, int> objectIDs = new Dictionary<GameObject ,int>();
	private Dictionary<Persistent, List<Persistent>> referenceTree = new Dictionary<Persistent, List<Persistent>>();
	public int ResolveReference(GameObject referent, Persistent requester){
		int returnID = -1;
		if (referent == null)
			return -1;
		if (objectIDs.ContainsKey(referent))
			returnID = objectIDs[referent];
		if (!referenceTree.ContainsKey(requester))
			referenceTree.Add(requester, new List<Persistent>());
		referenceTree[requester].Add(persistentObjects.FindKeyByValue(referent));
		// if (returnID == -1){
		// 	Debug.Log("reference not resolved!");
		// 	Debug.Log("tried to resolve reference to "+referent.name);
		// }
		return returnID;
	}
	public void AddToReferenceTree(Persistent treeParent, Persistent child){
		if (child == null || treeParent == null)
			return;
		if (!referenceTree.ContainsKey(treeParent))
			referenceTree.Add(treeParent, new List<Persistent>());
		referenceTree[treeParent].Add(child);
	}
	public List<Persistent> RetrieveReferenceTree(GameObject target){
		Persistent targetPersistent = null;
		List<Persistent> tree = new List<Persistent>();
		if (persistentObjects.ContainsValue(target))
			targetPersistent = persistentObjects.FindKeyByValue(target);
		if (targetPersistent == null){
			Debug.Log("no entry in persistentobjects for "+target.name);
		}
		tree.Add(targetPersistent);
		if (referenceTree.ContainsKey(targetPersistent)){
			bool refCheck = true;
			int checkIterations = 0;
			while(refCheck && checkIterations < 7){
				refCheck = false;
				List<Persistent> nextTree = new List<Persistent>(tree);
				foreach (Persistent persistent in tree){
					if (persistent == null){
						Debug.Log("null object found in persistent tree! did you interact with something that isn't marked?");
						continue;
					}
					if (referenceTree.ContainsKey(persistent))
						nextTree.AddRange(referenceTree[persistent]);
				}
				nextTree= nextTree.Distinct().ToList();
				if (nextTree != tree)
					refCheck = true;
				tree = nextTree;
				checkIterations++;
			}
		}
		return tree;
	}
}