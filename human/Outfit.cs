using UnityEngine;

public class Outfit : Interactive, ISaveable {
    public enum NudityType { normal, fullNude, demogorgon }
    public Gender gender;
    public NudityType nudityType;
    public bool nude;
    public string wornUniformName;
    public string readableUniformName;
    public string pluralUniformType;
    public GameObject initUniform;
    public GameObject uniform;

    public Controllable.HitState hitState;
    public Intrinsics uniformIntrinsics;
    void Awake() {
        Interaction wearInteraction = new Interaction(this, "Wear", "DonUniformWrapper");
        wearInteraction.dontWipeInterface = false;
        wearInteraction.holdingOnOtherConsent = false;
        interactions.Add(wearInteraction);

        Interaction stealInteraction = new Interaction(this, "Take Outfit", "StealUniform");
        stealInteraction.validationFunction = true;
        interactions.Add(stealInteraction);

        Toolbox.RegisterMessageCallback<MessageHitstun>(this, HandleHitStun);
    }
    void HandleHitStun(MessageHitstun message) {
        hitState = message.hitState;
    }
    void Start() {
        if (initUniform != null) {
            // Debug.Log("donning init uniform");
            GameObject uniObject = initUniform;
            if (initUniform.scene.name == null) {
                uniObject = Instantiate(initUniform);
            }
            Uniform uniform = uniObject.GetComponent<Uniform>();
            GameObject removedUniform = DonUniform(uniform, cleanStains: false);
            if (removedUniform)
                Destroy(removedUniform);
        }
    }
    public void StealUniform(Outfit otherOutfit) {
        GameObject uniObject = RemoveUniform();
        Uniform myUniform = uniObject.GetComponent<Uniform>();
        otherOutfit.DonUniform(myUniform);
        GoNude();
    }
    public void GoNude() {
        nude = true;
        initUniform = null;
        uniform = null;
        MessageAnimation anim = new MessageAnimation();

        switch (nudityType) {
            default:
            case NudityType.normal:
                if (gender == Gender.male) {
                    anim.outfitName = "nude";
                } else {
                    anim.outfitName = "nude_female";
                }
                break;
            case NudityType.fullNude:
                anim.outfitName = "nude_demon";
                break;
            case NudityType.demogorgon:
                anim.outfitName = "demogorgon";
                break;
        }
        wornUniformName = "nude";
        readableUniformName = "body";

        Toolbox.Instance.SendMessage(gameObject, this, anim);
    }
    public bool StealUniform_Validation(Outfit otherOutfit) {
        if (otherOutfit == this)
            return false;
        if (nude)
            return false;
        if (hitState >= Controllable.HitState.unconscious)
            return true;
        return false;
    }
    public string StealUniform_desc(Outfit otherOutfit) {
        return "Take " + readableUniformName;
    }
    public void DonUniformWrapper(Uniform uniform) {
        DonUniform(uniform);
    }
    public GameObject DonUniform(Uniform newUniform, bool cleanStains = true) {
        GameObject removedUniform = RemoveUniform();
        PhysicalBootstrapper phys = newUniform.GetComponent<PhysicalBootstrapper>();
        if (phys)
            phys.DestroyPhysical();
        MessageAnimation anim = new MessageAnimation();
        anim.outfitName = newUniform.baseName;
        Toolbox.Instance.SendMessage(gameObject, this, anim);

        if (gameObject == GameManager.Instance.playerObject) {
            EmailTrigger newUniformTrigger = newUniform.GetComponent<EmailTrigger>();
            if (newUniformTrigger != null && newUniformTrigger.triggerType == EmailTrigger.EmailTriggerType.onWear) {
                newUniformTrigger.SendEmail();
            }
        }

        nude = false;
        wornUniformName = Toolbox.Instance.CloneRemover(newUniform.gameObject.name);
        readableUniformName = newUniform.readableName;
        pluralUniformType = newUniform.pluralName;
        GameManager.Instance.CheckItemCollection(newUniform.gameObject, gameObject);
        this.uniform = newUniform.gameObject;
        Toolbox.Instance.AddChildIntrinsics(gameObject, this, newUniform.gameObject);
        newUniform.gameObject.SetActive(false);
        ClaimsManager.Instance.WasDestroyed(newUniform.gameObject);
        if (cleanStains) {
            foreach (Stain stain in GetComponentsInChildren<Stain>()) {
                Destroy(stain.gameObject);
            }
        }
        return removedUniform;
    }
    public string DonUniformWrapper_desc(Uniform uniform) {
        string uniformName = Toolbox.Instance.GetName(uniform.gameObject);
        return "Wear " + uniformName;
    }
    public GameObject RemoveUniform() {
        if (nude)
            return null;
        GameObject removed = null;
        if (uniform != null) {
            removed = uniform;
        } else {
            string prefabName = wornUniformName;
            removed = Instantiate(Resources.Load("prefabs/" + prefabName)) as GameObject;
        }

        Toolbox.Instance.RemoveChildIntrinsics(gameObject, this);
        removed.transform.position = transform.position;
        removed.SetActive(true);
        PhysicalBootstrapper pb = removed.GetComponent<PhysicalBootstrapper>();
        if (pb) {
            pb.InitPhysical(0.02f, Vector3.zero);
        } else {
            Debug.LogWarning($"no physicalbootstrapper on removed outfit? {removed}");
        }
        return removed;
    }
    public void SaveData(PersistentComponent data) {

        if (data.GUIDs.ContainsKey("uniform")) {
            data.GUIDs.Remove("uniform");
        }
        if (uniform != null) {
            MySaver.AddToReferenceTree(gameObject, uniform.gameObject);
            MySaver.UpdateGameObjectReference(uniform.gameObject, data, "uniform");
        }

        data.strings["worn"] = wornUniformName;
        data.ints["hitstate"] = (int)hitState;
        data.ints["gender"] = (int)gender;
        data.bools["nude"] = nude;
    }
    void OnDestroy() {
        if (uniform != null) {
            Destroy(uniform);
        }
    }
    public void LoadData(PersistentComponent data) {
        wornUniformName = data.strings["worn"];
        string wornuniform = data.strings["worn"];
        nude = data.bools["nude"];

        if (data.GUIDs.ContainsKey("uniform")) {
            GameObject go = MySaver.IDToGameObject(data.GUIDs["uniform"]);
            if (go != null) {
                // initUniform = go;
                GameObject removedUniform = DonUniform(go.GetComponent<Uniform>(), cleanStains: false);
                if (removedUniform)
                    Destroy(removedUniform);
            } else {
                Debug.LogError($"{this} could not find saved uniform {data.GUIDs["uniform"]}. Possible lost saved object on the loose!");
            }
            initUniform = null;
        }

        if (nude) {
            GoNude();
        }

        hitState = (Controllable.HitState)data.ints["hitstate"];
        gender = (Gender)data.ints["gender"];
    }
}
