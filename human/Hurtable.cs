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
	public Intrinsic myIntrinsic = new Intrinsic();
	private float hitStunCounter;
	private bool doubledOver;
	public float impulse;
	public float downedTimer;
	private float ouchFrequency = 0.1f;
	public GameObject dizzyEffect;
	public GameObject lastAttacker;
	public void TakeDamage(damageType type, float amount){
		bool tookDamage = false;
		if (!myIntrinsic.invulnerable.boolValue){
			switch (type){
			case damageType.physical:
				if (!myIntrinsic.noPhysicalDamage.boolValue){
					float netDam = Mathf.Max(amount - myIntrinsic.armor.floatValue, 0);
					health -= netDam;
					impulse += netDam;
					tookDamage = true;
				}
				break;
			case damageType.fire:
				if (!myIntrinsic.fireproof.boolValue){
					health -= amount;
					tookDamage = true;
				}
				break;
			default:
				break;
			}
		}
		if (health <= 0 && type == damageType.fire && hitState != Controllable.HitState.dead){
			Die(type);
		}
		if (health <= -0.5 * maxHealth && hitState != Controllable.HitState.dead){
			Die(type);
		}
		if (type != damageType.fire){
			hitState = Controllable.AddHitState(hitState, Controllable.HitState.stun);
			hitStunCounter = 0.25f;
		}
		if (tookDamage && Random.Range(0.0f, 1.0f) < ouchFrequency){
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
		if (type == damageType.fire){
			Inventory inv = GetComponent<Inventory>();
			if (inv){
				inv.DropItem();
			}
			Instantiate(Resources.Load("prefabs/skeleton"), transform.position, transform.rotation);
			Toolbox.Instance.AudioSpeaker("Flash Fire Ignite 01", transform.position);
			Destroy(gameObject);
			if (lastAttacker == gameObject){
				GameManager.Instance.data.achievementStats.selfImmolations += 1;
				GameManager.Instance.CheckAchievements();
			} else {
				GameManager.Instance.data.achievementStats.immolations += 1;
				GameManager.Instance.CheckAchievements();
			}
		} else {
			KnockDown();
		}
		if (dizzyEffect != null)
			Destroy(dizzyEffect);
		if (GameManager.Instance.playerObject == gameObject){
			GameManager.Instance.PlayerDeath();
		}
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
			if (dam.responsibleParty != null){
				lastAttacker = dam.responsibleParty;
			}
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
		if (dizzyEffect != null){
			if (dizzyEffect.transform.localScale != transform.localScale){
				Vector3 tempscale = transform.localScale;
				dizzyEffect.transform.localScale = tempscale; 
			}
		}
	}
	// TODO: knockdown message!
	public void KnockDown(){
		if (hitState >= Controllable.HitState.unconscious)
			return;
		hitState = Controllable.AddHitState(hitState, Controllable.HitState.unconscious);
		doubledOver = false;
		downedTimer = 10f;
		Vector3 pivot = transform.position;
		pivot.y -= 0.15f;
		transform.RotateAround(pivot, new Vector3(0, 0, 1), -90);
		MessageHitstun message = new MessageHitstun();
		message.doubledOver = false;
		message.hitState = hitState;
		Toolbox.Instance.SendMessage(gameObject, this, message);

		dizzyEffect = Instantiate(Resources.Load("prefabs/fx/dizzy"), transform.position + transform.up * 0.15f + new Vector3(0, 0.15f, 0), Quaternion.identity) as GameObject;
		dizzyEffect.transform.rotation = Quaternion.identity;
		dizzyEffect.transform.SetParent(transform, true);
	}
	public void GetUp(){
		hitState = Controllable.RemoveHitState(hitState, Controllable.HitState.unconscious);
		doubledOver = false;
		Vector3 pivot = transform.position;
		pivot.x -= 0.15f;
		transform.RotateAround(pivot, new Vector3(0, 0, 1), 90);
		if (dizzyEffect != null)
			Destroy(dizzyEffect);
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
	public void Bleed(Vector3 position){
		Liquid blood = LiquidCollection.LoadLiquid("blood");
		float initHeight = (position.y - transform.position.y ) + 0.15f;
		Toolbox.Instance.SpawnDroplet(blood, 0.3f, gameObject, initHeight);
	}
}
