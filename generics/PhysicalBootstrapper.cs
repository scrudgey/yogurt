using UnityEngine;
public class PhysicalBootstrapper : MonoBehaviour, ISaveable {
	public AudioClip[] impactSounds;
	public AudioClip[] landSounds;
	public AudioClip[] scrapeSounds;
	private GameObject hingeObject;
	private Rigidbody2D hingeBody;
	private HingeJoint2D hingeJoint2D;
	private GameObject groundObject;
	private Rigidbody2D groundBody;
	public BoxCollider2D groundCollider;
	private GameObject horizon;
	private SliderJoint2D sliderJoint2D;
	public Physical physical;
	public float initHeight = 0.01f;
	public float groundDrag = 10f;
	public Vector2 initVelocity;
	public bool doInit = true;
	private Vector3 setV;
	public GameObject thrownBy;
	public Collider2D objectCollider;
	public PersistentComponent loadData;
	public bool doLoad;
	private bool isQuitting = false;
	public bool impactsMiss;
	public bool noCollisions;
	public void Start(){
		tag = "Physical";
		GetComponent<Renderer>().sortingLayerName="main";
		if (doInit)
			InitPhysical(initHeight, initVelocity);
		if (impactSounds.Length > 0){
			Toolbox.Instance.SetUpAudioSource(gameObject);
		}
	}
	public void DestroyPhysical(){
		transform.SetParent(null);
		ClaimsManager.Instance.WasDestroyed(groundObject);
		Destroy(groundObject);
		Rigidbody2D body = GetComponent<Rigidbody2D>();
		body.gravityScale = 0;
		body.velocity = Vector2.zero;
		physical = null;
		doInit = false;
	}
	public void InitPhysical(float height, Vector3 initialVelocity){
		// Debug.Log(name + " init physical");
		doInit = false; 
		Vector2 initPos = transform.position;
		Vector2 groundPos = transform.position;

		// Set up hinge
		hingeObject = new GameObject("hinge");
		hingeObject.transform.position = initPos;
		transform.SetParent(hingeObject.transform);

		hingeBody = hingeObject.AddComponent<Rigidbody2D>();
		hingeJoint2D = hingeObject.AddComponent<HingeJoint2D>();
		hingeBody.mass = 1;
		hingeBody.drag = 1;
		hingeBody.angularDrag = 1;
		hingeBody.gravityScale = 0;
		hingeJoint2D.connectedBody = GetComponent<Rigidbody2D>();
		
		// Set up ground object
		groundObject = new GameObject(name + " Ground");
		groundObject.tag = "footprint";
		if (noCollisions){
			groundObject.layer = 15;
		} else {
			groundObject.layer = 16;
		}
		groundObject.transform.position = initPos;
		Toolbox.Instance.SetUpAudioSource(groundObject);
		//rigidbody 2D
		groundBody = groundObject.AddComponent<Rigidbody2D>();
		groundBody.mass = 1f;
		groundBody.drag = groundDrag;
		groundBody.angularDrag = 0.05f;
		groundBody.gravityScale = 0;
		groundBody.freezeRotation = true;
		//box collider
		groundCollider = groundObject.AddComponent<BoxCollider2D>();
		groundCollider.size = new Vector2(0.07f, 0.05f);
		groundCollider.sharedMaterial = Resources.Load<PhysicsMaterial2D>("ground"); 
		foreach (Table table in Object.FindObjectsOfType<Table>()){
			Collider2D tableCollider = table.transform.parent.GetComponent<Collider2D>();
			Physics2D.IgnoreCollision(groundCollider, tableCollider, true);
		}

		horizon = new GameObject("horizon");
		horizon.layer = 9;
		Rigidbody2D shadowBody = horizon.AddComponent<Rigidbody2D>();
		shadowBody.bodyType = RigidbodyType2D.Kinematic;
		horizon.AddComponent<EdgeCollider2D>();
		horizon.transform.position = initPos;
		horizon.transform.SetParent(groundObject.transform);
		shadowBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		objectCollider = GetComponent<Collider2D>();

		hingeObject.transform.SetParent(groundObject.transform);
		Vector2 tempPos = Vector2.zero;
		// float theta = Vector3.Angle(transform.up, Vector2.up) * (6.28f / 360.0f);
		// float offset = objectCollider.bounds.extents.y - objectCollider.offset.x * Mathf.Sin(theta) + objectCollider.offset.y * Mathf.Cos(theta);
		// height = Mathf.Max(height, offset);

		tempPos.y = height;
		groundPos.y -= height;
		hingeObject.transform.localPosition = tempPos;
		groundObject.transform.position = groundPos;
		
		//sprite renderer
		SpriteRenderer groundSprite = groundObject.AddComponent<SpriteRenderer>();
		groundSprite.sprite = Resources.Load<Sprite>("shadow");
		groundSprite.sortingLayerName="ground";
		
		//Physical
		Physical groundPhysical = groundObject.AddComponent<Physical>();
		groundPhysical.objectBody = GetComponent<Rigidbody2D>();
		groundPhysical.hingeBody = hingeBody;
		groundPhysical.groundDrag = groundDrag;
		groundPhysical.groundCollider = groundCollider;
		groundPhysical.objectRenderer = GetComponent<SpriteRenderer>();
		// Debug.Log(groundPhysical.objectRenderer);

		//Slider joint
		sliderJoint2D = groundObject.AddComponent<SliderJoint2D>();
		sliderJoint2D.autoConfigureAngle = false;
		sliderJoint2D.autoConfigureConnectedAnchor = false;
		sliderJoint2D.enableCollision = false;
		sliderJoint2D.angle = 90f;
		sliderJoint2D.connectedBody = hingeBody;
		
		groundPhysical.impactSounds = impactSounds;
		groundPhysical.landSounds = landSounds;
		
		physical = groundPhysical;
		physical.InitValues();
		physical.noCollisions = noCollisions;
		physical.impactsMiss = impactsMiss;
		groundPhysical.bootstrapper = this;
		Set3Motion(new Vector3(initialVelocity.x, initialVelocity.y, initialVelocity.z));
	}
	void OnCollisionStay2D(Collision2D coll){
		if (coll.gameObject != horizon)
			return;
		// if (physical.slider.useLimits == false){
		// 	float dist = coll.collider.Distance(objectCollider).distance;
		// 	physical.SetSliderLimit(Mathf.Abs(-2f * dist));
		// }
	}
	void OnCollisionEnter2D(Collision2D coll){
		if (coll.relativeVelocity.magnitude > 0.5){
			if (impactSounds.Length > 0){
				GetComponent<AudioSource>().PlayOneShot(impactSounds[Random.Range(0, impactSounds.Length)]);
			}
			EventData data = Toolbox.Instance.DataFlag(gameObject, chaos:1);
			data.noun = "collision";
			data.whatHappened = Toolbox.Instance.CloneRemover(coll.gameObject.name)+" collided with "+Toolbox.Instance.CloneRemover(gameObject.name);
		}
		// if (ignoreCollisions)
		// 	return;
		if (coll.gameObject == groundObject){
			// Debug.Log("I collided with the ground.");
		} else if (coll.gameObject == horizon){
			if (coll.relativeVelocity.magnitude > 0.1){
				physical.GroundMode();
				physical.BroadcastMessage("OnGroundImpact", physical, SendMessageOptions.DontRequireReceiver);
			} else {
				physical.suppressLandSound = true;
				physical.GroundMode();
			}
		} else {
			if (physical == null)
				return;
			if (physical.currentMode == Physical.mode.zip){
				// Debug.Log("physical bootstrapper collision: "+gameObject.name+" + "+coll.gameObject.name);
				MessageDamage message = new MessageDamage();
				message.responsibleParty = thrownBy;
				message.force = physical.objectBody.velocity;
				message.amount = 20f;
				message.type = damageType.physical;
				Toolbox.Instance.SendMessage(coll.gameObject, this, message);

				physical.FlyMode();
				GameObject speaker = Instantiate(Resources.Load("Speaker"), transform.position, Quaternion.identity) as GameObject;
				speaker.GetComponent<AudioSource>().clip = Resources.Load("sounds/8bit_impact1", typeof(AudioClip)) as AudioClip;
				speaker.GetComponent<AudioSource>().Play();
				Rebound();
			}
		}
	}
	public void Set3Motion(Vector3 velocity){
		setV = velocity;
	}
	public void Set3MotionImmediate(Vector3 velocity){
		Vector2 groundVelocity = new Vector2(velocity.x, velocity.y);
		Vector2 objectVelocity = new Vector2(velocity.x, velocity.z + velocity.y);
		physical.objectBody.velocity = objectVelocity;
		groundBody.velocity = groundVelocity;
	}
	public void Rebound(){
		Vector2 objectVelocity = physical.objectBody.velocity;
		objectVelocity.y = Random.Range(0.5f, 0.8f);
		physical.objectBody.velocity = objectVelocity;
		physical.objectBody.angularVelocity = Random.Range(360, 800);
		physical.ClearTempColliders();
		// Debug.Log("angular vel:" + physical.objectBody.angularVelocity.ToString());
		// Debug.Log("y vel:" + objectVelocity.y.ToString());
	}
	void FixedUpdate(){
		if (setV != Vector3.zero){
			Vector2 groundVelocity = new Vector2(setV.x, setV.y);
			Vector2 objectVelocity = new Vector2(setV.x, setV.z + setV.y);
			physical.objectBody.velocity = objectVelocity;
			groundBody.velocity = groundVelocity;
			setV = Vector3.zero;
		}
		if (doLoad){
			doLoad = false;
			initHeight = 0;
			initVelocity = Vector3.zero;
			GetComponent<Rigidbody2D>().velocity = Vector3.zero;
			if (loadData == null)
				return;
			if (physical != null){
				physical.currentMode = (Physical.mode)loadData.ints["mode"];
				physical.transform.position = loadData.vectors["groundPosition"];
				physical.hinge.transform.localPosition = loadData.vectors["objectPosition"];
				Vector2 offset = new Vector2(loadData.floats["horizonOffsetX"], loadData.floats["horizonOffsetY"]);
				physical.horizonCollider.offset = offset;
				if (physical.currentMode == Physical.mode.fly){
					physical.StartFlyMode();
				}
				if (physical.currentMode == Physical.mode.zip){
					physical.StartZipMode();
				}
				if (physical.currentMode == Physical.mode.ground){
					physical.StartGroundMode();
				}
				physical.objectBody.velocity = Vector3.zero;
			}
		}
	}
	void OnApplicationQuit(){
		isQuitting = true;
	}
	void OnDestroy(){
		if (isQuitting)
			return;
		if (physical && physical.gameObject){
			ClaimsManager.Instance.WasDestroyed(physical.gameObject);
			Destroy(physical.gameObject);
		}
	}
	public void SaveData(PersistentComponent data){
		data.bools["doInit"] = doInit;
		data.bools["physical"] = false;
		if (physical != null){
			if (!physical.isActiveAndEnabled)
				return;
			data.bools["physical"] = true;
			data.ints["mode"] = (int)physical.currentMode;
			data.vectors["groundPosition"] = physical.transform.position;
			data.vectors["objectPosition"] = physical.hinge.transform.localPosition;

			data.floats["horizonOffsetX"] = physical.horizonCollider.offset.x;
			data.floats["horizonOffsetY"] = physical.horizonCollider.offset.y;
		}
	}
	public void LoadData(PersistentComponent data){
		if (data.bools["physical"]){
			loadData = data;
		}
		doLoad = true;
	}
}
