using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
using System.Text.RegularExpressions;


public class MySaver {

	public delegate void SaveAction();
	// public static event SaveAction OnSave;
	public delegate void LoadAction();
	public enum SaverState{None, Saving, Loading}
	public static SaverState saveState;
	public static List<GameObject> disabledPersistents = new List<GameObject>();
	public static Dictionary<int, GameObject> loadedObjects = new Dictionary<int, GameObject>();

	public static Dictionary<Type, Func<SaveHandler> > Handlers = new Dictionary<Type, Func<SaveHandler>>{
		{typeof(Inventory), 						() => new InventoryHandler() },
		{typeof(PhysicalBootstrapper), 				() => new PhysicalBootStrapperHandler() },
		{typeof(Eater),								() => new EaterHandler() },
		{typeof(AdvancedAnimation),					() => new AdvancedAnimationHandler() },
		{typeof(Flammable),							() => new FlammableHandler() },
		{typeof(Destructible),						() => new DestructibleHandler() },
		{typeof(LiquidContainer),					() => new LiquidContainerHandler() },
		{typeof(Container),							() => new ContainerHandler()},
		{typeof(Blender),							() => new ContainerHandler() },
		{typeof(Head),								() => new HeadHandler() },
		{typeof(Outfit),							() => new OutfitHandler() },
		{typeof(Cabinet),							() => new CabinetHandler() },
	};

	public static void CleanupSaves(){
		string testPath = Path.Combine(Application.persistentDataPath, "test");
		// DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath);
		DirectoryInfo info = new DirectoryInfo(testPath);
		FileInfo[] fileInfo = info.GetFiles();
		foreach(FileInfo file in fileInfo){
			File.Delete(file.FullName);
		}
	}
	
	public static void Save(){
		try {
			saveState = SaverState.Saving;
			// open XML serialization stream
			// TODO: make this path nicer later when i have a directory structure
			var serializer = new XmlSerializer(typeof(PersistentContainer));
			string scenePath = GameManager.Instance.LevelSavePath();
			string playerPath = GameManager.Instance.PlayerSavePath();
			FileStream sceneStream = File.Create(scenePath);
			FileStream playerStream = File.Create(playerPath);
			
			// retrieve all persistent objects
			// TODO: theres probably a nice way to do this with linq but what the hell
			List<GameObject> objectList = new List<GameObject>();
			List<MyMarker> marks = new List<MyMarker>( GameObject.FindObjectsOfType<MyMarker>() );
			foreach (MyMarker mark in marks){
				objectList.Add(mark.gameObject);
			}
			// add those objects which are disabled and would therefore not be found by our first search
			objectList.AddRange(disabledPersistents);

			ReferenceResolver resolver = new ReferenceResolver();
			List<Persistent> persistentObjects = new List<Persistent>();
			int idIndex = 0;
			// create a persistent for each gameobject with the appropriate data
			foreach (GameObject gameObject in objectList){
				Persistent persistent = new Persistent(gameObject);
				persistent.id = idIndex;
				persistentObjects.Add(persistent);
				resolver.objectIDs.Add(gameObject, idIndex);
				resolver.persistentObjects.Add(persistent, gameObject);
				idIndex++;
			}

			// invoke the data handling here - this will populate all the component data, and assign a unique id to everything.
			foreach (Persistent persistent in persistentObjects){
					persistent.HandleSave(resolver);
			}

			List<Persistent> playerTree = resolver.RetrieveReferenceTree(GameManager.Instance.playerObject);
			List<Persistent> sceneTree = persistentObjects.Except(playerTree).ToList();

			// lastly we need to clean up any references the scene objects have to the player objects
			ReferenceResolver sceneResolver = new ReferenceResolver();
			Dictionary<GameObject, int> sceneIDs = new Dictionary<GameObject, int>();
			sceneResolver.persistentObjects = resolver.persistentObjects;
			foreach (Persistent persistent in sceneTree){
				sceneIDs.Add(resolver.persistentObjects[persistent], resolver.objectIDs[resolver.persistentObjects[persistent]]);
			}
			sceneResolver.objectIDs = sceneIDs;
			foreach (Persistent persistenet in sceneTree)
				persistenet.HandleSave(sceneResolver);

			PersistentContainer sceneContainer = new PersistentContainer(sceneTree);
			PersistentContainer playerContainer = new PersistentContainer(playerTree);

			// save the persistent object container
			serializer.Serialize(sceneStream, sceneContainer);
			serializer.Serialize(playerStream, playerContainer);
			
			// close the XML serialization stream
			sceneStream.Close();
			playerStream.Close();

			// call the save event
			// if (OnSave != null)
			// 	OnSave();
		} catch (System.Exception ex){
			Debug.Log("Problem saving!");
			Debug.Log(ex.Message);
			Debug.Log(ex.TargetSite);
		}
		
		GameManager.Instance.SaveGameData();
		saveState = SaverState.None;
	}

