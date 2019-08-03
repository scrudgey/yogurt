using UnityEngine;
using System.Collections.Generic;

public enum damageType { physical, fire, any, cutting, piercing, cosmic, asphyxiation }
public enum ImpactResult { normal, repel, strong }
public abstract class Damageable : MonoBehaviour {
    public static Dictionary<damageType, List<BuffType>> blockedBy = new Dictionary<damageType, List<BuffType>>(){
        {damageType.physical, new List<BuffType>(){BuffType.noPhysicalDamage, BuffType.ethereal, BuffType.invulnerable}},
        {damageType.fire, new List<BuffType>(){BuffType.fireproof, BuffType.ethereal, BuffType.invulnerable}},
        {damageType.cutting, new List<BuffType>(){BuffType.noPhysicalDamage, BuffType.ethereal, BuffType.invulnerable}},
        {damageType.piercing, new List<BuffType>(){BuffType.noPhysicalDamage, BuffType.ethereal, BuffType.invulnerable}},
        {damageType.cosmic, new List<BuffType>(){BuffType.invulnerable}},
        {damageType.asphyxiation, new List<BuffType>(){BuffType.undead}}
    };
    public bool immuneToFire;
    public bool immuneToPhysical;
    public AudioClip[] impactSounds;
    public AudioClip[] repelSounds;
    public AudioClip[] strongImpactSounds;
    public damageType lastDamage;
    public MessageDamage lastMessage;
    public GameObject gibsContainerPrefab;
    public Dictionary<BuffType, Buff> netBuffs;
    new public Rigidbody2D rigidbody2D;
    public Controllable controllable;
    public static bool Damages(Damageable damageable, damageType type, Dictionary<BuffType, Buff> netBuffs) {
        // no buffs means no immunities
        if (netBuffs == null)
            return true;
        // is this type of damage described in terms of immunities?
        if (!blockedBy.ContainsKey(type)) {
            return true;
        }
        // check each buff for providing immunity
        foreach (KeyValuePair<BuffType, Buff> kvp in netBuffs) {
            if (!kvp.Value.boolValue && kvp.Value.floatValue <= 0) {
                continue;
            }
            if (blockedBy[type].Contains(kvp.Key)) {
                return false;
            }
        }
        // by default, we take damage
        return true;
    }
    public virtual void Awake() {
        if (gibsContainerPrefab != null) {
            GameObject gibsContainer = Instantiate(gibsContainerPrefab) as GameObject;
            foreach (Gibs gib in gibsContainer.GetComponents<Gibs>()) {
                Gibs newGib = gameObject.AddComponent<Gibs>();
                newGib.CopyFrom(gib);
            }
            Destroy(gibsContainer);
        }
        if (impactSounds != null)
            if (impactSounds.Length == 0)
                impactSounds = Resources.LoadAll<AudioClip>("sounds/impact_normal/");
        if (repelSounds != null)
            if (repelSounds.Length == 0)
                repelSounds = Resources.LoadAll<AudioClip>("sounds/impact_repel/");
        if (strongImpactSounds != null)
            if (strongImpactSounds.Length == 0)
                strongImpactSounds = Resources.LoadAll<AudioClip>("sounds/impact_strong/");
        rigidbody2D = Toolbox.GetOrCreateComponent<Rigidbody2D>(gameObject);
        rigidbody2D.gravityScale = 0;
        controllable = GetComponent<Controllable>();
        Toolbox.RegisterMessageCallback<MessageDamage>(this, TakeDamage);
        Toolbox.RegisterMessageCallback<MessageNetIntrinsic>(this, HandleNetIntrinsic);
    }
    void HandleNetIntrinsic(MessageNetIntrinsic message) {
        netBuffs = message.netBuffs;
        NetIntrinsicsChanged(message);
    }
    public abstract void NetIntrinsicsChanged(MessageNetIntrinsic message);
    public virtual void TakeDamage(MessageDamage message) {
        if (message.amount == 0)
            return;
        lastMessage = message;
        lastDamage = message.type;
        bool vulnerable = Damages(this, message.type, netBuffs);
        ImpactResult result = ImpactResult.normal;
        float damage = 0f;
        if (vulnerable) {
            if (message.type == damageType.physical && immuneToPhysical)
                return;
            if (message.type == damageType.fire && immuneToFire)
                return;
            if (message.type == damageType.cosmic && netBuffs != null && netBuffs[BuffType.ethereal].boolValue) {
                message.type = damageType.cutting;
                lastDamage = damageType.cutting;
            }
            damage = CalculateDamage(message);
            if (damage > 0) {
                if (message.strength) {
                    result = ImpactResult.strong;
                } else {
                    result = ImpactResult.normal;
                }
            } else {
                result = ImpactResult.repel;
            }
            // play impact sounds
            if ((message.impactSounds.Length > 0) || (message.type != damageType.fire && message.type != damageType.asphyxiation && message.type != damageType.cosmic))
                PlayImpactSound(result, message);
            if (message.messenger != null)
                message.messenger.SendMessage("ImpactReceived", result, SendMessageOptions.DontRequireReceiver);
        } else {
            // do we play a repel sound here? or no?
        }
        if (damage <= 0)
            return;
        if (gameObject == GameManager.Instance.playerObject) {
            UINew.Instance.Hit();
        }
        if (message.type == damageType.fire || message.type == damageType.cosmic || message.type == damageType.asphyxiation)
            return;
        if (controllable) {
            controllable.direction = -1f * message.force;
        }
    }
    public abstract float CalculateDamage(MessageDamage message);
    public virtual void Destruct() {
        if (lastMessage == null)
            lastMessage = new MessageDamage(0.5f, damageType.physical);
        foreach (Gibs gib in GetComponents<Gibs>())
            gib.Emit(lastDamage, lastMessage.force);
        PhysicalBootstrapper phys = GetComponent<PhysicalBootstrapper>();
        if (phys) {
            phys.DestroyPhysical();
        }
        ClaimsManager.Instance.WasDestroyed(gameObject);
        Destroy(gameObject);
    }

    public void PlayImpactSound(ImpactResult impactType, MessageDamage message) {
        if (message.suppressImpactSound)
            return;
        AudioClip[] sounds = new AudioClip[0];
        switch (impactType) {
            case ImpactResult.normal:
                if (message.impactSounds.Length > 0) {
                    sounds = message.impactSounds;
                } else {
                    sounds = impactSounds;
                }
                break;
            case ImpactResult.repel:
                sounds = repelSounds;
                break;
            case ImpactResult.strong:
                sounds = strongImpactSounds;
                Instantiate(Resources.Load("particles/explosion1"), transform.position, Quaternion.identity);
                break;
            default:
                break;
        }
        if (sounds.Length > 0) {
            Toolbox.Instance.AudioSpeaker(sounds[Random.Range(0, sounds.Length)], transform.position);
        }
    }
}