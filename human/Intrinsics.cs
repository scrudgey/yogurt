using UnityEngine;
using System.Collections.Generic;

public enum BuffType { 
    telepathy, 
    speed, 
    bonusHealth, 
    armor, 
    fireproof, 
    noPhysicalDamage, 
    invulnerable, 
    strength, 
    poison, 
    vampirism, 
    ethereal,  
    coughing
    }

public class Intrinsics : MonoBehaviour, ISaveable {
    public List<Buff> buffs = new List<Buff>();
    public List<Buff> liveBuffs = new List<Buff>();
    // TODO: if we ever need to, we can extend "childBuffs" to be keyed on strings; 
    // think of it as accessible slots
    public Dictionary<Component, List<Buff>> childBuffs = new Dictionary<Component, List<Buff>>();
    public Dictionary<BuffType, GameObject> intrinsicFX;
    public void AddChild(Component owner, Intrinsics donor) {
        childBuffs[owner] = donor.buffs;
        IntrinsicsChanged();
    }
    public void RemoveChild(Component owner) {
        childBuffs.Remove(owner);
        IntrinsicsChanged();
    }
    public void CreateLiveBuffs(List<Buff> newBuffs) {
        foreach (Buff b in newBuffs) {
            AddNewLiveBuff(new Buff(b));
        }
        IntrinsicsChanged();
    }
    public void AddNewLiveBuff(Buff newBuff){
        foreach(Buff buff in liveBuffs){
            if (buff.type == newBuff.type){
                // do stuff
                buff.floatValue = Mathf.Max(buff.floatValue, newBuff.floatValue);
                buff.time = Mathf.Min(buff.time, newBuff.time);
                buff.boolValue = buff.boolValue || newBuff.boolValue;
                return;
            }
        }
        liveBuffs.Add(newBuff);
    }
    void Update() {
        bool changed = false;
        Stack<Buff> removeBuffs = new Stack<Buff>();
        foreach (Buff buff in liveBuffs) {
            changed |= buff.Update();
            if (buff.boolValue == false && buff.floatValue == 0)
                removeBuffs.Push(buff);
        }
        while (removeBuffs.Count > 0) {
            liveBuffs.Remove(removeBuffs.Pop());
            changed = true;
        }
        if (changed) {
            IntrinsicsChanged();
        }
    }
    public void Awake() {
        intrinsicFX = new Dictionary<BuffType, GameObject>();
        foreach (BuffType type in System.Enum.GetValues(typeof(BuffType))) {
            intrinsicFX[type] = null;
        }
    }
    public void Start() {
        SetBuffFX();
        IntrinsicsChanged();
    }
    public void SetBuffFX() {
        foreach (KeyValuePair<BuffType, Buff> kvp in NetBuffs()) {
            switch (kvp.Key) {
                case BuffType.vampirism:
                    UpdateBuffEffect(kvp.Value, "particles/vampire_particles");
                    break;
                case BuffType.strength:
                    UpdateBuffEffect(kvp.Value, "particles/strength_particles");
                    break;
                case BuffType.ethereal:
                    if (kvp.Value.boolValue || kvp.Value.floatValue > 0) {
                        FadeAlpha fader = Toolbox.GetOrCreateComponent<FadeAlpha>(gameObject);
                        Transform head = transform.Find("head");
                        if (head) {
                            fader.spriteRenderers.Add(head.GetComponent<SpriteRenderer>());
                        }
                    } else {

                    }
                    break;
                default:
                    break;
            }
        }

    }
    public void UpdateBuffEffect(Buff buff, string prefabPath) {
        if (intrinsicFX == null)
            return;
        if (buff.boolValue || buff.floatValue > 0) {
            if (intrinsicFX[buff.type] == null) {
                intrinsicFX[buff.type] = Instantiate(Resources.Load(prefabPath), transform.position, Quaternion.identity) as GameObject;
                intrinsicFX[buff.type].transform.SetParent(transform, false);
                intrinsicFX[buff.type].transform.localPosition = Vector3.zero;
            }
        } else {
            if (intrinsicFX.ContainsKey(buff.type))
                if (intrinsicFX[buff.type] != null) {
                    Destroy(intrinsicFX[buff.type]);
                }
        }
    }
    public bool boolValue(BuffType type) {
        bool result = false;
        foreach (List<Buff> list in new List<Buff>[] { buffs, liveBuffs })
            foreach (Buff buff in list) {
                result = result || buff.boolValue;
            }
        return result;
    }
    public float floatValue(BuffType type) {
        float result = 0;
        foreach (List<Buff> list in new List<Buff>[] { buffs, liveBuffs })
            foreach (Buff buff in list) {
                result = result + buff.floatValue;
            }
        return result;
    }
    public List<Buff> AllBuffs() {
        List<Buff> returnBuffs = new List<Buff>();
        returnBuffs.AddRange(buffs);
        returnBuffs.AddRange(liveBuffs);
        foreach (KeyValuePair<Component, List<Buff>> kvp in childBuffs) {
            returnBuffs.AddRange(kvp.Value);
            if (kvp.Value.Count > 0){
                Buff b = kvp.Value[0];
                // Debug.Log(gameObject.name + "> " + kvp.Key.ToString()+" : "+b.floatValue.ToString());
            }
        }
        return returnBuffs;
    }
    public Dictionary<BuffType, Buff> NetBuffs() {
        Dictionary<BuffType, Buff> netBuffs = new Dictionary<BuffType, Buff>();
        foreach (BuffType type in System.Enum.GetValues(typeof(BuffType))) {
            netBuffs[type] = new Buff();
            netBuffs[type].type = type;
        }
        foreach (Buff buff in AllBuffs()) {
            //
            //	THE BUFF ALGEBRA
            //
            netBuffs[buff.type].boolValue |= buff.boolValue;
            netBuffs[buff.type].floatValue += buff.floatValue;
        }
        return netBuffs;
    }
    public void IntrinsicsChanged() {
        // Debug.Log(gameObject.name+"> livebuffs: "+liveBuffs.Count.ToString()+", childs: "+childBuffs.Count.ToString());
        MessageNetIntrinsic message = new MessageNetIntrinsic(this);
        Toolbox.Instance.SendMessage(gameObject, this, message, sendUpwards:false);
        if (GameManager.Instance.playerObject == gameObject) {
            GameManager.Instance.FocusIntrinsicsChanged(this);
        }
        SetBuffFX();
    }
    public void SaveData(PersistentComponent data) {
        data.buffs = new List<Buff>();
        foreach (Buff b in liveBuffs) {
            data.buffs.Add(new Buff(b));
        }
    }
    public void LoadData(PersistentComponent data) {
        liveBuffs = new List<Buff>();
        foreach (Buff b in data.buffs) {
            liveBuffs.Add(new Buff(b));
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
    public Buff() { }
    public Buff(Buff otherBuff) {
        this.type = otherBuff.type;
        this.boolValue = otherBuff.boolValue;
        this.floatValue = otherBuff.floatValue;
        this.lifetime = otherBuff.lifetime;
        this.time = otherBuff.time;
    }
    public bool Update() {
        bool changed = false;
        time += Time.deltaTime;
        if (time > lifetime && lifetime > 0) {
            boolValue = false;
            floatValue = 0;
            changed = true;
        } else {
            boolValue = true;
        }
        return changed;
    }
}