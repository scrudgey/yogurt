using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;

public class PersistentObject {
	private static Regex regexClone = new Regex(@"(.+)\(Clone\)$", RegexOptions.Multiline);
	private static Regex regexSpace =  new Regex("\\s+", RegexOptions.Multiline);
	public string name;
	public string prefabPath;
	public bool noPrefab;
	public bool childObject;
	public int id;
	public Vector3 transformPosition;
	public Vector3 transformScale;
	public Quaternion transformRotation;
	public SerializableDictionary<string, PersistentComponent> persistentComponents = new SerializableDictionary<string, PersistentComponent>();
	public List<PersistentObject> persistentChildren = new List<PersistentObject>();
	public string parentObject;
	public int creationDate;
	public string sceneName;
	[XmlIgnoreAttribute]	
	public PersistentObject parentPersistent;
	public PersistentObject(){
		// needed for XML serialization
	}
	public void Update(GameObject gameObject){
		transformPosition = gameObject.transform.position;
		transformRotation = gameObject.transform.rotation;
		transformScale = gameObject.transform.localScale;
		sceneName = SceneManager.GetActiveScene().name;
	}
	public PersistentObject(GameObject gameObject){
		id = MySaver.NextIDNumber();
		MySaver.objectDataBase[id] = this;
		creationDate = GameManager.Instance.data.days;

		MatchCollection matches = regexClone.Matches(gameObject.name);
		if (matches.Count > 0){									// the object is a clone, capture just the normal name
			foreach (Match match in matches){
				name = match.Groups[1].Value;
			}
		} else {												// not a clone
			name = gameObject.name;
		}
		// set up the persistent transform
		Update(gameObject);
		MyMarker marker = gameObject.GetComponent<MyMarker>();
		if (marker != null){
			foreach (GameObject childObject in marker.persistentChildren){
				PersistentObject persistentChildObject = new PersistentObject(childObject);
				persistentChildObject.parentObject = childObject.name;
				persistentChildren.Add(persistentChildObject);
				persistentChildObject.childObject = true;
			}
		}
		prefabPath = @"prefabs/"+name;
		prefabPath = regexSpace.Replace(prefabPath, "_");
		if (Resources.Load(prefabPath) == null){
			noPrefab = true;
			name = gameObject.name;
		}
		foreach (Component component in gameObject.GetComponents<Component>()){
			if (component is ISaveable){
				PersistentComponent persist = new PersistentComponent(this);
				persistentComponents[component.GetType().ToString()] = persist;
			}
		}
	}
	public void HandleSave(GameObject parentObject){
		foreach (Component component in parentObject.GetComponents<Component>()){
			ISaveable saveable = component as ISaveable;
			if (saveable != null){
				// TODO: update each component, don't override.
				// handler.SaveData(component, persistentComponents[component.GetType().ToString()]);
				saveable.SaveData(persistentComponents[component.GetType().ToString()]);
			}
		}
		foreach (PersistentObject persistentChild in persistentChildren){
			if (persistentChild == this)
				continue;
			if (parentPersistent == null){
				persistentChild.parentPersistent = this;
			} else {
				persistentChild.parentPersistent = parentPersistent;
			}
			persistentChild.HandleSave(parentObject.transform.Find(persistentChild.parentObject).gameObject);
		}
	}
	public void HandleLoad(GameObject parentObject){
		List<Component> loadedComponents = new List<Component>(parentObject.GetComponents<Component>());
		loadedComponents.Sort(MySaver.CompareComponent);
		foreach (Component component in loadedComponents){
			ISaveable saveable = component as ISaveable;
			if (saveable != null){
				saveable.LoadData(persistentComponents[component.GetType().ToString()]);
			}
		}
		foreach (PersistentObject persistentChild in persistentChildren){
			if (persistentChild == this)
				continue;
			GameObject childObject = parentObject.transform.Find(persistentChild.parentObject).gameObject;
			persistentChild.HandleLoad(childObject);
		}
	}
}
