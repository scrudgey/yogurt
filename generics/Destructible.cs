using UnityEngine;
using System.Collections.Generic;

public enum damageType{physical, fire, any, cutting, piercing}
public class Destructible : Damageable, ISaveable {
	public float maxHealth;
	public float health;
	public float bonusHealth;
	public float armor;
	public AudioClip[] hitSound;
	public AudioClip[] destroySound;
	public bool invulnerable;
	public bool fireproof;
	public bool no_physical_damage;
	public float physicalMultiplier = 1f;
	public float fireMultiplier = 1f;
	public Dictionary<BuffType, Buff> currentBuffs;

	void Update () {
		if (health < 0){
			Die();
		}
		if (health > maxHealth + bonusHealth){
			health = maxHealth + bonusHealth;
		}
	}
	public override ImpactResult CalculateDamage(MessageDamage message){
		if (!invulnerable){
			switch (message.type)
			{
			case damageType.piercing:
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
		Destruct();
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
		EventData data = Toolbox.Instance.DataFlag(gameObject, chaos:Random.Range(1, 2));
		data.noun = "destruction";
		data.whatHappened = Toolbox.Instance.CloneRemover(gameObject.name)+" was destroyed";
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
			//TODO: i should fix this to be more physical
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
	public override void ReceiveMessage(Message message){
		base.ReceiveMessage(message);
		if (message is MessageNetIntrinsic){
			MessageNetIntrinsic intrins = (MessageNetIntrinsic)message;
			armor = intrins.netBuffs[BuffType.armor].floatValue;
			if (intrins.netBuffs[BuffType.bonusHealth].floatValue > bonusHealth){
				health += intrins.netBuffs[BuffType.bonusHealth].floatValue;
			}
			bonusHealth = intrins.netBuffs[BuffType.bonusHealth].floatValue;
		}
	}
	public void SaveData(PersistentComponent data){
		data.floats["health"] = health;
		data.bools["invulnerable"] = invulnerable;
		data.bools["fireproof"] = fireproof;
		data.bools["noPhysDam"] = no_physical_damage;
		data.ints["lastDamage"] = (int)lastDamage;
	}
	public void LoadData(PersistentComponent data){
		health = data.floats["health"];
		invulnerable = data.bools["invulnerable"];
		fireproof = data.bools["fireproof"];
		no_physical_damage = data.bools["noPhysDam"];
		lastDamage = (damageType)data.ints["lastDamage"];
	}
}
