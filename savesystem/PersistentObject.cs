using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

public class PersistentObject {
	private Regex rgx = new Regex(@"(.+)\(Clone\)$", RegexOptions.Multiline);
	public string name;
	public int id;
	public Vector3 transformPosition;
	public Vector3 transformScale;
	public Quaternion transformRotation;
	public SerializableDictionary<string, PersistentComponent> persistentComponents = new SerializableDictionary<string, PersistentComponent>();
	public List<PersistentObject> persistentChildren = new List<PersistentObject>();
	public string parentObject;
	[XmlIgnoreAttribute]	
	public PersistentObject parentPersistent;

	public PersistentObject(){
		// needed for XML serialization
	}
	public PersistentObject(GameObject gameObject){
		// name
		MatchCollection matches = rgx.Matches(gameObject.name);
		if (matches.Count > 0){									// the object is a clone, capture just the normal name
			foreach (Match match in matches){
				name = match.Groups[1].Value;
			}
		} else {												// not a clone
			name = gameObject.name;
		}
		// set up the persistent transform
		transformPosition = gameObject.transform.position;
		transformRotation = gameObject.transform.rotation;
		transformScale = gameObject.transform.localScale;
		MyMarker marker = gameObject.GetComponent<MyMarker>();
		if (marker != null){
			foreach (GameObject childObject in marker.persistentChildren){
				PersistentObject persistentChildObject = new PersistentObject(childObject);
				persistentChildObject.parentObject = childObject.name;
				persistentChildren.Add(persistentChildObject);
			}
		}
	}
	public void HandleSave(GameObject parentObject){
		foreach (Component component in parentObject.GetComponents<Component>()){
			SaveHandler handler;
			if (MySaver.Handlers.TryGetValue(component.GetType(), out handler)){
				PersistentComponent persist = new PersistentComponent(this);
				persistentComponents.Add(component.GetType().ToString(), persist);
				// Debug.Log(persist.persistent);
				handler.SaveData(component, persist);
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
			SaveHandler handler;
			if (MySaver.Handlers.TryGetValue(component.GetType(), out handler)){
				handler.LoadData(component, persistentComponents[component.GetType().ToString()]);
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