	public static GameObject LoadScene(){
		GameObject playerObject = null;
		try {
			saveState = SaverState.Loading;
			string scenePath = GameManager.Instance.LevelSavePath();
			string playerPath = GameManager.Instance.lastSavedPlayerPath;
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
			if (File.Exists(scenePath)){
				var sceneStream = new FileStream(scenePath,FileMode.Open);
				PersistentContainer sceneContainer = serializer.Deserialize(sceneStream) as PersistentContainer;
				sceneStream.Close();
				LoadPersistentContainer(sceneContainer);
			}
			if (File.Exists(playerPath)){
				var playerStream = new FileStream(playerPath,FileMode.Open);
				PersistentContainer playerContainer = serializer.Deserialize(playerStream) as PersistentContainer;
				playerStream.Close();
				playerObject = LoadPersistentContainer(playerContainer);
			} else {
				playerObject = GameObject.Instantiate(Resources.Load("prefabs/Tom")) as GameObject;
			}
		} catch {
			Debug.Log("problem loading!");
		}
		// GameManager.Instance.SaveGameData();
		saveState = SaverState.None;
		return playerObject;
	}

	public static GameObject LoadPersistentContainer(PersistentContainer container){
		
		string lastName = "first";
		string lastComponent = "first";
		GameObject rootObject = null;
		try {
			Regex reg =  new Regex("\\s+", RegexOptions.Multiline);
			loadedObjects = new Dictionary<int, GameObject>();
			foreach(Persistent persistent in container.PersistentObjects){
				lastName = persistent.name;
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
			}
		} catch {
			Debug.Log("Error occurred when instantiating persistent object "+ lastName);
		}
		//		now that each persistent object is loaded, handle references
		foreach (Persistent persistent in container.PersistentObjects){
			lastName = persistent.name;
			foreach (Component component in loadedObjects[persistent.id].GetComponents<Component>()){
				lastComponent = component.GetType().ToString();
				Func<SaveHandler> get;
				if (MySaver.Handlers.TryGetValue(component.GetType(), out get)){
					try {
						var handler = get();
						PersistentComponent data = persistent.persistentComponents[component.GetType().ToString()];
						handler.LoadData(component, data);
					} catch {
						Debug.Log("Error occured when configuring "+lastComponent+" on "+lastName);
					}
				}	
			}
			lastName = "finished";
			lastComponent = "finished"; 
			// handle child objects
			foreach (PersistentComponent persistentChild in persistent.persistentChildComponents.Values){
				GameObject childObject = loadedObjects[persistent.id].transform.FindChild(persistentChild.parentObject).gameObject;
				Component childComponent = childObject.GetComponent(persistentChild.type);
				if (childObject && childComponent){
					string lastChildComponent = childComponent.GetType().ToString();
					Func<SaveHandler> get;
					if (MySaver.Handlers.TryGetValue(childComponent.GetType(), out get)){
						try{
							var handler = get();
							PersistentComponent data = new PersistentComponent();
							if ( persistent.persistentChildComponents.TryGetValue( childComponent.GetType().ToString() , out data ) )
								handler.LoadData(childComponent, data);
						} catch {
							Debug.Log("Problem configuring child component "+lastChildComponent);
						}
					}
				} else {
					Debug.Log("could not resolve child or component on load");
					Debug.Log(childComponent.GetType().ToString() + " " + persistentChild.parentObject);
				}
			}

		}
		return rootObject;
	}
}



public class ReferenceResolver{
	public  Dictionary<Persistent, GameObject> persistentObjects = new Dictionary<Persistent, GameObject>();
	public  Dictionary<GameObject, int> objectIDs = new Dictionary<GameObject ,int>();
	private Dictionary<Persistent, List<Persistent> > referenceTree = new Dictionary<Persistent, List<Persistent>>();
	
	public int ResolveReference(GameObject referent, Persistent requester){
		int returnID = -1;
		if (objectIDs.ContainsKey(referent))
			returnID = objectIDs[referent];
		if (!referenceTree.ContainsKey(requester))
			referenceTree.Add(requester,new List<Persistent>());
		referenceTree[requester].Add(persistentObjects.FindKeyByValue(referent));
		return returnID;
	}
	

	public List<Persistent> RetrieveReferenceTree(GameObject target){
		Persistent targetPersistent = null;
		List<Persistent> tree = new List<Persistent>();
		if ( persistentObjects.ContainsValue(target) )
			targetPersistent = persistentObjects.FindKeyByValue( target );

		tree.Add(targetPersistent);

		if ( referenceTree.ContainsKey(targetPersistent) ){

			bool refCheck = true;
			int checkIterations = 0;

			while( refCheck && checkIterations < 7){
				refCheck = false;
				
				List<Persistent> nextTree = new List<Persistent>(tree);
				foreach (Persistent persistent in tree){
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