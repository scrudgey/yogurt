using UnityEngine;

public enum damageType{physical, fire, any, cutting, piercing, cosmic}
public enum ImpactResult {normal, repel, strong}
public abstract class Damageable: MonoBehaviour, IMessagable{
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
        // Outfit outfit = GetComponent<Outfit>();
        // if (outfit){
        //     outfit.RemoveUniform();
        // }
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
        }
	}
}