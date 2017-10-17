using UnityEngine;
using System.Collections.Generic;
public class Intrinsics : MonoBehaviour {
	public List<Intrinsic> intrinsics = new List<Intrinsic>();
	public List<Intrinsic> AddIntrinsic(Intrinsics ins, bool timeout=true){
		foreach(Intrinsic i in ins.intrinsics){
			// Intrinsic intrinsicCopy = new Intrinsic(i);
			// if (timeout){
			// 	foreach(Buff buff in intrinsicCopy.Buffs()){
			// 		if (buff.initLifetime != 0){
			// 			buff.lifetime = buff.initLifetime;
			// 		}
			// 	}
			// }
			// intrinsicCopy.persistent = true;
			// intrinsics.Add(intrinsicCopy);
			AddIntrinsic(i, timeout:timeout);
		}
		IntrinsicsChanged();
		return intrinsics;
	}
	public void AddIntrinsic(Intrinsic i, bool timeout=true){
		Intrinsic intrinsicCopy = new Intrinsic(i);
		if (timeout){
			foreach(Buff buff in intrinsicCopy.Buffs()){
				if (buff.initLifetime != 0){
					buff.lifetime = buff.initLifetime;
				}
			}
		}
		intrinsicCopy.persistent = true;
		intrinsics.Add(intrinsicCopy);
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
			netIntrinsic.strength.boolValue = netIntrinsic.strength.boolValue || i.strength.boolValue;
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
		bool changed = false;
		foreach(Intrinsic i in intrinsics){
			changed = changed || i.Update();
		}
		if (changed){
			IntrinsicsChanged();
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
	public Buff strength = new Buff();
	public Intrinsic(){	}
	public Intrinsic(Intrinsic otherIntrinsic){
		this.persistent = otherIntrinsic.persistent;
		List<Buff> buffCopies = new List<Buff>();
		foreach(Buff buff in otherIntrinsic.Buffs()){
			buffCopies.Add(new Buff(buff));
		}
		SetBuffs(buffCopies);
	}
	public List<Buff> Buffs(){
		return new List<Buff>(new Buff[]{telepathy, speed, bonusHealth, armor, fireproof, noPhysicalDamage, invulnerable, strength});
	}
	public void SetBuffs(List<Buff> buffs){
		telepathy = buffs[0];
		speed = buffs[1];
		bonusHealth = buffs[2];
		armor = buffs[3];
		fireproof = buffs[4];
		noPhysicalDamage = buffs[5];
		invulnerable = buffs[6];
		strength = buffs[7];
	}
	public bool Update(){
		bool val = false;
		foreach(Buff buff in Buffs()){
			val = val || buff.Update();
		}
		return val;
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
	public float initLifetime;
	public float time;
	public Buff(){}
	public Buff(Buff otherBuff){
		this.boolValue = otherBuff.boolValue;
		this.floatValue = otherBuff.floatValue;
		this.lifetime = 0;
		this.initLifetime = otherBuff.initLifetime;
		this.time = 0;
	}
	public bool Update(){
		bool changed = false;
		if (lifetime > 0){
			time += Time.deltaTime;
			if (time > lifetime){
				boolValue = false;
				floatValue = 0;
				changed = true;
			}
		}
		return changed;
	}
}