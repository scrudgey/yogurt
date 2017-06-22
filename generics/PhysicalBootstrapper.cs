using UnityEngine;
using System.Collections.Generic;
public class PhysicalBootstrapper : MonoBehaviour {
	public AudioClip[] impactSounds;
	public AudioClip[] landSounds;
	public AudioClip[] scrapeSounds;
	private GameObject hingeObject;
	private Rigidbody2D hingeBody;
	private HingeJoint2D hingeJoint2D;
	private GameObject groundObject;
	private Rigidbody2D groundBody;
	private BoxCollider2D groundCollider;
	private GameObject horizon;
	private SliderJoint2D sliderJoint2D;
	public Physical physical;
	public float initHeight = 0.01f;
	public float groundDrag = 10f;
	public Vector2 initVelocity;
	public bool ignoreCollisions;
	public bool doInit = true;
	private Vector3 setV;
	public GameObject thrownBy;
	public Collider2D objectCollider;
	public void Start () {
		tag = "Physical";
		GetComponent<Renderer>().sortingLayerName="main";
		if (doInit)
			InitPhysical(initHeight, initVelocity);
		if (impactSounds.Length > 0){
			Toolbox.Instance.SetUpAudioSource(gameObject);
		}
	}
	void LoadInit(){
		Start();
	}
	public void DestroyPhysical(){
		transform.SetParent(null);
		Destroy(groundObject);
		Rigidbody2D body = GetComponent<Rigidbody2D>();
		body.gravityScale = 0;
		body.velocity = Vector2.zero;
		physical = null;
		doInit = false;
	}
	public void InitPhysical(float height, Vector3 initialVelocity){
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
		groundObject.layer = 8;
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
		objectCollider = GetComponent<Collider2D>();

		hingeObject.transform.SetParent(groundObject.transform);
		Vector2 tempPos = Vector2.zero;
		float theta = Vector3.Angle(transform.up, Vector2.up) * (6.28f / 360.0f);
		float offset = objectCollider.bounds.extents.y - objectCollider.offset.x * Mathf.Sin(theta) + objectCollider.offset.y * Mathf.Cos(theta);
		height = Mathf.Max(height, offset);
		// if (objectCollider.offset.y > height)
		// 	height += objectCollider.offset.y;
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

		//Slider joint
		sliderJoint2D = groundObject.AddComponent<SliderJoint2D>();
		sliderJoint2D.enableCollision = false;
		sliderJoint2D.angle = 90f;
		sliderJoint2D.connectedBody = hingeBody;
		
		groundPhysical.impactSounds = impactSounds;
		groundPhysical.landSounds = landSounds;
		
		physical = groundPhysical;
		physical.InitValues();
		groundPhysical.bootstrapper = this;
		Set3Motion(new Vector3(initialVelocity.x, initialVelocity.y, initialVelocity.z));
	}
	void OnCollisionEnter2D(Collision2D coll){
		if (coll.relativeVelocity.magnitude > 0.5){
			if (impactSounds.Length > 0){
				GetComponent<AudioSource>().PlayOneShot(impactSounds[Random.Range(0, impactSounds.Length)]);
			}
			Toolbox.Instance.DataFlag(gameObject, 50f, 0f, 0f, 0f, 0f);
		}
		if (ignoreCollisions)
			return;
		if (coll.gameObject == groundObject){
			// Debug.Log("I collided with the ground.");
		} else if (coll.gameObject == horizon){
			if (coll.relativeVelocity.magnitude > 0.1){
				physical.GroundMode();
			} else {
				physical.suppressLandSound = true;
				physical.GroundMode();
			}
			physical.BroadcastMessage("OnGroundImpact", physical, SendMessageOptions.DontRequireReceiver);
		} else {
			if (physical.currentMode == Physical.mode.zip){
				// Debug.Log("physical bootstrapper collision: "+gameObject.name+" + "+coll.gameObject.name);
				MessageDamage message = new MessageDamage();
				message.responsibleParty = new List<GameObject>();
				message.responsibleParty.Add(thrownBy);
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
	}
	void OnDestroy(){
		if (physical)
			Destroy(physical.gameObject);
	}
}
