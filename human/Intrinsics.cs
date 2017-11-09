using UnityEngine;
using System.Collections.Generic;

public enum BuffType{telepathy, speed, bonusHealth, armor, fireproof, noPhysicalDamage, invulnerable, strength}

public class Intrinsics : MonoBehaviour {
	public List<Intrinsic> intrinsics = new List<Intrinsic>();
	public List<Intrinsic> AddIntrinsic(Intrinsics ins, bool timeout=true){
		foreach(Intrinsic i in ins.intrinsics){
			AddIntrinsic(i, timeout:timeout);
		}
		IntrinsicsChanged();
		return intrinsics;
	}
	public void AddIntrinsic(Intrinsic i, bool timeout=true){
		Intrinsic intrinsicCopy = new Intrinsic(i);
		if (timeout){
			foreach(Buff buff in intrinsicCopy.buffs.Values){
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
			foreach(BuffType key in i.buffs.Keys){
				if (netIntrinsic.buffs.ContainsKey(key)){
					//sum the values
					netIntrinsic.buffs[key].boolValue |= i.buffs[key].boolValue;
					netIntrinsic.buffs[key].floatValue += i.buffs[key].floatValue;
				} else {
					//set values to the given buff
					netIntrinsic.buffs[key].boolValue = i.buffs[key].boolValue;
					netIntrinsic.buffs[key].floatValue = i.buffs[key].floatValue;
				}
			}
		}
			// netIntrinsic.telepathy.boolValue = netIntrinsic.telepathy.boolValue || i.telepathy.boolValue;
			// netIntrinsic.fireproof.boolValue = netIntrinsic.fireproof.boolValue || i.fireproof.boolValue;
			// netIntrinsic.noPhysicalDamage.boolValue = netIntrinsic.noPhysicalDamage.boolValue || i.noPhysicalDamage.boolValue;
			// netIntrinsic.strength.boolValue = netIntrinsic.strength.boolValue || i.strength.boolValue;
			// netIntrinsic.armor.floatValue += i.armor.floatValue;
			// netIntrinsic.speed.floatValue += i.speed.floatValue;
			// netIntrinsic.bonusHealth.floatValue += i.bonusHealth.floatValue;
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
	public void SetupStatusIcon(){
		Debug.Log("setting up status icons");
		foreach(Intrinsic intrins in intrinsics){
			foreach(BuffType key in intrins.buffs.Keys){
				Debug.Log("creating the new status icon");
				GameObject icon = Instantiate(Resources.Load("UI/StatusIcon")) as GameObject;
				UIStatusIcon statusIcon = icon.GetComponent<UIStatusIcon>();
				statusIcon.Initialize(intrins.buffs[key]);
				UINew.Instance.AddStatusIcon(icon);
				Debug.Log(icon);
				Debug.Break();
			}
		}	
	}
}
[System.Serializable]
public class Intrinsic {
	public bool persistent;
	public SerializableDictionary<BuffType, Buff> buffs = new SerializableDictionary<BuffType, Buff>();
	// public Buff telepathy = new Buff("telepathy");
	// public Buff speed = new Buff("speed");
	// public Buff bonusHealth = new Buff("bonusHealth");
	// public Buff armor = new Buff("armor");
	// public Buff fireproof = new Buff("fireproof");
	// public Buff noPhysicalDamage = new Buff("immune to physical damage");
	// public Buff invulnerable = new Buff("invulnerable");
	// public Buff strength = new Buff("strength");
	public Intrinsic(){	}
	public Intrinsic(Intrinsic otherIntrinsic){
		this.persistent = otherIntrinsic.persistent;
		// List<Buff> buffCopies = new List<Buff>();
		foreach(BuffType key in otherIntrinsic.buffs.Keys){
			buffs[key] = new Buff(otherIntrinsic.buffs[key]);
			// buffCopies.Add(new Buff(buff));
		}
		// SetBuffs(buffCopies);
	}
	// public List<Buff> Buffs(){
	// 	return new List<Buff>(new Buff[]{telepathy, speed, bonusHealth, armor, fireproof, noPhysicalDamage, invulnerable, strength});
	// }
	// public void SetBuffs(List<Buff> buffs){
	// 	telepathy = buffs[0];
	// 	speed = buffs[1];
	// 	bonusHealth = buffs[2];
	// 	armor = buffs[3];
	// 	fireproof = buffs[4];
	// 	noPhysicalDamage = buffs[5];
	// 	invulnerable = buffs[6];
	// 	strength = buffs[7];
	// }
	public bool Update(){
		bool val = false;
		foreach(Buff buff in buffs.Values){
			val = val || buff.Update();
		}
		return val;
	}
	public bool Equals(Intrinsic other){
		bool match = true;
		foreach(BuffType key in buffs.Keys){
			if (other.buffs.ContainsKey(key)){
				match &= buffs[key].boolValue == other.buffs[key].boolValue;
				match &= buffs[key].floatValue == other.buffs[key].floatValue;
			} else {
				match = false;
			}
		}
        // match &= telepathy.boolValue == other.telepathy.boolValue;
		// match &= fireproof.boolValue == other.fireproof.boolValue;
		// match &= invulnerable.boolValue == other.invulnerable.boolValue;
		// match &= noPhysicalDamage.boolValue == other.noPhysicalDamage.boolValue;
		
		// match &= speed.floatValue == other.speed.floatValue;
		// match &= bonusHealth.floatValue == other.bonusHealth.floatValue;
		// match &= armor.floatValue == other.armor.floatValue;
        return match;
	}
	public bool boolValue(BuffType type){
		bool result = false;
		if (buffs.ContainsKey(type)){
			result = buffs[type].boolValue;
		}
		return result;
	}
	public float floatValue(BuffType type){
		float result = 0f;
		if (buffs.ContainsKey(type)){
			result = buffs[type].floatValue;
		}
		return result;
	}
}

[System.Serializable]
public class Buff {
	public bool boolValue;
	public float floatValue;
	public float lifetime;
	public float initLifetime;
	public float time;
	public string name;
	public Buff(){	}
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