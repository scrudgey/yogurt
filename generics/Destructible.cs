﻿using UnityEngine;
public enum damageType{physical, fire, any, cutting}

public class Destructible : MonoBehaviour, IMessagable, IDamageable {
	public float health;
	public float maxHealth;
	public float bonusHealth;
	public damageType lastDamage;
	public AudioClip[] hitSound;
	public AudioClip[] destroySound;
	public bool invulnerable;
	public bool fireproof;
	public bool no_physical_damage;
	public float armor;
	public float physicalMultiplier = 1f;
	public float fireMultiplier = 1f;

	void Update () {
		if (health < 0){
			Die();
		}
		if (health > maxHealth + bonusHealth){
			health = maxHealth + bonusHealth;
		}
	}

	public ImpactResult TakeDamage(MessageDamage message){
		if (!invulnerable){
			switch (message.type)
			{
			case damageType.cutting:
			case damageType.physical:
				if (!no_physical_damage){
					health -= message.amount * physicalMultiplier;
					
				}
				break;
			case damageType.fire:
				if (!fireproof){
					health -= message.amount * fireMultiplier;
				}
				break;
			default:
				break;
			}
			lastDamage = message.type;
			if (message.strength){
				return ImpactResult.strong;
			} else {
				return ImpactResult.normal;
			}
		}
		return ImpactResult.repel;
	}

	//TODO: make destruction chaos somehow proportional to object
	public void Die(){
		foreach (Gibs gib in GetComponents<Gibs>())
			if(gib.damageCondition == lastDamage || gib.damageCondition == damageType.any){
				gib.Emit();
			}
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
		ClaimsManager.Instance.WasDestroyed(gameObject);
		Destroy(gameObject);
		Toolbox.Instance.DataFlag(gameObject, 175f, 0f, 0f, 0f, 0f);
		if (lastDamage == damageType.fire){
			if (Toolbox.Instance.CloneRemover(name) == "dollar"){
				GameManager.Instance.data.achievementStats.dollarsBurned += 1;
				GameManager.Instance.CheckAchievements();
			}
		}
	}

	void OnCollisionEnter2D(Collision2D col){
		float vel = col.relativeVelocity.magnitude;
		if (vel > 1){
			//if we were hit hard, take a splatter damage
			//i should fix this to be more physical
			if (col.rigidbody){
				float damage = col.rigidbody.mass * vel / 5.0f;
				MessageDamage message = new MessageDamage();
				message.amount = damage;
				message.type = damageType.physical;
				message.force = col.relativeVelocity;
				TakeDamage(message);
				// Debug.Log("Collision damage on " + gameObject.name + " to the tune of " + damage.ToString());	
			} else {
				Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
				float damage = rigidbody.mass * vel / 5.0f;
				MessageDamage message = new MessageDamage();
				message.amount = damage;
				message.type = damageType.physical;
				message.force = col.relativeVelocity;
				TakeDamage(message);
				// Debug.Log("Collision damage on " + gameObject.name + " to the tune of " + damage.ToString());
			}
		}
		if (hitSound.Length > 0){
			GetComponent<AudioSource>().PlayOneShot(hitSound[Random.Range(0, hitSound.Length)]);
		}
	}

	public void ReceiveMessage(Message message){
		if (message is MessageNetIntrinsic){
			MessageNetIntrinsic intrins = (MessageNetIntrinsic)message;
			// if (intrins.netIntrinsic != null){
			armor = intrins.netIntrinsic.armor.floatValue;
			if (intrins.netIntrinsic.bonusHealth.floatValue > bonusHealth){
				health += intrins.netIntrinsic.bonusHealth.floatValue;
			}
			bonusHealth = intrins.netIntrinsic.bonusHealth.floatValue;
			// }
		}
		if (message is MessageDamage){
			MessageDamage dam = (MessageDamage)message;
			// TakeDamage(dam.type, dam.amount);
			// if (dam.impactor)	
			// 	dam.impactor.PlayImpactSound();
			dam.TakeDamage(this);
		}
	}

}
