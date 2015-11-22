using UnityEngine;
// using System.Collections;

public class PhysicalBootstrapper : MonoBehaviour {
	public AudioClip[] impactSounds;
	public AudioClip[] landSounds;
	public AudioClip[] scrapeSounds;
	private GameObject hingeObject;
	private GameObject groundObject;
	private Rigidbody2D groundBody;
	private Rigidbody2D hingeBody;
	private HingeJoint2D hingeJoint2D;
	private SliderJoint2D sliderJoint2D;
	public Physical physical;
	private Collider2D tomCollider;
	public float initHeight = 0.1f;
	public float groundDrag = 10f;
	public Vector2 initVelocity;
	public bool ignoreCollisions;
	public bool doInit = true;
	private Vector3 setV;
	private BoxCollider2D groundCollider;

	public void Start () {
		tag = "Physical";
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

	void LoadInit(){
		Start();
	}

	public void DestroyPhysical(){
		transform.parent = null;
		Destroy(groundObject);
		GetComponent<Rigidbody2D>().gravityScale = 0;
		Physics2D.IgnoreCollision(tomCollider, GetComponent<Collider2D>(),false);
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
//		Bounds spriteBounds = spriteRenderer.sprite.bounds;
//		height += spriteBounds.extents.y;
		tempPos.y = height;
		groundPos.y -= height;
		hingeObject.transform.localPosition = tempPos;
		groundObject.transform.position = groundPos;
		
		//rigidbody 2D
		groundBody = groundObject.AddComponent<Rigidbody2D>();
		groundBody.mass = 1f;
		groundBody.drag = groundDrag;
		groundBody.angularDrag = 0.05f;
		groundBody.gravityScale = 0;
		groundBody.freezeRotation = true;
		
		//box collider
		groundCollider = groundObject.AddComponent<BoxCollider2D>();
		groundCollider.size = new Vector2(0.1606f, 0.05f);
		groundCollider.offset = new Vector2(0.0f, -0.025f);
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
		groundPhysical.groundDrag = groundDrag;

		//Slider joint
		sliderJoint2D = groundObject.AddComponent<SliderJoint2D>();
		sliderJoint2D.enableCollision = true;
		sliderJoint2D.angle = 90f;
		sliderJoint2D.connectedBody = hingeBody;
		
		groundPhysical.impactSounds = impactSounds;
		groundPhysical.landSounds = landSounds;
		
		physical = groundPhysical;
		groundPhysical.bootstrapper = this;
		Set3Motion(new Vector3(initialVelocity.x, initialVelocity.y, initialVelocity.z));
	}

	void OnCollisionEnter2D(Collision2D coll){
		if (coll.relativeVelocity.magnitude > 0.5){
			if (impactSounds.Length > 0){
				GetComponent<AudioSource>().PlayOneShot(impactSounds[Random.Range(0,impactSounds.Length)]);
			}
		}
	}

	public void Set3Motion(Vector3 velocity){
		setV = velocity;
	}

	void FixedUpdate(){
		if (setV != Vector3.zero){
			Vector2 groundVelocity = new Vector2(setV.x, setV.y);
			Vector2 objectVelocity = new Vector2(setV.x, setV.z + setV.y);
			physical.objectBody.velocity = objectVelocity;
			Rigidbody2D groundBody = physical.GetComponent<Rigidbody2D>();
			groundBody.velocity = groundVelocity;
			setV = Vector3.zero;
		}
	}

	void OnDestroy(){
		if (physical)
			Destroy(physical.gameObject);
	}

}
