using UnityEngine;
using System.Collections.Generic;

public class Hurtable : Damageable {
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
	public float armor;
	
	// public Intrinsic myIntrinsic = new Intrinsic();
	private float hitStunCounter;
	private bool doubledOver;
	public float impulse;
	public float downedTimer;
	private float ouchFrequency = 0.1f;
	public GameObject dizzyEffect;
	public GameObject lastAttacker;
	public List<Collider2D> backgroundColliders = new List<Collider2D>();
	public Dictionary<BuffType, Buff> netBuffs = new Dictionary<BuffType, Buff>();
	// public Intrinsics intrinsics;
	public override void Start(){
		base.Start();
		backgroundColliders = new List<Collider2D>();
		foreach(Transform transform in transform.root.GetComponentsInChildren<Transform>()){
			if (transform.gameObject.layer == 8){
				Collider2D[] colliders = transform.GetComponents<Collider2D>();
				backgroundColliders.AddRange(colliders);
			}
		}
	}
	public override ImpactResult CalculateDamage(MessageDamage message){
		// Intrinsics intrinsics = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(gameObject);
		Dictionary<BuffType, Buff> netBuffs = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(gameObject).NetBuffs();
		bool tookDamage = false;
		if (message.responsibleParty != null){
			lastAttacker = message.responsibleParty;
		}
		if (!netBuffs[BuffType.invulnerable].boolValue){
			float armor = this.armor;
			if (message.strength || hitState == Controllable.HitState.dead)
				armor = 0;
			switch (message.type){
			case damageType.piercing:
			case damageType.cutting:
				if (armor <= message.amount){
					Bleed(transform.position);
					if (hitState == Controllable.HitState.dead){
						if (health <= -0.75 * maxHealth){
							Destruct();
							EventData data = Toolbox.Instance.DataFlag(gameObject, chaos:3, disturbing:4, disgusting:4, positive:-2, offensive:-2);
							data.noun = "corpse desecration";
							data.whatHappened = "the corpse of "+Toolbox.Instance.GetName(gameObject)+" was desecrated";
						}
					}
				}
				goto case damageType.physical;
			case damageType.physical:
				if (armor > message.amount){
					tookDamage = false;
				} else {
					if (message.strength){
						message.amount *= 2.5f;
						message.force *= 10f;
					}
					if (!netBuffs[BuffType.noPhysicalDamage].boolValue){
						float netDam = Mathf.Max(message.amount - armor, 0);
						health -= netDam;
						impulse += netDam;
						tookDamage = true;
					}
				}
				Rigidbody2D body = GetComponent<Rigidbody2D>();
				if (body){
					body.AddForce(message.force * 100f);
				}
				break;
			case damageType.fire:
				if (!netBuffs[BuffType.fireproof].boolValue){
					health -= message.amount;
					tookDamage = true;
				}
				break;
			default:
				break;
			}
		}
		if (health <= 0 && (message.type == damageType.fire || message.type == damageType.cutting) && hitState != Controllable.HitState.dead){
			Die(message.type);
		}
		if (health <= -0.5 * maxHealth && hitState != Controllable.HitState.dead){
			Die(message.type);
		}
		if (message.type != damageType.fire){
			hitState = Controllable.AddHitState(hitState, Controllable.HitState.stun);
			hitStunCounter = 0.25f;
		}
		if (tookDamage && Random.Range(0.0f, 1.0f) < ouchFrequency){
			MessageSpeech speechMessage = new MessageSpeech();
			speechMessage.nimrodKey = true;
			switch(message.type){
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
		if (tookDamage){
			if (message.strength){
				return ImpactResult.strong;
			} else {
				return ImpactResult.normal;
			}
		} else {
			return ImpactResult.repel;
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
			ClaimsManager.Instance.WasDestroyed(gameObject);
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
		if (dizzyEffect != null){
			ClaimsManager.Instance.WasDestroyed(dizzyEffect);
			Destroy(dizzyEffect);
		}
		if (GameManager.Instance.playerObject == gameObject){
			GameManager.Instance.PlayerDeath();
		}
		hitState = Controllable.AddHitState(hitState, Controllable.HitState.dead);
		OccurrenceDeath occurrenceData = new OccurrenceDeath();
		occurrenceData.dead = gameObject;
		Toolbox.Instance.OccurenceFlag(gameObject, occurrenceData);
	}
	public override void ReceiveMessage(Message incoming){
		base.ReceiveMessage(incoming);
		if (incoming is MessageNetIntrinsic){
			MessageNetIntrinsic intrins = (MessageNetIntrinsic)incoming;
			// myIntrinsic = intrins.netIntrinsic;
			// intrinsics = intrins.intrinsics;
			if (intrins.netBuffs[BuffType.bonusHealth].floatValue > bonusHealth){
				health += intrins.netBuffs[BuffType.bonusHealth].floatValue;
			}
			bonusHealth = intrins.netBuffs[BuffType.bonusHealth].floatValue;
			armor = intrins.netBuffs[BuffType.armor].floatValue;
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
			if (gameObject == GameManager.Instance.playerObject){
				health += Time.deltaTime*3f;
			} else {
				health += Time.deltaTime;
			}
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
	public void KnockDown(){
		if (hitState >= Controllable.HitState.unconscious)
			return;
		hitState = Controllable.AddHitState(hitState, Controllable.HitState.unconscious);
		doubledOver = false;
		if (gameObject == GameManager.Instance.playerObject){
			downedTimer = 2f;
		} else {
			downedTimer = 10f;
		}
		Vector3 pivot = transform.position;
		pivot.y -= 0.15f;
		transform.RotateAround(pivot, new Vector3(0, 0, 1), -90);
		MessageHitstun message = new MessageHitstun();
		message.doubledOver = false;
		message.knockedDown = true;
		message.hitState = hitState;
		Toolbox.Instance.SendMessage(gameObject, this, message);

		dizzyEffect = Instantiate(Resources.Load("prefabs/fx/dizzy"), transform.position + new Vector3(0.1f, 0.1f, 0), Quaternion.identity) as GameObject;
		FollowGameObject dizzyFollower = dizzyEffect.GetComponent<FollowGameObject>();
		dizzyFollower.target = gameObject;
		dizzyFollower.Init();

		// foreach(Collider2D collider in backgroundColliders){
		// 	collider.enabled = false;
		// }
	}
	public void GetUp(){
		hitState = Controllable.RemoveHitState(hitState, Controllable.HitState.unconscious);
		doubledOver = false;
		Vector3 pivot = transform.position;
		pivot.x -= 0.15f;
		transform.RotateAround(pivot, new Vector3(0, 0, 1), 90);
		if (dizzyEffect != null){
			ClaimsManager.Instance.WasDestroyed(dizzyEffect);
			Destroy(dizzyEffect);
		}
		// foreach(Collider2D collider in backgroundColliders){
		// 	collider.enabled = true;
		// }
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
		Liquid blood = Liquid.LoadLiquid("blood");
		float initHeight = (position.y - transform.position.y ) + 0.15f;
		GameObject drop = Toolbox.Instance.SpawnDroplet(blood, 0.3f, gameObject, initHeight);
		Edible edible = drop.GetComponent<Edible>();
		edible.human = true;
	}
}
