using UnityEngine;
// using System.Collections;


public class Physical : MonoBehaviour, IMessagable {
	public enum mode{none, fly, ground, zip}
	public AudioClip[] impactSounds;
	public AudioClip[] landSounds;
	
	public PhysicalBootstrapper bootstrapper;
	
	private GameObject trueObject;
	public Rigidbody2D objectBody;
	public Collider2D objectCollider;
	private OrderByY order;
	
	public Rigidbody2D hingeBody;
	public GameObject hinge;
	
	public SliderJoint2D slider;
	
	public BoxCollider2D groundCollider;
	
	private Collider2D tomCollider;
	public mode currentMode;
	public float height;
	public bool ignoreCollisions;
	public bool doFly;
	private bool doGround;
	private bool doZip;
	private SpriteRenderer spriteRenderer;
	public float groundDrag;
	private float ziptime;
	private float defaultOrderOffset;

	 void Start() {
		InitValues();
		// ignore collisions between ground and all other objects
		GameObject[] physicals = GameObject.FindGameObjectsWithTag("Physical");
		foreach(GameObject phys in physicals){
			Physics2D.IgnoreCollision(groundCollider, phys.GetComponent<Collider2D>(), true);
			// special types of object ignore all collisions with other objects
			// this is true for e.g. liquid droplets 
			if (ignoreCollisions){
				Physics2D.IgnoreCollision(objectCollider, tomCollider, true);
				Physics2D.IgnoreCollision(groundCollider, tomCollider, true);
				foreach (Collider2D col in phys.GetComponentsInParent<Collider2D>())
					Physics2D.IgnoreCollision(objectCollider, col, true);
			}
		}
		Physics2D.IgnoreCollision(groundCollider, objectCollider, false);
		if (currentMode == mode.none)
			FlyMode();
	}
	
	public void InitValues(){
		spriteRenderer = GetComponent<SpriteRenderer>();
		slider = GetComponent<SliderJoint2D>();
		hinge = transform.GetChild(0).gameObject;
		trueObject = hinge.transform.GetChild(0).gameObject;
		groundCollider = GetComponent<BoxCollider2D>();
		objectCollider = trueObject.GetComponent<Collider2D>();
		order = objectBody.GetComponent<OrderByY>();
		if (order)
			defaultOrderOffset = order.offset;
	}

	public void Impact(Vector2 f){
		Vector2 force = f / (objectBody.mass / 25f);
		if (currentMode != mode.fly)
			FlyMode();
		if (impactSounds.Length > 0)
			GetComponent<AudioSource>().PlayOneShot(impactSounds[Random.Range(0, impactSounds.Length)]);
		bootstrapper.Set3Motion(new Vector3(force.x, force.y, force.y + 0.5f));
	}

	void FixedUpdate() {
		height = groundCollider.size.y / 2f - groundCollider.offset.y + hinge.transform.localPosition.y;
		if (currentMode == mode.fly){
			if (height < 0){
				Vector2 hingePosition = hinge.transform.localPosition;
				hingePosition.y = 0.1f + groundCollider.size.y / 2f - groundCollider.offset.y;
				hinge.transform.localPosition = hingePosition;
			}
			if (height < 0.1){
				GetComponent<Rigidbody2D>().drag = 5;
				GetComponent<Rigidbody2D>().mass = objectBody.mass;
			} else {
				GetComponent<Rigidbody2D>().drag = 0;
				GetComponent<Rigidbody2D>().mass = 1;
			} 
		}
		if (ziptime > 0){
			ziptime -= Time.fixedDeltaTime;
			if (ziptime <= 0){
				// Debug.Log("zip to fly timeout");
				doFly = true;
			}
		}

		if (currentMode == mode.fly || currentMode == mode.zip){
			trueObject.layer = 13;
		} else {
			trueObject.layer = 8;
		}
		if(hinge.transform.localPosition.y > 0.3 && hinge.transform.localPosition.y < 0.5){
			trueObject.layer = 10;
		}
		if(hinge.transform.localPosition.y > 0.5 && hinge.transform.localPosition.y < 0.7){
			trueObject.layer = 11;
		}
		if(hinge.transform.localPosition.y > 0.7 ){
			trueObject.layer = 12;
		}
		if (doGround){
			doGround = false;
			StartGroundMode();
		}
		if (doFly){
			doFly = false;
			StartFlyMode();
		}
		if (doZip){
			doZip = false;
			StartZipMode();
		}
	}

