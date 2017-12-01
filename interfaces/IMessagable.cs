using UnityEngine;
using System.Collections.Generic;

public interface IMessagable
{
	void ReceiveMessage(Message message);
}
public class Message {
	public Component messenger;
}
public class MessageAnimation : Message {
	public enum AnimType {none, holding, throwing, swinging, fighting, punching, outfit};
	public AnimType type;
	public bool value;
	public string outfitName = "";
	public MessageAnimation(){}
	public MessageAnimation(AnimType initType, bool initVal){
		type = initType;
		this.value = initVal;
	}
}
public class MessageDirectable: Message{
	public IDirectable addDirectable;
	public IDirectable removeDirectable;
}
public class MessageNetIntrinsic: Message {
	public Intrinsics intrinsics;
	public Dictionary<BuffType, Buff> netBuffs;
	public MessageNetIntrinsic(Intrinsics intrinsics){
		this.intrinsics = intrinsics;
		this.netBuffs = intrinsics.NetBuffs();
	}
}
public class MessageHead : Message {
	public enum HeadType {none, eating, vomiting, speaking};
	public HeadType type;
	public bool value;
	public Color crumbColor;
}
public class MessageSpeech : Message {
	public string phrase;
	public string swear = "";
	public bool randomSwear;
	public bool randomSpeech;
	public bool sayLine;
	public GameObject swearTarget;
	public bool nimrodKey;
	public EventData eventData;
	public MessageSpeech () {}
	public MessageSpeech (string phrase, string swear=null, EventData eventData = null){
		this.phrase = phrase;
		this.swear = swear;
		this.eventData = eventData;
	}
}
public class MessageDamage : Message {
	public float amount;
	public damageType type;
	public Vector2 force;
	public AudioClip[] impactSounds;
	public PhysicalImpact impactor;
	public GameObject responsibleParty;
	public bool strength;
	public MessageDamage(){}
	public MessageDamage(float amount, damageType type){
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
// public class MessageScript : Message {
// 	public enum TomAction {none, yogurt}
// 	public string watchForSpeech;
// 	public string coStarLine;
// 	public TomAction tomAct;
// }
public class MessageInsult : Message {
}
public class MessageThreaten : Message {	
}
public class MessageOccurrence : Message {
	public OccurrenceData data;
	public MessageOccurrence(OccurrenceData data){
		this.data = data;
	}
}