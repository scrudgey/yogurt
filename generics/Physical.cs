using UnityEngine;
using System.Collections;

public class Physical : MonoBehaviour {

	public AudioClip[] impactSounds;
	public AudioClip[] landSounds;

	private SliderJoint2D slider;
	private GameObject trueObject;
	public Collider2D objectCollider;
	private Collider2D groundCollider;
	private Collider2D tomCollider;
	public Rigidbody2D objectBody;
	public Rigidbody2D hingeBody;
	private GameObject hinge;
	public enum mode{fly, ground, dormant}
	public mode currentMode;
	public float height;
	private float groundY;
	public float groundYVelocity;
	public bool ignoreCollisions;

	// Use this for initialization
	void Start () {

		if (impactSounds.Length + landSounds.Length > 0){
			if (!GetComponent<AudioSource>()){
				gameObject.AddComponent<AudioSource>();
			}
			GetComponent<AudioSource>().rolloffMode = AudioRolloffMode.Logarithmic;
			GetComponent<AudioSource>().minDistance = 0.4f;
			GetComponent<AudioSource>().maxDistance = 5.42f;
		}

		slider = GetComponent<SliderJoint2D>();
		trueObject = transform.GetChild(0).transform.GetChild(0).gameObject;
		hinge = transform.GetChild(0).gameObject;

		groundCollider = GetComponent<Collider2D>();
		objectCollider = trueObject.GetComponent<Collider2D>();
		
		//TODO: this will need to be modified when we have more humanoids!!!!:
		GameObject tom = GameObject.Find("Tom");
		if (tom)
			tomCollider = GameObject.Find("Tom").GetComponent<EdgeCollider2D>();

		// ignore collisions between ground and all other objects
		GameObject[] physicals = GameObject.FindGameObjectsWithTag("Physical");
		foreach(GameObject phys in physicals){
			Physics2D.IgnoreCollision(groundCollider, phys.GetComponent<Collider2D>(), true);
			//special types of object ignore all collisions with other objects
			if (ignoreCollisions){
				Physics2D.IgnoreCollision(objectCollider, tomCollider, true);
				Physics2D.IgnoreCollision(groundCollider, tomCollider, true);
				foreach (Collider2D col in phys.GetComponentsInParent<Collider2D>())
					Physics2D.IgnoreCollision(objectCollider, col, true);
			}
		}
		Physics2D.IgnoreCollision(groundCollider, objectCollider, false);

		FlyMode();

	}

	public void Impact(Vector2 f){
		// objectbody is the main object
//		objectBody.AddTorque(-100);
		//		objectBody.AddForce(f);
		if (currentMode != mode.fly)
			FlyMode();
		GetComponent<Rigidbody2D>().AddForce(f * GetComponent<Rigidbody2D>().mass/objectBody.mass);
		objectBody.AddForce(f );//+ Vector2.up * 200);
		Vector2 tempV = objectBody.velocity;
//		tempV.y += 0.5f;
		tempV.y += GetComponent<Rigidbody2D>().velocity.y + 0.5f;
		objectBody.velocity = tempV;
//		groundYVelocity = tempV.y;
//		objectBody.AddForce(Vector2.up * f.magnitude);
		if (impactSounds.Length > 0)
			GetComponent<AudioSource>().PlayOneShot(impactSounds[Random.Range(0,impactSounds.Length)]);
		Destructible destructible = trueObject.GetComponent<Destructible>();
		if (destructible){
			float damage =   f.magnitude / 200;
			destructible.TakeDamage(Destructible.damageType.physical, damage);
			Debug.Log("Impact damage on " + gameObject.name + " to the tune of " + damage.ToString());
		}
	}

	public void DirectionImpact(Vector2 f, Vector2 p){
		// test thing
		FlyMode();
		objectBody.AddForceAtPosition(f,p);
	}

	void FixedUpdate () {
		if (currentMode == mode.fly){
//			Vector3 tempPosition = transform.position;
//			tempPosition.y = groundY;
//			transform.position = tempPosition;
//
//			groundY = groundY + groundYVelocity * Time.deltaTime;

			if(hinge.transform.localPosition.y < 0.1){
				GetComponent<Rigidbody2D>().drag = 5;
				GetComponent<Rigidbody2D>().mass = objectBody.mass;
			} else {
				GetComponent<Rigidbody2D>().drag = 0;
				GetComponent<Rigidbody2D>().mass = 1;
			} 
		}
		// collision layer stuff
		if(hinge.transform.localPosition.y < 0.2){
			trueObject.layer = 9;
		}
		if(hinge.transform.localPosition.y > 0.2 && hinge.transform.localPosition.y < 0.4){
			trueObject.layer = 10;
		}
		if(hinge.transform.localPosition.y > 0.4 && hinge.transform.localPosition.y < 0.6){
			trueObject.layer = 11;
		}
		if(hinge.transform.localPosition.y > 0.6 ){
			trueObject.layer = 12;
		}
		height = hinge.transform.localPosition.y;
	}

