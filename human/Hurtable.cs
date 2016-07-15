using UnityEngine;
// using System.Collections;

public class Hurtable : MonoBehaviour, IMessagable {

	public float health;
	public float maxHealth;
	public float bonusHealth;
	private Intrinsic myIntrinsic = new Intrinsic();
	public bool unconscious;
	private float hitStunCounter;
	private bool hitstun;
	private bool doubledOver;
	public float impulse;
	private bool knockDown;
	private float downedTimer;

	public void TakeDamage(damageType type, float amount){
		if (!myIntrinsic.invulnerable.boolValue){
			switch (type){
			case damageType.physical:
				if (!myIntrinsic.no_physical_damage.boolValue){
					health -= amount;
					impulse += amount;
					
				}
				break;
			case damageType.fire:
				if (!myIntrinsic.fireproof.boolValue){
					health -= amount;
				}
				break;
			default:
				break;
			}
		}

		if (!hitstun){
			hitstun = true;
			hitStunCounter = 0.25f;
			MessageHitstun message = new MessageHitstun();
			message.value = true;
			Toolbox.Instance.SendMessage(gameObject, this, message);
		}
	}

	public void ReceiveMessage(Message incoming){
		if (incoming is MessageIntrinsic){
			MessageIntrinsic intrins = (MessageIntrinsic)incoming;
			if (intrins.netIntrinsic != null){
				myIntrinsic.armor.floatValue = intrins.netIntrinsic.armor.floatValue;
				if (intrins.netIntrinsic.bonusHealth.floatValue > bonusHealth){
					health += intrins.netIntrinsic.bonusHealth.floatValue;
				}
				bonusHealth = intrins.netIntrinsic.bonusHealth.floatValue;
			}
		}
		if (incoming is MessageDamage){
			MessageDamage dam = (MessageDamage)incoming;
			TakeDamage(dam.type, dam.amount);
			if (dam.impactor)	
				dam.impactor.PlayImpactSound();
			Rigidbody2D body = GetComponent<Rigidbody2D>();
			if (body){
				Debug.Log("adding force "+dam.force.ToString());
				body.AddForce(dam.force * 100f);
			}
		}
	}

	public void Update(){
		if (impulse > 0){
			impulse -= Time.deltaTime * 25f;
		}
		if (downedTimer > 0){
			downedTimer -= Time.deltaTime;
		}
		if (hitStunCounter > 0){
			hitStunCounter -= Time.deltaTime;
			if (hitStunCounter <= 0 && !doubledOver && !knockDown){
				hitstun = false;
				MessageHitstun message = new MessageHitstun();
				message.value = false;
				Toolbox.Instance.SendMessage(gameObject, this, message);
			}
		}
		if (health < 0.5 * maxHealth){
			health += Time.deltaTime;
		}
		if (health <= 0 && !unconscious){
			unconscious = true;
			if (!knockDown)
				KnockDown();
		}

		if (unconscious && health > 0){
			unconscious = false;
		}


		if (impulse > 50f && !knockDown){
			KnockDown();
		}
		if (downedTimer <= 0 && knockDown && health > 0){
			GetUp();
		}
		if (impulse > 35f && !doubledOver && !knockDown){
			DoubleOver(true);
		}
		if (impulse <= 0f && doubledOver &&!knockDown){
			DoubleOver(false);
		}
	}

	public void KnockDown(){
		knockDown = true;
		doubledOver = false;
		downedTimer = 2f;
		Vector3 pivot = transform.position;
		pivot.y -= 0.15f;
		transform.RotateAround(pivot, new Vector3(0, 0, 1), -90);
		MessageHitstun message = new MessageHitstun();
		message.value = true;
		message.doubledOver = false;
		Toolbox.Instance.SendMessage(gameObject, this, message);
	}

	public void GetUp(){
		knockDown = false;
		hitstun = false;
		doubledOver = false;
		Vector3 pivot = transform.position;
		pivot.x -= 0.15f;
		transform.RotateAround(pivot, new Vector3(0, 0, 1), 90);
		MessageHitstun message = new MessageHitstun();
		message.value = false;
		Toolbox.Instance.SendMessage(gameObject, this, message);
	}
	public void DoubleOver(bool val){
		if (val){
			doubledOver = true;
			MessageHitstun message = new MessageHitstun();
			message.value = true;
			message.doubledOver = true;
			Toolbox.Instance.SendMessage(gameObject, this, message);
		} else {
			doubledOver = false;
			hitstun = false;
			MessageHitstun message = new MessageHitstun();
			message.value = false;
			message.doubledOver = false;
			Toolbox.Instance.SendMessage(gameObject, this, message);
		}
	}
	

}
