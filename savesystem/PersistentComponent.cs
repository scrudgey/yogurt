using System.Collections.Generic;
using UnityEngine;

public class PersistentComponent {
	public string type;
	public SerializableDictionary<string, string> strings =		 new SerializableDictionary<string, string>();
	public SerializableDictionary<string, int> ints = 			 new SerializableDictionary<string, int> ();
	public SerializableDictionary<string, float> floats = 		 new SerializableDictionary<string, float>();
	public SerializableDictionary<string, bool> bools = 		 new SerializableDictionary<string, bool>();
	public SerializableDictionary<string, Vector3> vectors = 	 new SerializableDictionary<string, Vector3>();
	// TODO: initialize lists as necessary
	public List<SerializedKnowledge> knowledgeBase;
	public List<SerializedPersonalAssessment> people;
	public SerializableDictionary<string, SerializedKnowledge> knowledges = new SerializableDictionary<string, SerializedKnowledge>();
	public List<Buff> buffs;
	public List<Commercial> commercials;
	public int id;
	public PersistentComponent(){
		// Needed for XML serialization
	}
	public PersistentComponent(PersistentObject owner){
		id = owner.id;
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