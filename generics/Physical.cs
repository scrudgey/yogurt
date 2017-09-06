using UnityEngine;
using System.Collections.Generic;
public class Physical : MonoBehaviour, IMessagable {
	public enum mode{none, fly, ground, zip}
	public AudioClip[] impactSounds;
	public AudioClip[] landSounds;
	public AudioSource audioSource;
	public PhysicalBootstrapper bootstrapper;
	private GameObject trueObject;
	public Rigidbody2D objectBody;
	public Collider2D objectCollider;
	public Rigidbody2D hingeBody;
	public GameObject hinge;
	public SliderJoint2D slider;
	public EdgeCollider2D horizonCollider;
	public mode currentMode;
	public float height;
	public bool doFly;
	private bool doGround;
	private bool doZip;
	// private bool doFixSlider;
	private bool doStartTable;
	private bool doStopTable;
	private SpriteRenderer spriteRenderer;
	public float groundDrag;
	private float ziptime;
	public bool suppressLandSound;
	private Table table;
	public List<Collider2D> temporaryDisabledColliders = new List<Collider2D>();

	void Awake(){
		InitValues();
	}
	void Start() {
		// Debug.Log(name + " physical start");
		// ignore collisions between ground and all other objects
		foreach(Physical phys in GameObject.FindObjectsOfType<Physical>()){
			if (phys == this)
				continue;
			Physics2D.IgnoreCollision(horizonCollider, phys.GetComponent<Collider2D>(), true);
			foreach (Collider2D col in phys.GetComponentsInChildren<Collider2D>()){
				if (col.tag == "fire")
					continue;
				Physics2D.IgnoreCollision(objectCollider, col, true);
			}
		}
		Physics2D.IgnoreCollision(horizonCollider, objectCollider, false);
		if (currentMode == mode.none)
			FlyMode();
	}
	public void InitValues(){
		slider = GetComponent<SliderJoint2D>();
		hinge = transform.Find("hinge").gameObject;
		trueObject = hinge.transform.GetChild(0).gameObject;
		objectCollider = trueObject.GetComponent<Collider2D>();
		horizonCollider = transform.Find("horizon").GetComponent<EdgeCollider2D>();
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
		spriteRenderer = GetComponent<SpriteRenderer>();
	}
	public void Impact(Vector2 f){
		Vector2 force = f / (objectBody.mass / 25f);
		if (currentMode != mode.fly)
			FlyMode();
		if (impactSounds.Length > 0)
			audioSource.PlayOneShot(impactSounds[Random.Range(0, impactSounds.Length)]);
		bootstrapper.Set3Motion(new Vector3(force.x, force.y, force.y + 0.5f));
	}
	void FixedUpdate() {
		if (trueObject == null){
			Destroy(this);
			return;
		}
		// if (doFixSlider)
		// 	FixSliderLimits();
		height = horizonCollider.offset.y + hinge.transform.localPosition.y;
		if (currentMode == mode.fly){
			if (height < 0){
				Vector2 hingePosition = hinge.transform.localPosition;
				// hingePosition.y = 0.1f + groundCollider.size.y / 2f - groundCollider.offset.y;
				hingePosition.y = 0.1f - horizonCollider.offset.y;
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
				doFly = true;
			}
		}
		if (currentMode == mode.fly || currentMode == mode.zip){
			// projectiles should be on their own layer: if i placed them on main, they would 
			// not collide with their footprint. if i placed them on feet, they would not collide with 
			// extended objects.
			trueObject.layer = 11;
		} else {
			trueObject.layer = 10;
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
		if (doStartTable){
			doStartTable = false;
			StartTable();
		}
		if (doStopTable){
			doStopTable = false;
			StopTable();
		}
		if (currentMode == mode.ground){
			if (objectCollider.Distance(horizonCollider).isOverlapped){
				slider.useLimits = false;
				// Debug.Log("horizon overlap, undo limits "+name);
			} else {
				if (slider.useLimits == false){
					// Debug.Log("non overlap, reseting limit "+name);
					SetSliderLimit(0);
				}
			}
		}
	}
	public void GroundMode (){
		// Debug.Log(name + " groundmode");
		doGround = true;
	}
	public void FlyMode(){
		// Debug.Log(name + " flymode");
		doFly = true;
	}
	public void ZipMode(){
		// Debug.Log(name + " zipmode");
		doZip = true;
	}
	public void SetSliderLimit(float offset){
		JointTranslationLimits2D tempLimits = slider.limits;
		tempLimits.min = 0;
		tempLimits.max = hinge.transform.localPosition.y + offset;
		// tempLimits.max = hinge.transform.localPosition.y + 0.005f;
		slider.limits = tempLimits;
		slider.useLimits = true;
	}
	public void StartGroundMode(){
		// Debug.Log(name + " start ground mode");
		doGround = false;
		doFly = false;
		doZip = false;
		ziptime = 0f;
		ClearTempColliders();
		GetComponent<Rigidbody2D>().drag = groundDrag;
		GetComponent<Rigidbody2D>().mass = objectBody.mass;
		currentMode = mode.ground;
		foreach(Physical phys in FindObjectsOfType<Physical>()){
			if (phys == this)
				continue;
			if (phys.currentMode == mode.ground){
				Physics2D.IgnoreCollision(objectCollider, phys.horizonCollider, true);
				Physics2D.IgnoreCollision(horizonCollider, phys.objectCollider, true);
			}
		}
		if (landSounds.Length > 0 && !suppressLandSound)
			audioSource.PlayOneShot(landSounds[Random.Range(0, landSounds.Length)]);
		suppressLandSound = false;
		SetSliderLimit(1f* objectCollider.Distance(horizonCollider).distance);
	}
	public void ClearTempColliders(){
		foreach (Collider2D temporaryCollider in temporaryDisabledColliders){
			Physics2D.IgnoreCollision(temporaryCollider, objectCollider, false);
			Physics2D.IgnoreCollision(temporaryCollider, horizonCollider, false);
		}
		temporaryDisabledColliders = new List<Collider2D>();
	}
	public void StartFlyMode(){
		// Debug.Log(name + " start fly mode");
		doGround = false;
		doFly = false;
		doZip = false;
		ziptime = 0f;
		ClearTempColliders();
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
				Physics2D.IgnoreCollision(horizonCollider, phys.objectCollider, true);
				Physics2D.IgnoreCollision(objectCollider, phys.horizonCollider, true);
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
				audioSource.PlayOneShot(impactSounds[Random.Range(0, impactSounds.Length)]);
			}
		}
	}
	void OnTriggerEnter2D(Collider2D coll){
		if (coll.tag == "table" && coll.gameObject != gameObject){
			// Debug.Log(name + " table collision detected, dostarttable true");
			table = coll.GetComponentInParent<Table>();
			doStartTable = true;
		}
	}
	void OnTriggerExit2D(Collider2D coll){
		if (coll.tag == "table" && coll.gameObject != gameObject){
			table = coll.GetComponentInParent<Table>();
			doStopTable = true;
		}
	}
	void StartTable(){
		// Debug.Log(name + " start table");
		Vector2 newOffset = new Vector2(0f, table.height);
		horizonCollider.offset = newOffset;
		Vector3 objectPosition = hinge.transform.localPosition;
		if (objectPosition.y > table.height){
			// objectPosition.y -= table.height;
		} else {
			objectPosition.y += table.height + 0.02f;
		}
		hinge.transform.localPosition = objectPosition;
		JointTranslationLimits2D tempLimits = slider.limits;
		tempLimits.min = 0;
		tempLimits.max = hinge.transform.localPosition.y;
		slider.limits = tempLimits;

		transform.SetParent(table.transform);
		if (spriteRenderer)
			spriteRenderer.enabled = false;
	}
	void StopTable(){
		Vector3 objectPosition = hinge.transform.localPosition;

		Vector2 newOffset = new Vector2(0f, 0.0f);
		horizonCollider.offset = newOffset;
		hinge.transform.localPosition = objectPosition;

		JointTranslationLimits2D tempLimits = slider.limits;
		tempLimits.min = 0;
		tempLimits.max = hinge.transform.localPosition.y;
		slider.limits = tempLimits;
		transform.SetParent(null);
		if (spriteRenderer)
			spriteRenderer.enabled = true;
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
