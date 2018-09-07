using UnityEngine;
using System.Collections.Generic;

public class Message {
    public Component messenger;
}
public class MessageAnimation : Message {
    public enum AnimType { 
        none, 
        throwing, 
        swinging, 
        fighting, 
        punching, 
        outfit };
    public AnimType type;
    public bool value;
    public string outfitName = "";
    public MessageAnimation() { }
    public MessageAnimation(AnimType initType, bool initVal) {
        type = initType;
        this.value = initVal;
    }
}
public class MessageDirectable : Message {
    public List<IDirectable> addDirectable = new List<IDirectable>();
    public List<IDirectable> removeDirectable = new List<IDirectable>();
}
public class MessageNetIntrinsic : Message {
    public Intrinsics intrinsics;
    public Dictionary<BuffType, Buff> netBuffs;
    public MessageNetIntrinsic(Intrinsics intrinsics) {
        this.intrinsics = intrinsics;
        this.netBuffs = intrinsics.NetBuffs();
    }
}
public class MessageHead : Message {
    public enum HeadType { none, eating, vomiting, speaking };
    public HeadType type;
    public bool value;
    public Color crumbColor;
}
public class MessageSpeech : Message {
    public string phrase;
    public bool randomSwear;
    public bool randomSpeech;
    public bool sayLine;
    public GameObject swearTarget;
    public GameObject insultTarget;
    public GameObject threatTarget;
    public bool nimrod;
    public EventData eventData;
    public bool interrupt;
    public HashSet<GameObject> involvedParties = new HashSet<GameObject>();
    public MessageSpeech() { }
    public MessageSpeech(string phrase, EventData eventData = null) {
        this.phrase = phrase;
        this.eventData = eventData;
    }
}
public class MessageDamage : Message {
    public float amount;
    public damageType type;
    public Vector2 force;
    public AudioClip[] impactSounds = new AudioClip[0];
    public bool silentImpact;
    public bool impersonal;
    public PhysicalImpact impactor;
    public GameObject responsibleParty;
    public bool strength;
    public MessageDamage() { }
    public MessageDamage(float amount, damageType type) {
        this.amount = amount;
        this.type = type;
    }
}
public class MessageHitstun : Message {
    public Controllable.HitState hitState;
    public bool doubledOver;
    public bool knockedDown;
}
public class MessageInventoryChanged : Message {
    public GameObject dropped;
    public GameObject holding;
}
public class MessagePunch : Message {
}
public class MessageInsult : Message {
}
public class MessageThreaten : Message {

}
public class MessageOccurrence : Message {
    public OccurrenceData data;
    public MessageOccurrence(OccurrenceData data) {
        this.data = data;
    }
}
public class MessageNoise : Message {
    public Vector2 location;
    public MessageNoise(GameObject gameObject){
        this.location = gameObject.transform.position;
    }
}