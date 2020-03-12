using UnityEngine;
using System.Collections.Generic;

public class Intrinsics : MonoBehaviour, ISaveable {
    public Intrinsics parent;
    public List<Buff> buffs = new List<Buff>();
    public List<Buff> liveBuffs = new List<Buff>();
    public Dictionary<Component, Intrinsics> children = new Dictionary<Component, Intrinsics>();
    public Dictionary<BuffType, GameObject> intrinsicFX;
    public void AddChild(Component owner, Intrinsics donor) {
        children[owner] = donor;
        donor.parent = this;
        IntrinsicsChanged();
    }
    public void RemoveChild(Component owner) {
        if (children.ContainsKey(owner)) {
            children[owner].parent = null;
            children.Remove(owner);
            IntrinsicsChanged();
        }
    }
    public void CreateLiveBuffs(List<Buff> newBuffs) {
        foreach (Buff b in newBuffs) {
            AddNewLiveBuff(new Buff(b));
        }
        IntrinsicsChanged();
    }
    public void CreatePromotedLiveBuffs(List<Buff> newBuffs) {
        foreach (Buff b in newBuffs) {
            AddNewPromotedLiveBuff(new Buff(b));
        }
        IntrinsicsChanged();
    }
    public void AddNewLiveBuff(Buff newBuff) {
        foreach (Buff buff in liveBuffs) {
            if (buff.type == newBuff.type) {
                // do stuff
                buff.floatValue = Mathf.Max(buff.floatValue, newBuff.floatValue);
                buff.time = Mathf.Min(buff.time, newBuff.time);
                buff.boolValue = buff.boolValue || newBuff.boolValue;
                return;
            }
        }
        liveBuffs.Add(newBuff);
    }
    public void AddNewPromotedLiveBuff(Buff newBuff) {
        newBuff.lifetime = 0;
        newBuff.time = 0;
        foreach (Buff buff in liveBuffs) {
            if (buff.type == newBuff.type) {
                // do stuff
                buff.floatValue = Mathf.Max(buff.floatValue, newBuff.floatValue);
                buff.time = 0;
                buff.lifetime = 0;
                buff.boolValue = true;
                return;
            }
        }
        liveBuffs.Add(newBuff);
    }
    public void Update() {
        if (parent == null) {
            DoUpdate();
        }
    }
    public void DoUpdate() {
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
        foreach (KeyValuePair<Component, Intrinsics> kvp in children) {
            kvp.Value.DoUpdate();
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
                case BuffType.undead:
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
                        FadeAlpha fader = gameObject.GetComponent<FadeAlpha>();
                        Destroy(fader);
                    }
                    break;
                case BuffType.invulnerable:
                    if (kvp.Value.active()) {
                        Transform head = transform.Find("head");
                        GameObject halo = GameObject.Instantiate(Resources.Load("particles/halo")) as GameObject;
                        if (head) {
                            halo.transform.SetParent(head, false);
                            halo.transform.localPosition = Vector3.zero;
                        } else {
                            halo.transform.SetParent(transform, false);
                            halo.transform.localPosition = Vector3.zero;
                        }
                    } else {
                        Transform head = transform.Find("head");
                        Transform halo = transform.Find("halo(Clone)");
                        if (head) {
                            halo = head.Find("halo(Clone)");
                        }
                        if (halo != null)
                            Destroy(halo.gameObject);
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
        foreach (KeyValuePair<Component, Intrinsics> kvp in children) {
            returnBuffs.AddRange(kvp.Value.NetBuffs().Values);
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
            // netBuffs[buff.type].boolValue |= buff.boolValue;
            // netBuffs[buff.type].floatValue += buff.floatValue;
            netBuffs[buff.type] = netBuffs[buff.type] + buff;
        }
        return netBuffs;
    }
    public void IntrinsicsChanged() {
        // Debug.Log(gameObject.name+"> livebuffs: "+liveBuffs.Count.ToString()+", childs: "+childBuffs.Count.ToString());
        MessageNetIntrinsic message = new MessageNetIntrinsic(this);
        Toolbox.Instance.SendMessage(gameObject, this, message, sendUpwards: false);
        if (GameManager.Instance.playerObject == gameObject) {
            GameManager.Instance.FocusIntrinsicsChanged(this);
        }
        SetBuffFX();
        if (parent != null) {
            parent.IntrinsicsChanged();
        }
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
        if (data.buffs.Count > 0)// || blessed)
            IntrinsicsChanged();
    }
}