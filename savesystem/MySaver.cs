using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Text.RegularExpressions;


public class MySaver {

	public delegate void SaveAction();
	public static event SaveAction OnSave;
	public delegate void LoadAction();
	public static event LoadAction OnPostLoad;
	public enum SaverState{None,Saving,Loading}
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
	};

	public static void CleanupSaves(){
		DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath);
		FileInfo[] fileInfo = info.GetFiles();
		foreach(FileInfo file in fileInfo){
//			Debug.Log(file.FullName);
			File.Delete(file.FullName);
		}
	}
	
	public static void Save(){
		try {
		saveState = SaverState.Saving;
		// open XML serialization stream
		// make this path nicer later when i have a directory structure
		Debug.Log("Persistent data path: "+Application.persistentDataPath);
		string scenePath = Application.persistentDataPath+"/"+Application.loadedLevelName+"_state.xml";
		string playerPath = Application.persistentDataPath+"/player_"+GameManager.Instance.playerObject.name+"_state.xml";
		GameManager.Instance.lastSavedPlayerPath = playerPath;

		FileStream sceneStream = File.Create(scenePath);
		FileStream playerStream = File.Create(playerPath);
		var serializer = new XmlSerializer(typeof(PersistentContainer));
		
		// retrieve all persistent objects
		// theres probably a nice way to do this with linq but what the hell
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
			resolver.objectIDs.Add(gameObject,idIndex);
			resolver.persistentObjects.Add(persistent,gameObject);
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
			sceneIDs.Add( resolver.persistentObjects[persistent], resolver.objectIDs[ resolver.persistentObjects[persistent] ] );
		}
		sceneResolver.objectIDs = sceneIDs;
		foreach (Persistent persistenet in sceneTree)
			persistenet.HandleSave(sceneResolver);

		PersistentContainer sceneContainer = new PersistentContainer(sceneTree);
		PersistentContainer playerContainer = new PersistentContainer(playerTree);

		// save the persistent object container
		serializer.Serialize(sceneStream,sceneContainer);
		serializer.Serialize(playerStream,playerContainer);
		
		// close the XML serialization stream
		sceneStream.Close();
		playerStream.Close();

		// call the save event
		if (OnSave != null)
			OnSave();
		} catch{
			Debug.Log("Problem saving!");
		}
		
		saveState = SaverState.None;
	}

	public static void LoadScene(){
		try {
		saveState = SaverState.Loading;
		string scenePath = Application.persistentDataPath+"/"+Application.loadedLevelName+"_state.xml";
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
		
//		Debug.Log("checking scene path at "+scenePath);
		if (File.Exists(scenePath)){
//			Debug.Log("found "+scenePath);
			var sceneStream = new FileStream(scenePath,FileMode.Open);
			PersistentContainer sceneContainer = serializer.Deserialize(sceneStream) as PersistentContainer;
			sceneStream.Close();
			LoadPersistentContainer(sceneContainer);
		}
		
//		Debug.Log("checking player path at "+playerPath);
		if (File.Exists(playerPath)){
//			Debug.Log("found "+playerPath);
			var playerStream = new FileStream(playerPath,FileMode.Open);
			PersistentContainer playerContainer = serializer.Deserialize(playerStream) as PersistentContainer;
			playerStream.Close();
			LoadPersistentContainer(playerContainer);
		} else {
			// put some default behavior in here
		}

		// call the load event
		if (OnPostLoad != null)
			OnPostLoad();
		} catch {
			Debug.Log("problem loading!");
		}
		
		saveState = SaverState.None;
	}

	public static void LoadPersistentContainer(PersistentContainer container){
		
		string lastName = "first";
		string lastComponent = "first";
		try {
			Regex reg =  new Regex("\\s+", RegexOptions.Multiline);
			loadedObjects = new Dictionary<int, GameObject>();
			foreach(Persistent persistent in container.PersistentObjects){
				lastName = persistent.name;
				string path = @"prefabs/"+persistent.name;
				path = reg.Replace(path,"_");
				GameObject go = GameObject.Instantiate(
					Resources.Load(path),
					persistent.transformPosition,
					persistent.transformRotation) as GameObject;
				loadedObjects.Add(persistent.id,go);
				go.BroadcastMessage("LoadInit", SendMessageOptions.DontRequireReceiver);
				go.name = Toolbox.Instance.ScrubText(go.name);
			}
		} catch {
			Debug.Log("Error occurred when instantiating persistent object "+ lastName);
		}


		//		// now that each persistent object is loaded, handle references

		foreach (Persistent persistent in container.PersistentObjects){
			lastName = persistent.name;
//			Debug.Log("configuring "+persistent.name);
			foreach (Component component in loadedObjects[persistent.id].GetComponents<Component>() ){
				lastComponent = component.GetType().ToString();
				Func<SaveHandler> get;
//				Debug.Log("configuring "+component.GetType());
				if ( MySaver.Handlers.TryGetValue(component.GetType(), out get ) ){
					try {
						var handler = get();
						//					// make this call more robust already before it ruins everything, dick!
						PersistentComponent data = persistent.persistentComponents[component.GetType().ToString()];
						//					// load the data into the component using the handler
						handler.LoadData(component,data);
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
					if ( MySaver.Handlers.TryGetValue(childComponent.GetType(), out get ) ){
						try{
							var handler = get();
							//					// make this call more robust already before it ruins everything, dick!
							PersistentComponent data = new PersistentComponent();
							if ( persistent.persistentChildComponents.TryGetValue( childComponent.GetType().ToString() , out data ) )
								handler.LoadData(childComponent,data);
						} catch {
							Debug.Log("Problem configuring child component "+lastChildComponent);
						}

					}
				} else {
					Debug.Log("could not resolve child and component on load");
					Debug.Log(childComponent.GetType().ToString() + " " + persistentChild.parentObject);
				}
			}
		}

	}

}



public class ReferenceResolver{
	
	public  Dictionary<Persistent, GameObject> persistentObjects = new Dictionary<Persistent, GameObject>();
	public  Dictionary<GameObject, int> objectIDs = new Dictionary<GameObject ,int>();
	private Dictionary<Persistent, List<Persistent> > referenceTree = new Dictionary<Persistent, List<Persistent>>();
	
	public int ResolveReference(GameObject referent, Persistent requester){

		int returnID = -1;
		if ( objectIDs.ContainsKey(referent) )
			returnID = objectIDs[referent];

		if ( ! referenceTree.ContainsKey(requester) )
			referenceTree.Add(requester,new List<Persistent>());

		referenceTree[requester].Add( persistentObjects.FindKeyByValue(referent)  );

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