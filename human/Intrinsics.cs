using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class Intrinsics : MonoBehaviour, IMessagable {

	public List<Intrinsic> intrinsics = new List<Intrinsic>();
	public void AddIntrinsic(Intrinsics i){
		foreach(Intrinsic intrinsic in i.intrinsics){
			intrinsics.Add(intrinsic);
		}
		IntrinsicsChanged();
	}

	public void RemoveIntrinsic(Intrinsics i){
		foreach(Intrinsic intrinsic in i.intrinsics){
			intrinsics.Remove(intrinsic);
		}
		IntrinsicsChanged();
	}

	public void ReceiveMessage(Message incoming){
		if (incoming is MessageIntrinsic){
			MessageIntrinsic intrins = (MessageIntrinsic)incoming;
			if (intrins.addIntrinsic && intrins.addIntrinsic != this){
				AddIntrinsic(intrins.addIntrinsic);
			}
			if (intrins.removeIntrinsic && intrins.removeIntrinsic != this){
				RemoveIntrinsic(intrins.removeIntrinsic);
			}
		}
	}

	public Intrinsic NetIntrinsic(){
		Intrinsic netIntrinsic = new Intrinsic();
		foreach(Intrinsic i in intrinsics){
			netIntrinsic.telepathy.boolValue = netIntrinsic.telepathy.boolValue || i.telepathy.boolValue;
			netIntrinsic.armor.floatValue += i.armor.floatValue;
			netIntrinsic.speed.floatValue += i.speed.floatValue;
			netIntrinsic.bonusHealth.floatValue += i.bonusHealth.floatValue;
		}
		return netIntrinsic;
	}

	public void IntrinsicsChanged(){
		Intrinsic net = NetIntrinsic();
		MessageIntrinsic message = new MessageIntrinsic();
		message.netIntrinsic = net;
		Toolbox.Instance.SendMessage(gameObject, this, message);
		if (GameManager.Instance.playerObject == gameObject)
			GameManager.Instance.FocusIntrinsicsChanged(net);
	}


	// TODO: fix the intrinsic update. A smarter way to zero out the intrinsic when it's timed out.
	// I could do this by adding complexity to the netintrinsic calculation but that's kind of annoying? unless I abstract it out.
	public void Update(){
		foreach(Intrinsic i in intrinsics){
			const BindingFlags flags = /*BindingFlags.NonPublic | */BindingFlags.Public | 
				BindingFlags.Instance | BindingFlags.Static;
			FieldInfo[] fields = typeof(Intrinsic).GetFields(flags);
			foreach(FieldInfo field in fields){
				if (field.FieldType == typeof(Buff)){
					Buff buff = field.GetValue(i) as Buff;
					if (buff.lifetime > 0){
						buff.time += Time.deltaTime;
						if (buff.time > buff.lifetime){
							buff.boolValue = false;
							buff.floatValue = 0;
						}
					}
				}
			}
		}
	}

}


[System.Serializable]
public class Intrinsic {
	public Buff telepathy = new Buff();
	public Buff speed = new Buff();
	public Buff bonusHealth = new Buff();
	public Buff armor = new Buff();
	public Buff fireproof = new Buff();
	public Buff no_physical_damage = new Buff();
	public Buff invulnerable = new Buff();
}

[System.Serializable]
public class Buff {
	public bool boolValue;
	public float floatValue;
	public float lifetime;
	public float time;
}