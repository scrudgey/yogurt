using UnityEngine;
// using System;
using System.Text.RegularExpressions;

public class PersistentObject {
	private Regex rgx = new Regex(@"(.+)\(Clone\)$", RegexOptions.Multiline);
	public string name;
	public int id;
	public Vector3 transformPosition;
	public Vector3 transformScale;
	public Quaternion transformRotation;
	public SerializableDictionary<string, PersistentComponent> persistentComponents = new SerializableDictionary<string, PersistentComponent>();
	public SerializableDictionary<string, PersistentComponent> persistentChildComponents = new SerializableDictionary<string, PersistentComponent>();
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
		// here i will add a persistentcomponent object for each component that has a handler
		// foreach (Component component in gameObject.GetComponents<Component>() ){
		// 	if (MySaver.Handlers.ContainsKey(component.GetType())){
		// 		PersistentComponent persist = new PersistentComponent(this);
		// 		persistentComponents.Add(component.GetType().ToString(), persist);
		// 	}
		// }
		// handle marked child objects
		MyMarker marker = gameObject.GetComponent<MyMarker>();
		foreach (GameObject childObject in marker.persistentChildren){
			foreach (Component component in childObject.GetComponents<Component>()){
				if (MySaver.Handlers.ContainsKey(component.GetType())){
					PersistentComponent persist = new PersistentComponent(this);
					persistentChildComponents.Add(component.GetType().ToString(), persist);
					persist.parentObject = component.gameObject.name;
					persist.type = component.GetType().ToString();
				}
			}
		}
	}
	public void HandleSave(GameObject parentObject, ReferenceResolver resolver){
		foreach (Component component in parentObject.GetComponents<Component>() ){
			// Func<SaveHandler> getter;
			SaveHandler handler;
			// if (MySaver.Handlers.TryGetValue(component.GetType(), out getter)){
			// 	PersistentComponent persistentComponent = persistentComponents[component.GetType().ToString()];
			// 	var handler = getter();
			// 	handler.SaveData(component, persistentComponent, resolver);
			if (MySaver.Handlers.TryGetValue(component.GetType(), out handler)){
				PersistentComponent persist = new PersistentComponent(this);
				persistentComponents.Add(component.GetType().ToString(), persist);

				SaveHandler secondHandler = MySaver.Handlers[component.GetType()];
				PersistentComponent persistentComponent = persistentComponents[component.GetType().ToString()];
				secondHandler.SaveData(component, persistentComponent, resolver);
			}
		}
		foreach (PersistentComponent persistentChildComponent in persistentChildComponents.Values){
			GameObject childObject = parentObject.transform.Find(persistentChildComponent.parentObject).gameObject;
			Component component = childObject.GetComponent(persistentChildComponent.type);
			if (childObject && component){
				// Func<SaveHandler> get;
				SaveHandler handler;
				if (MySaver.Handlers.TryGetValue(component.GetType(), out handler)){
					// var handler = get();
					handler.SaveData(component, persistentChildComponent, resolver);
				}
			} else {
				Debug.Log("couldn't resolve child object and component on save");
				Debug.Log(persistentChildComponent.type + " " + persistentChildComponent.parentObject);
			}
		}
	}
}
