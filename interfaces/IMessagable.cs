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
        outfit,
        panic
    };
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
    // public bool randomSpeech;
    public bool sayLine;
    public GameObject swearTarget;
    public GameObject insultTarget;
    public GameObject threatTarget;
    public bool nimrod;
    private EventData eventData;
    public bool interrupt;
    public HashSet<GameObject> involvedParties = new HashSet<GameObject>();
    public MessageSpeech() { }
    public MessageSpeech(string phrase) {
        this.phrase = phrase;
    }
    public MessageSpeech(string phrase, EventData data = null) {
        this.phrase = phrase;
        this.eventData = data;
    }
    public OccurrenceSpeech ToOccurrenceSpeech(Nimrod.Grammar grammar) {
        OccurrenceSpeech speechData = new OccurrenceSpeech(eventData);
        if (grammar != null && nimrod) {
            phrase = grammar.Parse(phrase);
            if (phrase == "")
                return null;
        }
        // sswearList = new List<bool>();
        MessagePhrase censoredPhrase = Speech.ProcessDialogue(phrase, ref speechData.swearList);
        // TODO: fix
        speechData.profanity = censoredPhrase.profanity;
        speechData.line = censoredPhrase.phrase;
        if (insultTarget != null) {
            speechData.insult = true;
            speechData.target = insultTarget;
        }
        if (threatTarget != null) {
            speechData.threat = true;
            speechData.target = threatTarget;
        }
        return speechData;
    }
}
public class MessageDamage : Message {
    public float amount;
    public damageType type;
    public Vector3 force;
    public AudioClip[] impactSounds = new AudioClip[0];
    public bool suppressImpactSound;
    public bool impersonal;
    public PhysicalImpact impactor;
    public GameObject responsibleParty;
    public string weaponName;
    public bool strength;
    public float angleAboveHorizontal;
    public bool suppressGibs;
    public MessageDamage() { }
    public MessageDamage(float amount, damageType type) {
        this.amount = amount;
        this.type = type;
    }
    public MessageDamage(MessageDamage other) {
        this.amount = other.amount;
        this.type = other.type;
        this.force = other.force;
        this.impactSounds = other.impactSounds;
        this.suppressImpactSound = other.suppressImpactSound;
        this.impersonal = other.impersonal;
        this.impactor = other.impactor;
        this.responsibleParty = other.responsibleParty;
        this.strength = other.strength;
        this.angleAboveHorizontal = other.angleAboveHorizontal;
        this.weaponName = other.weaponName;
    }
}
public class MessageHitstun : Message {
    // Message for passing hitstun state internally between components
    public Controllable.HitState hitState;
    public bool doubledOver;
    public bool knockedDown;
}
public class MessageStun : Message {
    // Message for stunning a target for amount of time timout
    public float timeout;
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
    public MessageNoise(GameObject gameObject) {
        this.location = gameObject.transform.position;
    }
}
public class MessageOnCamera : Message {
    public bool value;
    public MessageOnCamera(bool value) {
        this.value = value;
    }
}
public class MessageSmoothieOrder : Message {
    public int idn;
    public MessageSmoothieOrder(int idn) {
        this.idn = idn;
    }
}