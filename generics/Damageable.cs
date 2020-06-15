using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class ImpactGibs {
    public float probability;
    public Gibs gibs;
}
public enum ImpactResult { none, damageNormal, damageStrong, damageCosmic, damageOther, repelNormal, repelEthereal, repelInvulnerable, repelOther }

public enum damageType { physical, fire, any, cutting, piercing, cosmic, asphyxiation, explosion }
public abstract class Damageable : MonoBehaviour {
    // public static Dictionary<damageType, List<BuffType>> blockedBy = new Dictionary<damageType, List<BuffType>>(){
    //     {damageType.physical, new List<BuffType>(){BuffType.noPhysicalDamage, BuffType.ethereal, BuffType.invulnerable}},
    //     {damageType.fire, new List<BuffType>(){BuffType.fireproof, BuffType.ethereal, BuffType.invulnerable}},
    //     {damageType.cutting, new List<BuffType>(){BuffType.noPhysicalDamage, BuffType.ethereal, BuffType.invulnerable}},
    //     {damageType.piercing, new List<BuffType>(){BuffType.noPhysicalDamage, BuffType.ethereal, BuffType.invulnerable}},
    //     {damageType.cosmic, new List<BuffType>(){BuffType.invulnerable}},
    //     {damageType.asphyxiation, new List<BuffType>(){BuffType.undead}},
    //     {damageType.explosion, new List<BuffType>(){BuffType.noPhysicalDamage, BuffType.ethereal, BuffType.invulnerable}}
    // };
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
    public List<ImpactGibs> impactGibs = new List<ImpactGibs>();
    public virtual void Awake() {
        if (gibsContainerPrefab != null) {
            GameObject gibsContainer = Instantiate(gibsContainerPrefab) as GameObject;
            foreach (Gibs gib in gibsContainer.GetComponents<Gibs>()) {
                Gibs newGib = gameObject.AddComponent<Gibs>();
                newGib.CopyFrom(gib);
            }
            Destroy(gibsContainer);
        }
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

        rigidbody2D = Toolbox.GetOrCreateComponent<Rigidbody2D>(gameObject);
        rigidbody2D.gravityScale = 0;
        controllable = GetComponent<Controllable>();
        Toolbox.RegisterMessageCallback<MessageDamage>(this, HandleMessageDamage);
        Toolbox.RegisterMessageCallback<MessageNetIntrinsic>(this, HandleNetIntrinsic);
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
        }
    }
    public virtual void TakeDamage(MessageDamage message) {
        if (!enabled)
            return;
        if (message.amount == 0)
            return;

        if (message.type == damageType.physical && immuneToPhysical)
            return;
        if (message.type == damageType.fire && immuneToFire)
            return;

        ImpactResult result = Damages(message, netBuffs);
        if (DamageResults.Contains(result)) {
            lastMessage = message;
            if (message.responsibleParty != null) {
                lastAttacker = message.responsibleParty;
            }
            impersonalAttacker = message.impersonal;
            foreach (ImpactGibs impactGib in impactGibs) {
                if (Random.Range(0, 1f) <= impactGib.probability)
                    impactGib.gibs.Emit(message);
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
        if (gameObject.name == "ghost" && SceneManager.GetActiveScene().name == "mayors_attic") {
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
    }

    public static HashSet<ImpactResult> DamageResults = new HashSet<ImpactResult> {
        ImpactResult.damageNormal, ImpactResult.damageCosmic, ImpactResult.damageStrong, ImpactResult.damageOther
    };
    public static HashSet<ImpactResult> RepelResults = new HashSet<ImpactResult> {
        ImpactResult.repelNormal, ImpactResult.repelEthereal, ImpactResult.repelInvulnerable, ImpactResult.repelOther
    };
    public ImpactResult Damages(MessageDamage message, Dictionary<BuffType, Buff> netBuffs) {

        // TODO: account for fire

        if (!message.strength)
            message.amount = Mathf.Max(message.amount - netBuffs[BuffType.armor].floatValue, 0);

        ImpactResult result = ImpactResult.none;
        AudioClip[] sounds = new AudioClip[0];

        // blocks
        if (netBuffs != null) {
            float armor = netBuffs[BuffType.armor].floatValue;
            bool ethereal = netBuffs[BuffType.ethereal].active();
            bool invulnerable = netBuffs[BuffType.invulnerable].active();
            bool fireproof = netBuffs[BuffType.fireproof].active();
            bool undead = netBuffs[BuffType.undead].active();

            switch (message.type) {
                case damageType.physical:
                case damageType.cutting:
                case damageType.piercing:
                    if (message.amount <= 0) {
                        sounds = repelSounds;
                        result = ImpactResult.repelNormal;
                    }
                    if (ethereal) {
                        sounds = etherealRepelSounds;
                        result = ImpactResult.repelEthereal;
                    }
                    if (invulnerable) {
                        sounds = invulnerableRepelSounds;
                        result = ImpactResult.repelInvulnerable;
                    }
                    break;
                case damageType.cosmic:
                    if (invulnerable) {
                        sounds = invulnerableRepelSounds;
                        result = ImpactResult.repelInvulnerable;
                    }
                    break;
                case damageType.fire:
                    if (fireproof) result = ImpactResult.repelOther;
                    break;
                case damageType.asphyxiation:
                    if (undead) result = ImpactResult.repelOther;
                    break;
                default:
                    break;
            }

        }
        if (!RepelResults.Contains(result) && !message.suppressImpactSound) {
            switch (message.type) {
                case damageType.physical:
                case damageType.cutting:
                case damageType.piercing:
                    if (message.impactSounds.Length > 0) {
                        sounds = message.impactSounds;
                    } else {
                        sounds = impactSounds;
                    }
                    result = ImpactResult.damageNormal;
                    break;
                case damageType.cosmic:
                    // TODO: cosmic impact effect
                    sounds = cosmicImpactSounds;
                    result = ImpactResult.damageCosmic;
                    Instantiate(Resources.Load("particles/cosmic_impact"), transform.position, Quaternion.identity);
                    break;
                default:
                    result = ImpactResult.damageOther;
                    break;
            }
            if (message.strength) {
                result = ImpactResult.damageStrong;
                sounds = strongImpactSounds;
                Instantiate(Resources.Load("particles/explosion1"), transform.position, Quaternion.identity);
            }
        }

        // play sound
        if (sounds.Length > 0) {
            Toolbox.Instance.AudioSpeaker(sounds[Random.Range(0, sounds.Length)], transform.position);
        }
        if (message.messenger != null)
            message.messenger.SendMessage("ImpactReceived", result, SendMessageOptions.DontRequireReceiver);
        return result;
    }
}