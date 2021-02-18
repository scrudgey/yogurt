using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Poisson;

[System.Serializable]

public enum ImpactResult { none, damageNormal, damageStrong, damageCosmic, damageOther, damageSilent, repelNormal, repelEthereal, repelInvulnerable, repelOther, repelSilent, repelFireproof }

public enum damageType { physical, fire, any, cutting, piercing, cosmic, asphyxiation, explosion, acid }

public abstract class Damageable : MonoBehaviour {
    public static HashSet<ImpactResult> DamageResults = new HashSet<ImpactResult> {
        ImpactResult.damageNormal, ImpactResult.damageCosmic, ImpactResult.damageStrong, ImpactResult.damageOther, ImpactResult.damageSilent
    };
    public static HashSet<ImpactResult> RepelResults = new HashSet<ImpactResult> {
        ImpactResult.repelNormal, ImpactResult.repelEthereal, ImpactResult.repelInvulnerable, ImpactResult.repelOther, ImpactResult.repelSilent, ImpactResult.repelFireproof
    };
    public Dictionary<BuffType, Buff> netBuffs = Intrinsics.emptyBuffMap();
    public bool immuneToFire;
    public bool immuneToPhysical;
    public AudioClip[] impactSounds;
    public AudioClip[] repelSounds;
    public AudioClip[] strongImpactSounds;
    public AudioClip[] cosmicImpactSounds;
    public AudioClip[] etherealRepelSounds;
    public AudioClip[] invulnerableRepelSounds;
    public MessageDamage lastMessage;
    public GameObject lastAttacker;
    public bool impersonalAttacker;
    public GameObject gibsContainerPrefab;
    new public Rigidbody2D rigidbody2D;
    public Controllable controllable;
    public MessageDamage cacheFiredMessage;
    private float cachedTime;
    private List<Gibs> impactGibs = new List<Gibs>();
    public float blockTextTimer;
    // public static readonly MessageDamage acidDamage = new MessageDamage(2, damageType.acid);
    public virtual void Awake() {
        LoadGibsPrefab();
        if (impactSounds != null && impactSounds.Length == 0)
            impactSounds = Resources.LoadAll<AudioClip>("sounds/impact_normal/");
        if (strongImpactSounds != null && strongImpactSounds.Length == 0)
            strongImpactSounds = Resources.LoadAll<AudioClip>("sounds/impact_strong/");
        if (cosmicImpactSounds != null && cosmicImpactSounds.Length == 0)
            cosmicImpactSounds = Resources.LoadAll<AudioClip>("sounds/impact_cosmic/");

        if (repelSounds != null && repelSounds.Length == 0)
            repelSounds = Resources.LoadAll<AudioClip>("sounds/repel_normal/");
        if (etherealRepelSounds != null && etherealRepelSounds.Length == 0)
            etherealRepelSounds = Resources.LoadAll<AudioClip>("sounds/repel_ethereal/");
        if (invulnerableRepelSounds != null && invulnerableRepelSounds.Length == 0)
            invulnerableRepelSounds = Resources.LoadAll<AudioClip>("sounds/repel_invulnerable/");
        impactGibs = new List<Gibs>();
        foreach (Gibs gib in GetComponents<Gibs>()) {
            if (gib.impactEmitExpectedPer100 > 0)
                impactGibs.Add(gib);
        }
        rigidbody2D = Toolbox.GetOrCreateComponent<Rigidbody2D>(gameObject);
        rigidbody2D.gravityScale = 0;
        controllable = GetComponent<Controllable>();
        Toolbox.RegisterMessageCallback<MessageDamage>(this, HandleMessageDamage);
        Toolbox.RegisterMessageCallback<MessageNetIntrinsic>(this, HandleNetIntrinsic);
    }
    public void LoadGibsPrefab() {
        if (gibsContainerPrefab != null) {
            GameObject gibsContainer = Instantiate(gibsContainerPrefab) as GameObject;
            foreach (Gibs gib in gibsContainer.GetComponents<Gibs>()) {
                Gibs newGib = gameObject.AddComponent<Gibs>();
                newGib.CopyFrom(gib);
            }
            Destroy(gibsContainer);
        }
    }
    public virtual void HandleNetIntrinsic(MessageNetIntrinsic message) {
        netBuffs = message.netBuffs;
        NetIntrinsicsChanged(message);
    }
    public virtual void NetIntrinsicsChanged(MessageNetIntrinsic message) {
        netBuffs = message.netBuffs;
    }
    private void HandleMessageDamage(MessageDamage message) {
        // Debug.Log(gameObject.name + " handling message " + message.type.ToString());
        if (message.type == damageType.fire) {
            cacheFiredMessage = message;
            return;
        } else {
            TakeDamage(message);
        }
    }
    protected virtual void Update() {
        cachedTime += Time.deltaTime;
        if (cachedTime > 0.2f) {
            cachedTime = 0f;
            if (cacheFiredMessage != null) {
                TakeDamage(cacheFiredMessage);
                cacheFiredMessage = null;
            }
            if (netBuffs[BuffType.acidDamage].active()) {
                // health += Time.deltaTime * 10f;

                MessageDamage acidDamage = new MessageDamage(Time.deltaTime * 150f, damageType.acid);
                if (gameObject == GameManager.Instance.playerObject) {
                    acidDamage = new MessageDamage(Time.deltaTime * 400f, damageType.acid);
                }
                TakeDamage(acidDamage);
            }
        }
        if (blockTextTimer > 0) {
            blockTextTimer -= Time.deltaTime;
        }

    }
    public void BlockText(string text, Color color) {
        if (blockTextTimer > 0)
            return;
        blockTextTimer = 5f;
        GameObject blocktext = GameObject.Instantiate(Resources.Load("UI/Blocktext"), transform.position, Quaternion.identity) as GameObject;
        InvulnerabilityText invulnerabilityText = blocktext.GetComponentInChildren<InvulnerabilityText>();
        invulnerabilityText.text.text = text;
        invulnerabilityText.color = color;
    }
    public virtual void TakeDamage(MessageDamage message) {
        if (!enabled)
            return;
        if (message.amount == 0)
            return;

        // TODO: immuneToPhysical is kinda bullshit?
        if (message.type == damageType.physical && immuneToPhysical)
            return;
        if (message.type == damageType.fire && immuneToFire)
            return;

        ImpactResult result = Vulnerable(message, netBuffs);

        AudioClip[] sounds = new AudioClip[0];
        switch (result) {
            case ImpactResult.repelEthereal:
                BlockText("* ethereal *", Color.cyan);
                if (message.type != damageType.fire && message.type != damageType.acid)
                    sounds = etherealRepelSounds;
                break;
            case ImpactResult.repelInvulnerable:
                BlockText("* invulnerable *", Color.yellow);
                if (message.type != damageType.fire && message.type != damageType.acid)
                    sounds = invulnerableRepelSounds;
                break;
            case ImpactResult.repelNormal:
                BlockText("* armor *", Color.white);
                if (message.type != damageType.fire && message.type != damageType.acid)
                    sounds = repelSounds;
                break;
            case ImpactResult.repelFireproof:
                BlockText("* fireproof *", Color.red);
                break;
            // case ImpactResult.repelOther:
            //     sounds = repelSounds;
            //     break;
            case ImpactResult.damageNormal:
                if (message.impactSounds.Length > 0) {
                    sounds = message.impactSounds;
                } else {
                    sounds = impactSounds;
                }
                break;
            case ImpactResult.damageCosmic:
                // TODO: cosmic impact effect
                sounds = cosmicImpactSounds;
                Instantiate(Resources.Load("particles/cosmic_impact"), transform.position, Quaternion.identity);
                break;
            case ImpactResult.damageStrong:
                sounds = strongImpactSounds;
                Instantiate(Resources.Load("particles/explosion1"), transform.position, Quaternion.identity);

                CameraControl cam = GameObject.FindObjectOfType<CameraControl>();
                float distanceToCamera = Vector2.Distance(transform.position, cam.transform.position) * 10;
                float amount = Mathf.Min(0.1f, 0.1f / distanceToCamera);
                cam.Shake(amount);
                break;
            default:
                break;
        }

        // play sound
        if (sounds.Length > 0 && !message.suppressImpactSound) {
            Toolbox.Instance.AudioSpeaker(sounds[Random.Range(0, sounds.Length)], transform.position);
        }
        if (message.messenger != null && message.type != damageType.acid)
            message.messenger.SendMessage("ImpactReceived", result, SendMessageOptions.DontRequireReceiver);

        if (DamageResults.Contains(result)) {
            lastMessage = message;
            if (message.responsibleParty != null) {
                lastAttacker = message.responsibleParty;
            }
            impersonalAttacker = message.impersonal;
            foreach (Gibs gib in impactGibs) {
                // expected number of gibs to emit:
                // damage * ( ratePer100Damage /100)
                double lambda = (message.amount / 100f) * gib.impactEmitExpectedPer100;
                int number = Poisson.Poisson.GetPoisson(lambda);
                if (number > 0)
                    gib.Emit(number, message);
            }
            CalculateDamage(message);

            // UI hit effect if i am the player
            if (gameObject == GameManager.Instance.playerObject && message.type != damageType.asphyxiation) {
                UINew.Instance.Hit();
            }
            // look in the direction 
            if (controllable != null && message.type != damageType.fire && message.type != damageType.asphyxiation) {
                controllable.direction = -1f * message.force;
            }
        }
    }
    public abstract void CalculateDamage(MessageDamage message);
    public virtual void Destruct() {
        if (gameObject.name.Contains("ghost") && SceneManager.GetActiveScene().name == "mayors_attic") {
            GameManager.Instance.data.ghostsKilled += 1;
        }
        if (lastMessage == null) {
            lastMessage = new MessageDamage(0.5f, damageType.physical);
        }
        if (lastMessage.type == damageType.cosmic) {
            GameObject.Instantiate(Resources.Load("particles/cosmic_destruction"), transform.position, Quaternion.identity);
        }
        Intrinsics myIntrinsics = GetComponent<Intrinsics>();
        // TODO: does this ruin undead gibs?
        foreach (Gibs gib in GetComponents<Gibs>())
            gib.Emit(lastMessage);//, intrinsics: myIntrinsics);
        PhysicalBootstrapper phys = GetComponent<PhysicalBootstrapper>();
        if (phys) {
            phys.DestroyPhysical();
        }
        ClaimsManager.Instance.WasDestroyed(gameObject);
        Destroy(gameObject);
        gameObject.SendMessage("OnDestruct", SendMessageOptions.DontRequireReceiver);
    }

