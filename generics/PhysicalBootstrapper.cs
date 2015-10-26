using UnityEngine;
using System.Collections;

public class PhysicalBootstrapper : MonoBehaviour {

	public AudioClip[] impactSounds;
	public AudioClip[] landSounds;

	private GameObject hingeObject;
	private GameObject groundObject;
	private Rigidbody2D groundBody;
	private Rigidbody2D hingeBody;
	private HingeJoint2D hingeJoint2D;
	private SliderJoint2D sliderJoint2D;
	public Physical physical;
	private Collider2D tomCollider;
	private SpriteRenderer spriteRenderer;
	public float initHeight;
	public Vector2 initVelocity;
	public bool ignoreCollisions;
	public bool doInit = true;

	private float setVx;
	private float setVy;
	private float setVz;

	public void Start () {
		tag = "Physical";
		spriteRenderer = GetComponent<SpriteRenderer>();
		GetComponent<Renderer>().sortingLayerName="main";

		//this will need to be modified when we have more humanoids!!!!:
		GameObject tom = GameObject.Find("Tom");
		if (tom)
			tomCollider = GameObject.Find("Tom").GetComponent<Collider2D>(); 
		if (doInit)
			InitPhysical(initHeight, initVelocity);

		if (impactSounds.Length > 0){
			Toolbox.Instance.SetUpAudioSource(gameObject);
		}
	}

	public void DestroyPhysical(){
		transform.parent = null;
		Destroy(groundObject);
		GetComponent<Rigidbody2D>().gravityScale = 0;
		Physics2D.IgnoreCollision(tomCollider,GetComponent<Collider2D>(),false);
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
		transform.parent = hingeObject.transform;

		hingeBody = hingeObject.AddComponent<Rigidbody2D>();
		hingeJoint2D = hingeObject.AddComponent<HingeJoint2D>();
		hingeBody.mass = 1;
		hingeBody.drag = 1;
		hingeBody.angularDrag = 1;
		hingeBody.gravityScale = 0;
		hingeJoint2D.connectedBody = GetComponent<Rigidbody2D>();
		
		// Set up ground object
		groundObject = new GameObject(name + " Ground");
		groundObject.transform.position = initPos;
		Toolbox.Instance.SetUpAudioSource(groundObject);

		hingeObject.transform.parent = groundObject.transform;
		Vector2 tempPos = Vector2.zero;
		Bounds spriteBounds = spriteRenderer.sprite.bounds;
//		height = height + spriteBounds.extents.y;
		tempPos.y = height;
		groundPos.y -= height;
		hingeObject.transform.localPosition = tempPos;
		groundObject.transform.position = groundPos;
		Debug.Log("initial height "+height.ToString());
		
		//rigidbody 2D
		groundBody = groundObject.AddComponent<Rigidbody2D>();
		groundBody.mass = 1f;
		groundBody.drag = 1f;
		groundBody.angularDrag = 0.05f;
		groundBody.gravityScale = 0;
		groundBody.fixedAngle = true;
		
		//box collider
		BoxCollider2D groundCollider = groundObject.AddComponent<BoxCollider2D>();
		groundCollider.size = new Vector2(0.1606f, 0.05f);
		groundCollider.offset = new Vector2(0.0f, -0.05f);
		groundCollider.sharedMaterial = Resources.Load<PhysicsMaterial2D>("ground"); 

		//sprite renderer
		SpriteRenderer groundSprite = groundObject.AddComponent<SpriteRenderer>();
		groundSprite.sprite = Resources.Load<Sprite>("shadow");
		groundSprite.sortingLayerName="ground";
		
		//Physical
		Physical groundPhysical = groundObject.AddComponent<Physical>();

		groundPhysical.ignoreCollisions = ignoreCollisions;
		groundPhysical.objectBody = GetComponent<Rigidbody2D>();
		groundPhysical.hingeBody = hingeBody;

		//Slider joint
		sliderJoint2D = groundObject.AddComponent<SliderJoint2D>();
		sliderJoint2D.collideConnected = true;
		sliderJoint2D.angle = 90f;
		sliderJoint2D.connectedBody = hingeBody;
		
		groundPhysical.impactSounds = impactSounds;
		groundPhysical.landSounds = landSounds;
		
		physical = groundPhysical;
		Set3Motion(initialVelocity.x, initialVelocity.y, initialVelocity.z);
	}

	void OnCollisionEnter2D(Collision2D coll){
		if (coll.relativeVelocity.magnitude > 0.5){
			if (impactSounds.Length > 0){
				GetComponent<AudioSource>().PlayOneShot(impactSounds[Random.Range(0,impactSounds.Length)]);
			}
		}
	}

	public void Set3Motion(float vx, float vy, float vz){
		setVx = vx;
		setVy = vy;
		setVz = vz;
	}

	void FixedUpdate(){
		if (setVx != 0 || setVy != 0 || setVz != 0){
			Vector2 groundVelocity = new Vector2(setVx, setVy);
			Vector2 objectVelocity = new Vector2(setVx, setVz + setVy);
			physical.objectBody.velocity = objectVelocity;
			Rigidbody2D groundBody = physical.GetComponent<Rigidbody2D>();
			groundBody.velocity = groundVelocity;
			setVx = setVy = setVz = 0;
		}
	}

//	public void SetVelocity(Vector2 velocity){
//		Vector2 groundVelocity = Vector2.zero;
//		Vector2 objectVelocity = Vector2.zero;
//
//		groundVelocity.x = velocity.x;
//		objectVelocity.y = velocity.y;
//
//		groundBody.velocity = groundVelocity;
//		GetComponent<Rigidbody2D>().velocity = objectVelocity;
//
//	}

	void OnDestroy(){
		if (physical)
			Destroy(physical.gameObject);
	}
}
