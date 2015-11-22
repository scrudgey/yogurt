using UnityEngine;
// using System.Collections;

public class Destructible : MonoBehaviour {

	public float health;
	public enum damageType{physical,fire,any}
	public damageType lastDamage;
	public AudioClip[] hitSound;
	public AudioClip[] destroySound;
	public bool invulnerable;
	public bool fireproof;
	public bool no_physical_damage;

	void Update () {
		if (health < 0){
			Die();
		}
	}

	public void TakeDamage( Destructible.damageType type , float damage){
		if (!invulnerable){
			switch (type)
			{
			case damageType.physical:
				if (!no_physical_damage){
					health -= damage;
					lastDamage = type;
				}
				break;
			case damageType.fire:
				if (!fireproof){
					health -= damage;
					lastDamage = type;
				}
				break;
			default:
				break;
			}
		}
	}

	private void Die(){
		foreach (Gibs gib in GetComponents<Gibs>())
			if(gib.damageCondition == lastDamage || gib.damageCondition == damageType.any)
				gib.Emit();
		PhysicalBootstrapper phys = GetComponent<PhysicalBootstrapper>();
		if (phys){
			phys.DestroyPhysical();
		}
		if (destroySound.Length > 0){
			GameObject speaker = Instantiate(Resources.Load("Speaker"),transform.position,Quaternion.identity) as GameObject;
			speaker.GetComponent<AudioSource>().clip = destroySound[Random.Range(0,destroySound.Length)];
			speaker.GetComponent<AudioSource>().Play();
		}
		LiquidContainer container = GetComponent<LiquidContainer>();
		if (container && container.amount > 0){
			Toolbox.Instance.SpawnDroplet(transform.position, container.liquid);
			Toolbox.Instance.SpawnDroplet(transform.position, container.liquid);
			Toolbox.Instance.SpawnDroplet(transform.position, container.liquid);
		}
		BroadcastMessage("OnDestruction", SendMessageOptions.DontRequireReceiver);
		Messenger.Instance.WasDestroyed(gameObject);
	}

	void OnCollisionEnter2D(Collision2D col){
		float vel = col.relativeVelocity.magnitude;
		if (vel > 1){
			//if we were hit hard, take a splatter damage
			//i should fix this to be more physical
			float damage = col.rigidbody.mass * vel / 5.0f;
			TakeDamage(damageType.physical, damage);
//			Debug.Log("Collision damage on " + gameObject.name + " to the tune of " + damage.ToString());
//			Debug.Log("Collided with "+col.gameObject.name);
		}
		if (hitSound.Length > 0){
			GetComponent<AudioSource>().PlayOneShot(hitSound[Random.Range(0,hitSound.Length)]);
		}
	}

}
