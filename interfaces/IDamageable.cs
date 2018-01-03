using UnityEngine;
using System.Collections.Generic;

public enum damageType{physical, fire, any, cutting, piercing, cosmic}
public enum ImpactResult {normal, repel, strong}
public abstract class Damageable: MonoBehaviour, IMessagable{
    public static Dictionary<damageType, List<BuffType>> defeatedBy = new Dictionary<damageType, List<BuffType>>(){
        {damageType.fire, new List<BuffType>(){BuffType.ethereal, BuffType.fireproof, BuffType.invulnerable}},
        {damageType.cosmic, new List<BuffType>(){}},
        {damageType.cutting, new List<BuffType>(){BuffType.ethereal, BuffType.invulnerable}},
        
    };
    public damageType lastDamage;
    public GameObject gibsContainerPrefab;
    public ImpactResult TakeDamage(MessageDamage message){
        lastDamage = message.type;
        return CalculateDamage(message);
    }
    public abstract ImpactResult CalculateDamage(MessageDamage message);
    public virtual void Start(){
        if (gibsContainerPrefab != null){
            GameObject gibsContainer = Instantiate(gibsContainerPrefab) as GameObject;
            foreach(Gibs gib in gibsContainer.GetComponents<Gibs>()){
                Gibs newGib = gameObject.AddComponent<Gibs>();
                newGib.CopyFrom(gib);
            }
            Destroy(gibsContainer);
        }
    }
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
    public virtual void ReceiveMessage(Message message){
		if (message is MessageDamage){
			MessageDamage dam = (MessageDamage)message;
			ImpactResult result = TakeDamage(dam);
            if (dam.impactor){
			    dam.impactor.PlayImpactSound(result);
		    }
            message.messenger.SendMessage("ImpactReceived", result, SendMessageOptions.DontRequireReceiver);
        }
	}
    public static bool Damages(damageType type, Dictionary<BuffType, Buff> netBuffs){
        if (!defeatedBy.ContainsKey(type)){
            return true;
        }
        foreach(KeyValuePair<BuffType, Buff> kvp in netBuffs){
            if (!kvp.Value.boolValue && kvp.Value.floatValue <= 0){
                continue;
            }
            if (defeatedBy[type].Contains(kvp.Key)){
                return false;
            }
        }
        return true;
    }
}