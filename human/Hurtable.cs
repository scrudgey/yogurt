using UnityEngine;
public class Hurtable : MonoBehaviour, IMessagable {
	private Controllable.HitState _hitState;
	public Controllable.HitState hitState{
		get {return _hitState;}
		set {
			// if value has changed, send a message:
			if (value != _hitState){
				_hitState = value;
				MessageHitstun message = new MessageHitstun();
				message.hitState = value;
				Toolbox.Instance.SendMessage(gameObject, this, message);
			}
		}
	}
	public float health;
	public float maxHealth;
	public float bonusHealth;
	private Intrinsic myIntrinsic = new Intrinsic();
	private float hitStunCounter;
	private bool doubledOver;
	public float impulse;
	private float downedTimer;
	private float ouchFrequency = 0.1f;
	public void TakeDamage(damageType type, float amount){
		if (!myIntrinsic.invulnerable.boolValue){
			switch (type){
			case damageType.physical:
				if (!myIntrinsic.noPhysicalDamage.boolValue){
					float netDam = Mathf.Max(amount - myIntrinsic.armor.floatValue, 0);
					health -= netDam;
					impulse += netDam;
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
		if (health <= -0.5 * maxHealth){
			Die(type);
		}
		if (type != damageType.fire){
			hitState = Controllable.AddHitState(hitState, Controllable.HitState.stun);
			hitStunCounter = 0.25f;
		}

		if (Random.Range(0.0f, 1.0f) < ouchFrequency){
			MessageSpeech speechMessage = new MessageSpeech();
			speechMessage.nimrodKey = true;
			switch(type){
				case damageType.physical:
				speechMessage.phrase = "pain-physical";
				break;
				case damageType.fire:
				speechMessage.phrase = "pain-fire";
				break;
				default:
				speechMessage.phrase = "pain-physical";
				break;
			}
			Toolbox.Instance.SendMessage(gameObject, this, speechMessage);
		}
	}
	public void Die(damageType type){
		KnockDown();
		hitState = Controllable.AddHitState(hitState, Controllable.HitState.dead);
	}
	public void ReceiveMessage(Message incoming){
		if (incoming is MessageNetIntrinsic){
			MessageNetIntrinsic intrins = (MessageNetIntrinsic)incoming;
			myIntrinsic = intrins.netIntrinsic;
			if (intrins.netIntrinsic.bonusHealth.floatValue > bonusHealth){
				health += intrins.netIntrinsic.bonusHealth.floatValue;
			}
			bonusHealth = intrins.netIntrinsic.bonusHealth.floatValue;
		}
		if (incoming is MessageDamage){
			MessageDamage dam = (MessageDamage)incoming;
			TakeDamage(dam.type, dam.amount);
			if (dam.impactor)	
				dam.impactor.PlayImpactSound();
			Rigidbody2D body = GetComponent<Rigidbody2D>();
			if (body){
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
			if (hitStunCounter <= 0 && !doubledOver){
				hitState = Controllable.RemoveHitState(hitState, Controllable.HitState.stun);
			}
		}
		if (health < 0.5 * maxHealth){
			health += Time.deltaTime;
		}
		if (health <= 0 && hitState < Controllable.HitState.unconscious){
			KnockDown();
		}
		if (impulse > 50f && hitState < Controllable.HitState.unconscious){
			KnockDown();
		}
		if (downedTimer <= 0 && hitState == Controllable.HitState.unconscious && health > 0){
			GetUp();
		}
		if (impulse > 35f && !doubledOver && hitState < Controllable.HitState.unconscious){
			DoubleOver(true);
		}
		if (impulse <= 0f && doubledOver && hitState < Controllable.HitState.unconscious){
			DoubleOver(false);
		}
	}
	// TODO: knockdown message!
	public void KnockDown(){
		if (hitState >= Controllable.HitState.unconscious)
			return;
		hitState = Controllable.AddHitState(hitState, Controllable.HitState.unconscious);
		doubledOver = false;
		downedTimer = 2f;
		Vector3 pivot = transform.position;
		pivot.y -= 0.15f;
		transform.RotateAround(pivot, new Vector3(0, 0, 1), -90);
		MessageHitstun message = new MessageHitstun();
		message.doubledOver = false;
		message.hitState = hitState;
		Toolbox.Instance.SendMessage(gameObject, this, message);
	}
	public void GetUp(){
		hitState = Controllable.RemoveHitState(hitState, Controllable.HitState.unconscious);
		doubledOver = false;
		Vector3 pivot = transform.position;
		pivot.x -= 0.15f;
		transform.RotateAround(pivot, new Vector3(0, 0, 1), 90);
	}
	public void DoubleOver(bool val){
		if (val){
			hitState = Controllable.AddHitState(hitState, Controllable.HitState.stun);
			doubledOver = true;
			MessageHitstun message = new MessageHitstun();
			message.doubledOver = true;
			message.hitState = hitState;
			Toolbox.Instance.SendMessage(gameObject, this, message);
		} else {
			hitState = Controllable.RemoveHitState(hitState, Controllable.HitState.stun);
			doubledOver = false;
			MessageHitstun message = new MessageHitstun();
			message.doubledOver = false;
			message.hitState = hitState;
			Toolbox.Instance.SendMessage(gameObject, this, message);
		}
	}
}
