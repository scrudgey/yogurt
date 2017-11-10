using UnityEngine;
using System.Collections.Generic;

public enum BuffType{telepathy, speed, bonusHealth, armor, fireproof, noPhysicalDamage, invulnerable, strength}

public class Intrinsics : MonoBehaviour {
	public List<Intrinsic> intrinsics = new List<Intrinsic>();
	[System.Serializable]
	public struct initBuff{
		public BuffType type;
		public Buff buff;
	}
	public List<initBuff> initialBuffs = new List<initBuff>();
	void Awake(){
		Intrinsic initIntrinsic = new Intrinsic();
		intrinsics.Add(initIntrinsic);
		foreach(initBuff i in initialBuffs){
			initIntrinsic.buffs[i.type] = new Buff(i.buff);
		}
	}
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
		// intrinsicCopy.persistent = true;
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
					netIntrinsic.buffs[key] = new Buff();
					netIntrinsic.buffs[key].boolValue = i.buffs[key].boolValue;
					netIntrinsic.buffs[key].floatValue = i.buffs[key].floatValue;
				}
			}
		}
		return netIntrinsic;
	}
	public void IntrinsicsChanged(){
		Intrinsic net = NetIntrinsic();
		MessageNetIntrinsic message = new MessageNetIntrinsic();
		message.netIntrinsic = net;
		Toolbox.Instance.SendMessage(gameObject, this, message);
		if (GameManager.Instance.playerObject == gameObject){
			GameManager.Instance.FocusIntrinsicsChanged(this);
		}
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
		foreach(Intrinsic intrins in intrinsics){
			foreach(BuffType key in intrins.buffs.Keys){
				if (intrins.buffs[key].boolValue == true || intrins.buffs[key].floatValue > 0){
					GameObject icon = Instantiate(Resources.Load("UI/StatusIcon")) as GameObject;
					UIStatusIcon statusIcon = icon.GetComponent<UIStatusIcon>();
					statusIcon.Initialize(key, intrins.buffs[key]);
					UINew.Instance.AddStatusIcon(icon);
				}
			}
		}	
	}
}
[System.Serializable]
public class Intrinsic {
	// public bool persistent;
	public SerializableDictionary<BuffType, Buff> buffs = new SerializableDictionary<BuffType, Buff>();	public Intrinsic(){	}
	public Intrinsic(Intrinsic otherIntrinsic){
		// this.persistent = otherIntrinsic.persistent;
		foreach(BuffType key in otherIntrinsic.buffs.Keys){
			buffs[key] = new Buff(otherIntrinsic.buffs[key]);
		}
	}
	public bool Update(){
		bool val = false;
		List<BuffType> removeBuffs = new List<BuffType>();
		foreach(KeyValuePair<BuffType, Buff> kvp in buffs){
			val = val || kvp.Value.Update();
			if (kvp.Value.boolValue == false && kvp.Value.floatValue == 0)
				removeBuffs.Add(kvp.Key);
		}
		foreach(BuffType type in removeBuffs){
			buffs.Remove(type);
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
	public Intrinsic intrinsic;
	public Buff(){}
	// public Buff(BuffType type){
	// 	this.type = type;
	// }
	public Buff(Buff otherBuff){
		// this.intrinsic = intrinsic;
		// this.type = type;
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