    public static ImpactResult Vulnerable(MessageDamage message, Dictionary<BuffType, Buff> netBuffs) {
        if (!message.strength) {
            switch (message.type) {
                case damageType.physical:
                case damageType.cutting:
                case damageType.acid:
                case damageType.piercing:
                    message.amount = Mathf.Max(message.amount - netBuffs[BuffType.armor].floatValue, 0);
                    break;
                default:
                    break;
            }
        }
        ImpactResult result = ImpactResult.none;
        // blocks
        if (netBuffs != null) {
            float armor = netBuffs[BuffType.armor].floatValue;
            bool ethereal = netBuffs[BuffType.ethereal].active();
            bool invulnerable = netBuffs[BuffType.invulnerable].active();
            bool fireproof = netBuffs[BuffType.fireproof].active();
            bool undead = netBuffs[BuffType.undead].active();

            switch (message.type) {
                case damageType.acid:
                    if (message.amount <= 0 || ethereal || invulnerable) {
                        result = ImpactResult.repelSilent;
                    } else result = ImpactResult.damageSilent;
                    break;
                case damageType.explosion:
                case damageType.physical:
                case damageType.cutting:
                case damageType.piercing:
                    if (message.amount <= 0) {
                        result = ImpactResult.repelNormal;
                    }
                    if (ethereal) {
                        result = ImpactResult.repelEthereal;
                    }
                    if (invulnerable) {
                        result = ImpactResult.repelInvulnerable;
                    }
                    break;
                case damageType.cosmic:
                    if (invulnerable) {
                        result = ImpactResult.repelInvulnerable;
                    }
                    break;
                case damageType.fire:
                    if (fireproof) result = ImpactResult.repelFireproof;
                    if (ethereal) {
                        result = ImpactResult.repelEthereal;
                    }
                    if (invulnerable) {
                        result = ImpactResult.repelInvulnerable;
                    }
                    break;
                case damageType.asphyxiation:
                    if (undead) result = ImpactResult.repelOther;
                    break;
                default:
                    break;
            }
        }
        if (!RepelResults.Contains(result)) {
            switch (message.type) {
                case damageType.acid:
                    result = ImpactResult.damageSilent;
                    break;
                case damageType.physical:
                case damageType.cutting:
                case damageType.piercing:
                    result = ImpactResult.damageNormal;
                    break;
                case damageType.cosmic:
                    result = ImpactResult.damageCosmic;
                    break;
                default:
                    result = ImpactResult.damageOther;
                    break;
            }
            if (message.strength) {
                result = ImpactResult.damageStrong;
            }
        }
        return result;
    }
}