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
		if (health <= 0){
			health = 0f;
			if (!unconscious){
				unconscious = true;
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
		}
	}

	public void Update(){
		if (hitStunCounter > 0){
			hitStunCounter -= Time.deltaTime;
			if (hitStunCounter <= 0 && !doubledOver){
				hitstun = false;

				MessageHitstun message = new MessageHitstun();
				message.value = false;
				Toolbox.Instance.SendMessage(gameObject, this, message);
			}
		}
		if (impulse > 0){
			impulse -= Time.deltaTime * 25f;
		}
		if (impulse > 30f && !doubledOver){
			doubledOver = true;
			MessageHitstun message = new MessageHitstun();
			message.value = true;
			message.doubledOver = true;
			Toolbox.Instance.SendMessage(gameObject, this, message);
		}
		if (impulse <= 0f && doubledOver){
			doubledOver = false;
			hitstun = false;
			MessageHitstun message = new MessageHitstun();
			message.value = false;
			message.doubledOver = false;
			Toolbox.Instance.SendMessage(gameObject, this, message);
		}
	}

}
