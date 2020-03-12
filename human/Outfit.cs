﻿using UnityEngine;

public class Outfit : Interactive, ISaveable {
    public Gender gender;
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
        initUniform = null;
        uniform = null;
        MessageAnimation anim = new MessageAnimation();
        if (gender == Gender.male) {
            anim.outfitName = "nude";
            wornUniformName = "nude";
            readableUniformName = "body";
        } else {
            anim.outfitName = "nude_female";
            wornUniformName = "nude_female";
            readableUniformName = "body";
        }
        Toolbox.Instance.SendMessage(gameObject, this, anim);
    }
    public bool StealUniform_Validation(Outfit otherOutfit) {
        if (otherOutfit == this)
            return false;
        if (wornUniformName == "nude")
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


        wornUniformName = Toolbox.Instance.CloneRemover(newUniform.gameObject.name);
        readableUniformName = newUniform.readableName;
        pluralUniformType = newUniform.pluralName;
        GameManager.Instance.CheckItemCollection(newUniform.gameObject, gameObject);
        // ClaimsManager.Instance.WasDestroyed(uniform.gameObject);
        // Destroy(uniform.gameObject);
        this.uniform = newUniform.gameObject;
        Toolbox.Instance.AddChildIntrinsics(gameObject, this, newUniform.gameObject);
        newUniform.gameObject.SetActive(false);
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
        if (wornUniformName == "nude")
            return null;
        // TODO: change?
        GameObject removed = null;
        if (uniform != null) {
            removed = uniform;
        } else {
            string prefabName = wornUniformName;
            removed = Instantiate(Resources.Load("prefabs/" + prefabName)) as GameObject;
        }

        Toolbox.Instance.RemoveChildIntrinsics(gameObject, this);
        removed.transform.position = transform.position;
        // SpriteRenderer sprite = uniform.GetComponent<SpriteRenderer>();
        // sprite.sortingLayerName = "ground";
        removed.SetActive(true);
        return removed;
    }
    public void SaveData(PersistentComponent data) {

        if (uniform != null) {
            MySaver.AddToReferenceTree(gameObject, uniform.gameObject);
            MySaver.UpdateGameObjectReference(uniform.gameObject, data, "uniform");
        }

        data.strings["worn"] = wornUniformName;
        data.ints["hitstate"] = (int)hitState;
        data.ints["gender"] = (int)gender;
    }
    public void LoadData(PersistentComponent data) {
        wornUniformName = data.strings["worn"];
        string wornuniform = data.strings["worn"];

        if (data.GUIDs.ContainsKey("uniform")) {
            GameObject go = MySaver.IDToGameObject(data.GUIDs["uniform"]);
            if (go != null) {
                // initUniform = go;
                GameObject removedUniform = DonUniform(go.GetComponent<Uniform>(), cleanStains: false);
                if (removedUniform)
                    Destroy(removedUniform);
            }
            initUniform = null;
        }

        if (wornuniform == "nude") {
            GoNude();
        }

        hitState = (Controllable.HitState)data.ints["hitstate"];
        gender = (Gender)data.ints["gender"];
    }
}
