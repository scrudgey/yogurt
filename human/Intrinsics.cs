using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Intrinsics : MonoBehaviour, ISaveable {
    public Intrinsics parent;
    public List<Buff> buffs = new List<Buff>();
    public List<Buff> liveBuffs = new List<Buff>();
    public Dictionary<Component, Intrinsics> children = new Dictionary<Component, Intrinsics>();
    public Dictionary<BuffType, GameObject> intrinsicFX;
    public bool disableStrengthFx = false;
    public void AddChild(Component owner, Intrinsics donor) {
        children[owner] = donor;
        donor.parent = this;
        IntrinsicsChanged();
        // Debug.Log($"{this} added child {owner}");
    }
    public void RemoveChild(Component owner) {
        if (children.ContainsKey(owner)) {
            children[owner].parent = null;
            children.Remove(owner);
            IntrinsicsChanged();
            // Debug.Log($"{this} removed child {owner}");
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
        Transform headTransform = transform.Find("head");
        Head head = null;
        if (headTransform != null) {
            head = headTransform.GetComponent<Head>();
        }
        foreach (KeyValuePair<BuffType, Buff> kvp in NetBuffs()) {
            switch (kvp.Key) {
                case BuffType.undead:
                    UpdateBuffEffect(kvp.Value, "particles/vampire_particles");
                    break;
                case BuffType.strength:
                    if (!disableStrengthFx)
                        UpdateBuffEffect(kvp.Value, "particles/strength_particles");
                    break;
                case BuffType.ethereal:
                    if (kvp.Value.boolValue || kvp.Value.floatValue > 0) {
                        FadeAlpha fader = Toolbox.GetOrCreateComponent<FadeAlpha>(gameObject);

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
                        // Debug.Log($"{this} handling intrinsic invulnerability");
                        // only create halo if invuln. is not the hat
                        // add to head if head exists, else add to me
                        if (!(head != null && children.ContainsKey(head) && children[head].NetBuffs()[BuffType.invulnerable].active())) {
                            Transform haloTransform = transform.Find("halo");
                            if (haloTransform == null) {
                                GameObject halo = GameObject.Instantiate(Resources.Load("particles/halo")) as GameObject;
                                halo.name = "halo";
                                if (head != null) {
                                    halo.transform.SetParent(head.transform, false);
                                } else {
                                    halo.transform.SetParent(transform, false);
                                }
                                halo.transform.localPosition = Vector3.zero;
                            }
                        }
                    } else {

                        // only delete halo if invuln. is not the hat
                        // delete from head if head exists, otherwise delete from me
                        if (!(head != null && children.ContainsKey(head) && children[head].NetBuffs()[BuffType.invulnerable].active())) {
                            Transform[] children = transform.GetComponentsInChildren<Transform>();
                            foreach (var child in children) {
                                if (child == null)
                                    continue;
                                if (child.name.Contains("halo")) {
                                    Debug.Log($"Destroying {child.gameObject}");
                                    DestroyImmediate(child.gameObject);
                                }
                            }
                        }
                    }
                    break;
                case BuffType.polymorph:
                    if (kvp.Value.active()) {
                        PersonRandomizer randomizer = gameObject.GetComponent<PersonRandomizer>();
                        if (randomizer == null) {
                            randomizer = gameObject.AddComponent<PersonRandomizer>();
                        }
                        randomizer.continuous = true;
                        randomizer.configured = false;

                        randomizer.randomizeGender = true;
                        randomizer.randomizeHead = true;
                        randomizer.randomizePersonality = true;
                        randomizer.randomizeSkinColor = true;

                        GameObject prefab = Resources.Load("prefabs/randomPerson full") as GameObject;
                        PersonRandomizer src = prefab.GetComponent<PersonRandomizer>();
                        randomizer.randomOutfits = src.randomOutfits;
                        randomizer.randomFemaleOutfits = src.randomFemaleOutfits;
                        randomizer.maleHeads = src.maleHeads;
                        randomizer.femaleHeads = src.femaleHeads;
                        randomizer.randomVoices = src.randomVoices;
                        randomizer.randomHats = src.randomHats;

                        randomizer.randomHatProbability = 0.1f;
                        randomizer.randomItemProbability = 0f;
                        randomizer.deleteProbability = 0f;

                    } else {
                        foreach (PersonRandomizer randomizer in gameObject.GetComponents<PersonRandomizer>()) {
                            if (randomizer.continuous)
                                randomizer.configured = true;
                        }
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
        if (buff.active()) {
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
        Dictionary<BuffType, Buff> netBuffs = emptyBuffMap();
        foreach (Buff buff in AllBuffs()) {
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
        data.ints["buffCount"] = buffs.Count;
        foreach (Buff b in buffs) {
            data.buffs.Add(new Buff(b));
        }
        foreach (Buff b in liveBuffs) {
            data.buffs.Add(new Buff(b));
        }
    }
    public void LoadData(PersistentComponent data) {
        liveBuffs = new List<Buff>();
        buffs = new List<Buff>();
        foreach (Buff b in data.buffs) {
            if (buffs.Count < data.ints["buffCount"]) {
                buffs.Add(new Buff(b));
            } else {
                liveBuffs.Add(new Buff(b));
            }
        }
        if (data.buffs.Count > 0)
            IntrinsicsChanged();
    }
    public static Dictionary<BuffType, Buff> emptyBuffMap() {
        Dictionary<BuffType, Buff> dict = new Dictionary<BuffType, Buff>();
        foreach (BuffType type in System.Enum.GetValues(typeof(BuffType))) {
            dict[type] = new Buff();
            dict[type].type = type;
        }
        return dict;
    }
}