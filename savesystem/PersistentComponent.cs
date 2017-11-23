using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class PersistentComponent {
	public string type;
	// public string parentObject;
	public SerializableDictionary<string, string> strings =		 new SerializableDictionary<string, string>();
	public SerializableDictionary<string, int> ints = 			 new SerializableDictionary<string, int> ();
	public SerializableDictionary<string, float> floats = 		 new SerializableDictionary<string, float>();
	public SerializableDictionary<string, bool> bools = 		 new SerializableDictionary<string, bool>();
	public SerializableDictionary<string, Vector3> vectors = 	 new SerializableDictionary<string, Vector3>();
	// TODO: initialize lists as necessary
	public List<SerializedKnowledge> knowledgeBase;
	public List<SerializedPersonalAssessment> people;
	public SerializableDictionary<string, SerializedKnowledge> knowledges = new SerializableDictionary<string, SerializedKnowledge>();
	public List<Intrinsic> intrinsics;
	public List<Commercial> commercials;
	[XmlIgnoreAttribute]	
	public PersistentObject persistent;
	public PersistentComponent(){
		// Needed for XML serialization
	}
	public PersistentComponent(PersistentObject owner){
		persistent = owner;
	}
}
[System.Serializable]
public struct SerializedKnowledge{
	public Vector3 lastSeenPosition;
	public float lastSeenTime;
	public int gameObjectID;
}
[System.Serializable]
public struct SerializedPersonalAssessment{
	public PersonalAssessment.friendStatus status;
	public SerializedKnowledge knowledge;
	public bool unconscious;
	public int gameObjectID;
}