using UnityEngine;
// using System.Collections;

public interface IMessagable
{
	void ReceiveMessage(Message message);
}

public class Message {
	public Component messenger;
}

public class MessageAnimation : Message {
	public enum AnimType {none, holding, throwing, swinging, fighting, punching};
	public AnimType type;
	public bool value;
	public MessageAnimation(){}
	public MessageAnimation(AnimType initType, bool initVal){
		type = initType;
		value = initVal;
	}
}