	public void GroundMode (){
		doGround = true;
	}

	public void FlyMode(){
		doFly = true;
	}
	
	public void ZipMode(){
		doZip = true;
	}

	public void StartGroundMode(){
		doGround = false;
		ziptime = 0f;
		// set tom collider. this is a temporary hack.
		if (GameManager.Instance.playerObject)
			tomCollider = GameManager.Instance.playerObject.GetComponent<EdgeCollider2D>();
		// enable ground-feet
		Physics2D.IgnoreCollision(objectCollider, tomCollider, true);
		Physics2D.IgnoreCollision(groundCollider, tomCollider, false);

		// set object gravity 0
		objectBody.gravityScale = 0;
		// fix slider
		JointTranslationLimits2D tempLimits = slider.limits;
		tempLimits.min = 0;
		tempLimits.max = hinge.transform.localPosition.y;
		slider.limits = tempLimits;
		slider.useLimits = true;
		// set ground friction
		GetComponent<Rigidbody2D>().drag = groundDrag;
		GetComponent<Rigidbody2D>().mass = objectBody.mass;
		// update mode
		currentMode = mode.ground;
		// remove vertical velocities
		Vector3 tempVelocity = GetComponent<Rigidbody2D>().velocity;
		tempVelocity.y = 0;
		GetComponent<Rigidbody2D>().velocity = tempVelocity;
		objectBody.velocity = tempVelocity;
		hingeBody.velocity = tempVelocity;
		foreach(Physical phys in FindObjectsOfType<Physical>()){
			if (phys.currentMode == mode.ground){
				if (ignoreCollisions){
					Physics2D.IgnoreCollision(objectCollider, phys.groundCollider, true);
				} else {
					Physics2D.IgnoreCollision(groundCollider, phys.groundCollider, false);
				}
			}
		}

		foreach(GameObject table in GameObject.FindGameObjectsWithTag("table")){
			foreach(Collider2D tableCollider in table.GetComponentsInChildren<Collider2D>()){
				if (tableCollider.isTrigger == false){
					Physics2D.IgnoreCollision(groundCollider, tableCollider, true);
					Physics2D.IgnoreCollision(objectCollider, tableCollider, true);
				}
			}
		}
		if (landSounds.Length > 0)
			GetComponent<AudioSource>().PlayOneShot(landSounds[Random.Range(0, landSounds.Length)]);
	}

	public void StartFlyMode(){
		doFly = false;
		ziptime = 0f;
		// set tom collider. this is a temporary hack.
		if (GameManager.Instance.playerObject){
			tomCollider = GameManager.Instance.playerObject.GetComponent<EdgeCollider2D>();
		}
		// disable player - ground collision
		Physics2D.IgnoreCollision(groundCollider, tomCollider, true);
		Physics2D.IgnoreCollision(objectCollider, tomCollider, true);
		// set object gravity on
		objectBody.gravityScale = GameManager.Instance.gravity;
		slider = GetComponent<SliderJoint2D>();
		slider.useLimits = false;
		//update mode
		currentMode = mode.fly;
		// set ground friction
		GetComponent<Rigidbody2D>().drag = 0;
		GetComponent<Rigidbody2D>().mass = 1;
		foreach(Physical phys in FindObjectsOfType<Physical>()){
			if (phys.gameObject != gameObject){
				Physics2D.IgnoreCollision(groundCollider, phys.groundCollider, true);
				Physics2D.IgnoreCollision(groundCollider, phys.objectCollider, true);
				if (ignoreCollisions){
					Physics2D.IgnoreCollision(objectCollider, phys.objectCollider, true);
				}
			}
		}
		foreach(GameObject table in GameObject.FindGameObjectsWithTag("table")){
			foreach(Collider2D tableCollider in table.GetComponentsInParent<Collider2D>()){
				if (tableCollider.isTrigger == false){
					Physics2D.IgnoreCollision(groundCollider, tableCollider, true);
					Physics2D.IgnoreCollision(objectCollider, tableCollider, true);
				}
			}
		}
	}
	public void StartZipMode(){
		doZip = false;
		doFly = false;
		StartFlyMode();
		objectBody.gravityScale = 0;
		ziptime = 0.5f;
		currentMode = mode.zip;
	}
	

