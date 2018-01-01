using UnityEngine;
using System.Collections.Generic;

public enum BuffType{telepathy, speed, bonusHealth, armor, fireproof, noPhysicalDamage, invulnerable, strength, poison, vampirism}

public class Intrinsics : MonoBehaviour, ISaveable {
	public List<Buff> buffs = new List<Buff>();
	public List<Buff> liveBuffs = new List<Buff>();
	public HashSet<Intrinsics> childIntrinsics = new HashSet<Intrinsics>();
	public void AddChild(Intrinsics child){
		childIntrinsics.Add(child);
		IntrinsicsChanged();
	}
	public void RemoveChild(Intrinsics child){
		childIntrinsics.Remove(child);
		IntrinsicsChanged();
	}
	public void CreateLiveBuffs(List<Buff> newBuffs){
		foreach(Buff b in newBuffs){
			liveBuffs.Add(new Buff(b));
		}
		IntrinsicsChanged();
	}
	void Update(){
		bool changed = false;
		Stack<Buff> removeBuffs = new Stack<Buff>();
		foreach(Buff buff in liveBuffs){
			changed |= buff.Update();
			if (buff.boolValue == false && buff.floatValue == 0)
				removeBuffs.Push(buff);
		}
		while (removeBuffs.Count > 0){
			liveBuffs.Remove(removeBuffs.Pop());
			changed = true;
		}
		if (changed){
			IntrinsicsChanged();
		}
	}
	public bool boolValue(BuffType type){
		bool result = false;
		foreach(List<Buff> list in new List<Buff>[]{buffs, liveBuffs})
			foreach (Buff buff in list){
				result = result || buff.boolValue;
			}
		return result;
	}
	public float floatValue(BuffType type){
		float result = 0;
		foreach(List<Buff> list in new List<Buff>[]{buffs, liveBuffs})
			foreach (Buff buff in list){
				result = result + buff.floatValue;
			}
		return result;
	}
	public List<Buff> AllBuffs(){
		List<Buff> returnBuffs = new List<Buff>();
		returnBuffs.AddRange(buffs);
		returnBuffs.AddRange(liveBuffs);
		foreach(Intrinsics child in childIntrinsics){
			returnBuffs.AddRange(child.AllBuffs());
		}
		return returnBuffs;
	}
	public Dictionary<BuffType, Buff> NetBuffs(){
		Dictionary<BuffType, Buff> netBuffs = new Dictionary<BuffType, Buff>();
		foreach(BuffType type in System.Enum.GetValues(typeof(BuffType))){
			netBuffs[type] = new Buff();
			netBuffs[type].type = type;
		}
		foreach(Buff buff in AllBuffs()){
			//
			//	THE BUFF ALGEBRA
			//
			netBuffs[buff.type].boolValue |= buff.boolValue;
			netBuffs[buff.type].floatValue += buff.floatValue;
		}
		return netBuffs;
	}
	public void IntrinsicsChanged(){
		MessageNetIntrinsic message = new MessageNetIntrinsic(this);
		Toolbox.Instance.SendMessage(gameObject, this, message);
		if (GameManager.Instance.playerObject == gameObject){
			GameManager.Instance.FocusIntrinsicsChanged(this);
		}
	}
	public void SaveData(PersistentComponent data){
		data.buffs = new List<Buff>();
		foreach(Buff b in liveBuffs){
			data.buffs.Add(b);
		}
	}
	public void LoadData(PersistentComponent data){
		liveBuffs = new List<Buff>();
		foreach(Buff b in data.buffs){
			liveBuffs.Add(b);
		}
		if (data.buffs.Count > 0)
			IntrinsicsChanged();
	}
}
[System.Serializable]
public class Buff {
	public BuffType type;
	public bool boolValue;
	public float floatValue;
	public float lifetime;
	public float time;
	public Buff(){}
	public Buff(Buff otherBuff){
		this.type = otherBuff.type;
		this.boolValue = otherBuff.boolValue;
		this.floatValue = otherBuff.floatValue;
		this.lifetime = otherBuff.lifetime;
		this.time = 0;
	}
	public bool Update(){
		bool changed = false;
		time += Time.deltaTime;
		if (time > lifetime){
			boolValue = false;
			floatValue = 0;
			changed = true;
		} else {
			boolValue = true;
		}
		return changed;
	}
}