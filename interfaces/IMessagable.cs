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
		value = initVal;
	}
}

public class MessageIntrinsic : Message {
	public Intrinsic netIntrinsic;	
	public Intrinsics addIntrinsic;
	public Intrinsics removeIntrinsic;
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

	// public Occurrence 

	public MessageSpeech () {}

	public MessageSpeech (string phrase){
		this.phrase = phrase;
	}
	public MessageSpeech (string phrase, string swear){
		this.phrase = phrase;
		this.swear = swear;
	}
}

public class MessageDamage : Message {
	public float amount;
	public damageType type;
	public Vector2 force;
	public PhysicalImpact impactor;
	public MessageDamage(){}
	public List<GameObject> responsibleParty = new List<GameObject>();
	public MessageDamage(float amount, damageType type){
		this.amount = amount;
		this.type = type;
	}
}

public class MessageHitstun : Message {
	public bool value;
	public bool doubledOver;
}