	void OnCollisionEnter2D(Collision2D coll){
		// Debug.Log("ground collision between "+gameObject.name+" "+coll.gameObject.name);
		if (coll.relativeVelocity.magnitude > 0.5){
			if (impactSounds.Length > 0){
				GetComponent<AudioSource>().PlayOneShot(impactSounds[Random.Range(0, impactSounds.Length)]);
			}
		}
		// this part right here is pretty essential to whether the object can fall off the world, or what
		if (coll.collider == objectCollider){
			GroundMode();
			BroadcastMessage("OnGroundImpact", this, SendMessageOptions.DontRequireReceiver);
		} else {
			
		}
	}

	void OnCollisionStay2D(Collision2D coll){
		if(coll.collider == objectCollider && coll.relativeVelocity.magnitude < 0.01 && currentMode != mode.ground){
			GroundMode();
		}
	}

	void OnTriggerEnter2D(Collider2D coll){
		if (coll.tag == "table" && coll.gameObject != gameObject && currentMode != mode.zip){
			Table table = coll.GetComponent<Table>();
			ActivateTableCollider(table);
		}
	}
	
	void OnTriggerExit2D(Collider2D coll){
		if (coll.tag == "table" && coll.gameObject != gameObject && currentMode != mode.zip){
			Table table = coll.GetComponent<Table>();
			DeactivateTableCollider(table);
		}
	}

	void ActivateTableCollider(Table table){
		if (groundCollider){
			spriteRenderer.enabled = false;
			groundCollider.size = new Vector2(0.07f, 0.02f + table.height);
			Collider2D[] tableColliders = table.gameObject.GetComponentsInParent<Collider2D>();
			foreach (Collider2D tableCollider in tableColliders){
				if (tableCollider.isTrigger == false){
					Physics2D.IgnoreCollision(groundCollider, tableCollider, true);
					Physics2D.IgnoreCollision(objectCollider, tableCollider, true);
				}
			}
			// OrderByY order = objectBody.GetComponent<OrderByY>();
			OrderByY orderTable = table.GetComponentInParent<OrderByY>();
			if (order != null && orderTable != null){
				order.offset = -1f * (objectBody.transform.position.y - table.transform.position.y - 0.01f);
			}
		}
	}
	
	void DeactivateTableCollider(Table table){
		if (groundCollider){
			spriteRenderer.enabled = true;
			groundCollider.size = new Vector2(0.07f, 0.02f);
			FlyMode();
			Collider2D[] tableColliders = table.gameObject.GetComponentsInParent<Collider2D>();
			foreach (Collider2D tableCollider in tableColliders){
				if (tableCollider.isTrigger == false){
					Physics2D.IgnoreCollision(groundCollider, tableCollider, false);
				}
			}
			// OrderByY order = objectBody.GetComponent<OrderByY>();
			if (order){
				order.offset = defaultOrderOffset;
			}
		}
	}

	public void ReceiveMessage(Message message){
		if (message is MessageDamage){
			MessageDamage dam = (MessageDamage)message;
			if (dam.type != damageType.fire){
				Impact(dam.force);
				if (dam.impactor)
					dam.impactor.PlayImpactSound();
			}
		}
	}

}
