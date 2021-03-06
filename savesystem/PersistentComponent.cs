﻿using System.Collections.Generic;
using UnityEngine;

public class PersistentComponent {
    public string type;
    public SerializableDictionary<string, string> strings = new SerializableDictionary<string, string>();
    public SerializableDictionary<string, int> ints = new SerializableDictionary<string, int>();
    public SerializableDictionary<string, System.Guid> GUIDs = new SerializableDictionary<string, System.Guid>();
    public SerializableDictionary<string, float> floats = new SerializableDictionary<string, float>();
    public SerializableDictionary<string, bool> bools = new SerializableDictionary<string, bool>();
    public SerializableDictionary<string, Vector3> vectors = new SerializableDictionary<string, Vector3>();
    public SerializableDictionary<string, Liquid> liquids = new SerializableDictionary<string, Liquid>();
    // TODO: initialize lists as necessary
    public List<SerializedKnowledge> knowledgeBase;
    public List<SerializedPersonalAssessment> people;
    public SerializableDictionary<string, SerializedKnowledge> knowledges = new SerializableDictionary<string, SerializedKnowledge>();
    public List<Buff> buffs = new List<Buff>();
    public List<Commercial> commercials = new List<Commercial>();
    public System.Guid id;
    public PersistentComponent() {
        // Needed for XML serialization
    }
    public PersistentComponent(PersistentObject owner) {
        id = owner.id;
    }
}
[System.Serializable]
public struct SerializedKnowledge {
    public Vector3 lastSeenPosition;
    public float lastSeenTime;
    public System.Guid gameObjectID;
}
[System.Serializable]
public struct SerializedPersonalAssessment {
    public PersonalAssessment.friendStatus status;
    public SerializedKnowledge knowledge;
    public Controllable.HitState HitState;
    public System.Guid gameObjectID;
}