using UnityEngine;

public class LiquidContainer : Interactive, ISaveable {
    public SpriteRenderer liquidSprite;
    public Liquid liquid;
    private float _amount;
    public float amount {
        get {
            return _amount;
        }
        set {
            _amount = value;
            CheckLiquid();
        }
    }
    public float fillCapacity;
    private float spillTimeout = 0.075f;
    protected bool empty;
    public bool lid;
    private bool doSpill = false;
    private float spillSeverity;
    public string initLiquid;
    public string descriptionName;
    public AudioClip[] drinkSounds;
    public bool configured = false;
    void Update() {
        if (spillTimeout > 0) {
            spillTimeout -= Time.deltaTime;
        }
        if (Vector3.Dot(transform.up, Vector3.up) < 0.05 && spillTimeout <= 0 && !lid) {
            Spill();
        }
    }
    virtual public void Awake() {
        if (!configured) {
            configured = true;
            Interaction fillReservoir = new Interaction(this, "Fill", "FillFromReservoir");
            Interaction fillContainer = new Interaction(this, "Fill", "FillFromContainer");
            Interaction drinker = new Interaction(this, "Drink", "Drink");
            drinker.validationFunction = true;
            drinker.playerOnOtherConsent = false;
            drinker.defaultPriority = 5;
            fillContainer.validationFunction = true;
            interactions.Add(drinker);
            interactions.Add(fillContainer);
            interactions.Add(fillReservoir);
            empty = true;
            if (liquidSprite)
                liquidSprite.enabled = false;
            if (initLiquid != "") {
                FillByLoad(initLiquid);
            }
            Toolbox.RegisterMessageCallback<MessageDamage>(this, HandleDamage);
        }
    }
    public void OnEnable() {
        if (liquid != null)
            UpdateIntrinsics(liquid);
    }
    public void UpdateIntrinsics(Liquid l) {
        // TODO: this seems a bit rough
        Intrinsics myIntrinsics = GetComponent<Intrinsics>();
        if (myIntrinsics != null) {
            DestroyImmediate(myIntrinsics);
        }
        Intrinsics intrinsics = Toolbox.GetOrCreateComponent<Intrinsics>(gameObject);
        if (l.buffs.Count > 0) {
            intrinsics.buffs.AddRange(liquid.buffs);
            intrinsics.IntrinsicsChanged();
        }
    }
    public void HandleDamage(MessageDamage message) {
        if (message.type == damageType.physical || message.type == damageType.cutting) {
            if (!lid)
                Spill();
        }
    }
    public void FillFromReservoir(LiquidResevoir l) {
        FillWithLiquid(l.liquid);
        if (l.fillSound != null) {
            Toolbox.Instance.AudioSpeaker(l.fillSound, transform.position);
        }
    }
    public string FillFromReservoir_desc(LiquidResevoir l) {
        string myname = Toolbox.Instance.GetName(gameObject);
        string resname = "reservoir";
        if (l.genericName != "") {
            resname = l.genericName;
        } else {
            resname = Toolbox.Instance.GetName(l.gameObject);
        }
        return "Fill " + myname + " with " + l.liquid.name + " from " + resname;
    }
    public void FillFromContainer(LiquidContainer l) {
        if (l.amount > 0) {
            FillWithLiquid(l.liquid);
            float fill = Mathf.Min(l.amount, fillCapacity);
            l.amount -= fill;
            amount = fill;
        }
    }
    public bool FillFromContainer_Validation(LiquidContainer l) {
        if (l.amount > 0) {
            return true;
        } else {
            return false;
        }
    }
    public string FillFromContainer_desc(LiquidContainer l) {
        string myname = Toolbox.Instance.GetName(gameObject);
        string resname = Toolbox.Instance.GetName(l.gameObject);
        return "Fill " + myname + " with " + l.liquid.name + " from " + resname;
    }
    public void FillWithLiquid(Liquid l) {
        if (amount > 0) {
            l = Liquid.MixLiquids(liquid, l);

            // not ideal to put this here instead of in MixLiquids, but we need monobehavior
            Buff mixedBuff = Liquid.MixPotion(l);
            if (mixedBuff != null) {
                l.buffs.Add(mixedBuff);
                l.name = Liquid.GetName(l);
                // TODO: play special effect
                GameObject.Instantiate(Resources.Load("particles/potionMixEffect"), transform.position, Quaternion.identity);
            }
        }
        liquid = l;
        amount = fillCapacity;
        UpdateIntrinsics(l);
        CheckLiquid();
    }
    public void FillByLoad(string type) {
        liquid = Liquid.LoadLiquid(type);
        amount = fillCapacity;
        UpdateIntrinsics(liquid);
        CheckLiquid();
    }
    private void CheckLiquid() {
        if (liquid != null && amount > 0 && liquidSprite != null) {
            if (liquidSprite != null) {
                liquidSprite.enabled = true;
                liquidSprite.color = liquid.color;
            }
        }
        if (amount <= 0) {
            if (liquidSprite)
                liquidSprite.enabled = false;
        }
        if (empty && amount > 0) {
            empty = false;
        }
        if (!empty && amount <= 0) {
            empty = true;
        }
    }
    public void Spill() {
        Spill(0.02f);
    }
    public void Spill(float severity) {
        doSpill = true;
        spillSeverity = severity;
    }
    void FixedUpdate() {
        if (doSpill) {
            doSpill = false;
            if (amount > 0 && spillTimeout <= 0) {
                GameObject droplet = Toolbox.Instance.SpawnDroplet(liquid, spillSeverity, gameObject, 0.02f);
                foreach (Collider2D myCollider in transform.root.GetComponentsInChildren<Collider2D>()) {
                    foreach (Collider2D dropCollider in droplet.transform.root.GetComponentsInChildren<Collider2D>()) {
                        Physics2D.IgnoreCollision(myCollider, dropCollider, true);
                    }
                }
                amount -= 0.25f;
                spillTimeout = 0.075f;
            }
        }
    }
    public void Drink(Eater eater) {
        if (eater) {
            GameObject sip = Instantiate(Resources.Load("prefabs/droplet"), transform.position, Quaternion.identity) as GameObject;
            // sip.AddComponent<MonoLiquid>();
            Liquid.MonoLiquidify(sip, liquid);
            eater.Eat(sip.GetComponent<Edible>());
            amount -= 1f;
            if (drinkSounds.Length > 0) {
                Toolbox.Instance.AudioSpeaker(drinkSounds[Random.Range(0, drinkSounds.Length - 1)], transform.position);
            }
            GameManager.Instance.CheckItemCollection(gameObject, eater.gameObject);
        }
    }
    public bool Drink_Validation(Eater eater) {
        return amount > 0;
    }
    public string Drink_desc(Eater eater) {
        string myname = Toolbox.Instance.GetName(gameObject);
        return "Drink " + liquid.name + " from " + myname;
    }
    void OnGroundImpact(Physical phys) {
        if (!lid)
            Spill();
    }

    public void SaveData(PersistentComponent data) {
        data.floats["fillCapacity"] = fillCapacity;
        data.floats["amount"] = amount;
        data.bools["lid"] = lid;
        if (liquid != null) {
            data.strings["liquid"] = liquid.filename;
        } else {
            data.strings["liquid"] = "";
        }
    }
    // TODO: if i had some nicer methods to handle loading an amount of liquid 
    // i think this would be a little better.
    public void LoadData(PersistentComponent data) {
        if (data.strings["liquid"] != "") {
            FillByLoad(data.strings["liquid"]);
        } else {
            FillByLoad("water");
        }
        fillCapacity = data.floats["fillCapacity"];
        amount = data.floats["amount"];
        lid = data.bools["lid"];
    }
}