	public void GroundMode (){

		if (landSounds.Length > 0)
			GetComponent<AudioSource>().PlayOneShot(landSounds[Random.Range(0,landSounds.Length)]);

		Vector3 hingePosition = hinge.transform.localPosition;

		// stop colliding player and object
		Physics2D.IgnoreCollision(objectCollider, tomCollider, true);
		// enable player - ground collision
		Physics2D.IgnoreCollision(groundCollider, tomCollider, false);
		// set object gravity 0
		objectBody.gravityScale = 0;
		// fix slider
		JointTranslationLimits2D tempLimits = slider.limits;
		tempLimits.min = 0;
		tempLimits.max = hinge.transform.localPosition.y/2;
		slider.limits = tempLimits;
		slider.useLimits = true;

		// set ground friction
		GetComponent<Rigidbody2D>().drag = 10;
		GetComponent<Rigidbody2D>().mass = objectBody.mass;

		// update mode
		currentMode = mode.ground;

		// remove vertical velocities
		Vector3 tempVelocity = GetComponent<Rigidbody2D>().velocity;
		tempVelocity.y = 0;
		GetComponent<Rigidbody2D>().velocity = tempVelocity;
		objectBody.velocity = tempVelocity;
		hingeBody.velocity = tempVelocity;

		
//		Vector3 tempPosition = transform.position;
//		tempPosition.y = groundY;
//		transform.position = tempPosition;
//		hinge.transform.localPosition = hingePosition;

		Physical[] physicals = FindObjectsOfType<Physical>();
		foreach(Physical phys in physicals){
			if (phys.currentMode == mode.ground){
//				Physics2D.IgnoreCollision(GetComponent<Collider2D>(), phys.GetComponent<Collider2D>(), false);
				Physics2D.IgnoreCollision(groundCollider, phys.GetComponent<Collider2D>(), false);
				//special types of object ignore all collisions with other objects
				if (ignoreCollisions){
					Physics2D.IgnoreCollision(objectCollider, phys.GetComponent<Collider2D>(), true);
					Physics2D.IgnoreCollision(objectCollider, tomCollider,true);
				}
			}
		}

	}

	public void FlyMode(){

		// enable colliding player and object
		if (!ignoreCollisions)
			Physics2D.IgnoreCollision(objectCollider, tomCollider, false);
		// disable player - ground collision
		Physics2D.IgnoreCollision(groundCollider, tomCollider, true);
		Physics2D.IgnoreCollision(objectCollider, tomCollider, true);

		// set object gravity on
		objectBody.gravityScale = GameManager.Instance.gravity;

		//unfix sliderLimits = slider.limits;
		slider = GetComponent<SliderJoint2D>();
		slider.useLimits = false;

		//fix ground Y coordinate
		groundY = transform.position.y;
//		groundYVelocity = GetComponent<Rigidbody2D>().velocity.y;
//		groundYVelocity = objectBody.velocity.y;

		//update mode
		currentMode = mode.fly;

		// set ground friction
		GetComponent<Rigidbody2D>().drag = 0;
		GetComponent<Rigidbody2D>().mass = 1;

		Physical[] physicals = FindObjectsOfType<Physical>();
		foreach(Physical phys in physicals){
//			Physics2D.IgnoreCollision(GetComponent<Collider2D>(), phys.GetComponent<Collider2D>(), true);
			Physics2D.IgnoreCollision(groundCollider, phys.GetComponent<Collider2D>(), true);

		}

	}
	

	void OnCollisionEnter2D(Collision2D coll){
		if (coll.relativeVelocity.magnitude > 0.5){
			if (impactSounds.Length > 0){
				GetComponent<AudioSource>().PlayOneShot(impactSounds[Random.Range(0, impactSounds.Length)]);
			}
		}
		if (coll.collider == objectCollider){
			BroadcastMessage("OnGroundImpact", this, SendMessageOptions.DontRequireReceiver);
		}
	}

	void OnCollisionStay2D(Collision2D coll){
//		if(coll.collider == objectCollider && coll.relativeVelocity.magnitude < 0.01 && currentMode != mode.ground){
		
		if(coll.collider == objectCollider && currentMode != mode.ground){
			GroundMode();
		}
	}

}
