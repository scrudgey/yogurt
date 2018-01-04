using UnityEngine;
using System.Collections.Generic;

public enum damageType{physical, fire, any, cutting, piercing, cosmic}
public enum ImpactResult {normal, repel, strong}
public abstract class Damageable: MonoBehaviour, IMessagable{
    public static Dictionary<damageType, List<BuffType>> defeatedBy = new Dictionary<damageType, List<BuffType>>(){
        {damageType.physical, new List<BuffType>(){BuffType.noPhysicalDamage, BuffType.ethereal, BuffType.invulnerable}},
        {damageType.fire, new List<BuffType>(){BuffType.fireproof, BuffType.ethereal, BuffType.invulnerable}},
        {damageType.cutting, new List<BuffType>(){BuffType.noPhysicalDamage, BuffType.ethereal, BuffType.invulnerable}},
        {damageType.piercing, new List<BuffType>(){BuffType.noPhysicalDamage, BuffType.ethereal, BuffType.invulnerable}},
        {damageType.cosmic, new List<BuffType>(){BuffType.invulnerable}},
    };
    public AudioClip[] impactSounds;
	public AudioClip[] repelSounds;
	public AudioClip[] strongImpactSounds;
    public damageType lastDamage;
    public GameObject gibsContainerPrefab;
    public Dictionary<BuffType, Buff> netBuffs;
    new public Rigidbody2D rigidbody2D;
    public Controllable controllable;
    public static bool Damages(damageType type, Dictionary<BuffType, Buff> netBuffs){
        // no buffs means no immunities
        if (netBuffs == null)
            return true;
        // is this type of damage described in terms of immunities?
        if (!defeatedBy.ContainsKey(type)){
            return true;
        }
        // check each buff for providing immunity
        foreach(KeyValuePair<BuffType, Buff> kvp in netBuffs){
            if (!kvp.Value.boolValue && kvp.Value.floatValue <= 0){
                continue;
            }
            if (defeatedBy[type].Contains(kvp.Key)){
                return false;
            }
        }
        // by default, we take damage
        return true;
    }

    public virtual void Start(){
        if (gibsContainerPrefab != null){
            GameObject gibsContainer = Instantiate(gibsContainerPrefab) as GameObject;
            foreach(Gibs gib in gibsContainer.GetComponents<Gibs>()){
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
        rigidbody2D = Toolbox.Instance.GetOrCreateComponent<Rigidbody2D>(gameObject);
        controllable = GetComponent<Controllable>();
    }
    public void TakeDamage(MessageDamage message){
        lastDamage = message.type;
        bool vulnerable = Damages(message.type, netBuffs);
        ImpactResult result = ImpactResult.normal;
        float damage = 0f;
        if (vulnerable){
            damage = CalculateDamage(message);
            if (damage > 0){
                if (message.strength){
                    result = ImpactResult.strong;
                } else {
                    result = ImpactResult.normal;
                }
            } else {
                result = ImpactResult.repel;
            }
            // play impact sounds
            if (message.type != damageType.fire)    
                PlayImpactSound(result, message);
            if (message.messenger != null)
                message.messenger.SendMessage("ImpactReceived", result, SendMessageOptions.DontRequireReceiver);
		} else {
			// do we play a repel sound here? or no?
		}
        // apply force
        if (damage <= 0)
            return;
        if (message.type == damageType.fire || message.type == damageType.cosmic)
            return;
        if (rigidbody2D){
            rigidbody2D.AddForce(message.force);
        }
        if (controllable){
            controllable.direction = -1f * message.force;
        }
    }
    public abstract float CalculateDamage(MessageDamage message);

    public virtual void Destruct(){
        foreach (Gibs gib in GetComponents<Gibs>())
            gib.Emit(lastDamage);
        PhysicalBootstrapper phys = GetComponent<PhysicalBootstrapper>();
		if (phys){
			phys.DestroyPhysical();
		}
        if (GameManager.Instance.playerObject == gameObject){
			GameManager.Instance.PlayerDeath();
		}
        ClaimsManager.Instance.WasDestroyed(gameObject);
        Destroy(gameObject);
    }
    public virtual void ReceiveMessage(Message incoming){
		if (incoming is MessageDamage){
			MessageDamage message = (MessageDamage)incoming;
            TakeDamage(message);
        }
        if (incoming is MessageNetIntrinsic){
            MessageNetIntrinsic message = (MessageNetIntrinsic)incoming;
            netBuffs = message.netBuffs;
        }
	}
    public void PlayImpactSound(ImpactResult impactType, MessageDamage message){
        AudioClip[] sounds = new AudioClip[0];
        switch(impactType){
            case ImpactResult.normal:
            if (message.impactSounds.Length > 0){
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
		if (sounds.Length > 0){
            Toolbox.Instance.AudioSpeaker(sounds[Random.Range(0, sounds.Length)], transform.position);
        }
	}
}