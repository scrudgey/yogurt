using UnityEngine;
using System.Collections.Generic;
// using System.Reflection;
public class Intrinsics : MonoBehaviour {
	public List<Intrinsic> intrinsics = new List<Intrinsic>();
	public List<Intrinsic> AddIntrinsic(Intrinsics i){
		foreach(Intrinsic intrinsic in i.intrinsics){
			intrinsics.Add(intrinsic);
		}
		IntrinsicsChanged();
		return i.intrinsics;
	}
	public void RemoveIntrinsic(Intrinsics i){
		foreach(Intrinsic removeThis in i.intrinsics){
			foreach(Intrinsic intrinsic in intrinsics){
				if (intrinsic.Equals(removeThis)){
					intrinsics.Remove(intrinsic);
					break;
				}
			}
		}
		IntrinsicsChanged();
	}
	public Intrinsic NetIntrinsic(){
		Intrinsic netIntrinsic = new Intrinsic();
		foreach(Intrinsic i in intrinsics){
			netIntrinsic.telepathy.boolValue = netIntrinsic.telepathy.boolValue || i.telepathy.boolValue;
			netIntrinsic.fireproof.boolValue = netIntrinsic.fireproof.boolValue || i.fireproof.boolValue;
			netIntrinsic.noPhysicalDamage.boolValue = netIntrinsic.noPhysicalDamage.boolValue || i.noPhysicalDamage.boolValue;
			netIntrinsic.armor.floatValue += i.armor.floatValue;
			netIntrinsic.speed.floatValue += i.speed.floatValue;
			netIntrinsic.bonusHealth.floatValue += i.bonusHealth.floatValue;
		}
		return netIntrinsic;
	}
	public void IntrinsicsChanged(){
		Intrinsic net = NetIntrinsic();
		MessageNetIntrinsic message = new MessageNetIntrinsic();
		message.netIntrinsic = net;
		Toolbox.Instance.SendMessage(gameObject, this, message);
		if (GameManager.Instance.playerObject == gameObject)
			GameManager.Instance.FocusIntrinsicsChanged(net);
	}
	// TODO: fix the intrinsic update. A smarter way to zero out the intrinsic when it's timed out.
	// move the timeout and incrementing logic & etc to the bugg itself, duh
	// I could do this by adding complexity to the netintrinsic calculation but that's kind of annoying? unless I abstract it out.
	public void Update(){
		foreach(Intrinsic i in intrinsics){
			i.Update();
		}
	}
}
[System.Serializable]
public class Intrinsic {
	public bool persistent;
	public Buff telepathy = new Buff();
	public Buff speed = new Buff();
	public Buff bonusHealth = new Buff();
	public Buff armor = new Buff();
	public Buff fireproof = new Buff();
	public Buff noPhysicalDamage = new Buff();
	public Buff invulnerable = new Buff();
	public void Update(){
		telepathy.Update();
		speed.Update();
		bonusHealth.Update();
		armor.Update();
		fireproof.Update();
		noPhysicalDamage.Update();
		invulnerable.Update();
	}
	public bool Equals(Intrinsic other){
		bool match = true;
        match &= telepathy.boolValue == other.telepathy.boolValue;
		match &= fireproof.boolValue == other.fireproof.boolValue;
		match &= invulnerable.boolValue == other.invulnerable.boolValue;
		match &= noPhysicalDamage.boolValue == other.noPhysicalDamage.boolValue;
		
		match &= speed.floatValue == other.speed.floatValue;
		match &= bonusHealth.floatValue == other.bonusHealth.floatValue;
		match &= armor.floatValue == other.armor.floatValue;
        return match;
	}
}

[System.Serializable]
public class Buff {
	public bool boolValue;
	public float floatValue;
	public float lifetime;
	public float time;
	public void Update(){
		if (lifetime > 0){
			time += Time.deltaTime;
			if (time > lifetime){
				boolValue = false;
				floatValue = 0;
			}
		}
	}
}