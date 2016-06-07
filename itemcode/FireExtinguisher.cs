using UnityEngine;
// using System.Collections;

public class FireExtinguisher : Interactive, IDirectable {

	public GameObject particle;
	public float emissionSpeed;
	public float emissionRate;
	private float emissionTimeout;
	public AudioClip spraySound;
	public Sprite spraySprite;
	private Sprite defaultSprite;
	private SpriteRenderer spriteRenderer;
	private Vector2 direction = Vector2.right;
	private bool doSpray;

	void Start () {
		Interaction spray = new Interaction(this,"Spray", "Spray", false, true);
		spray.defaultPriority = 1;
		spray.continuous = true;
		spray.dontWipeInterface = true;
		interactions.Add(spray);

		Interaction spray2 = new Interaction(this, "Spray", "SprayObject", true, false);
		spray2.continuous = true;
		spray2.limitless = true;
		spray2.dontWipeInterface = true;
		spray2.displayVerb = "Spray at";
		spray2.validationFunction = true;
		interactions.Add(spray2);

		if (!GetComponent<AudioSource>()){
			gameObject.AddComponent<AudioSource>();
		}
		GetComponent<AudioSource>().rolloffMode = AudioRolloffMode.Logarithmic;
		GetComponent<AudioSource>().minDistance = 0.4f;
		GetComponent<AudioSource>().maxDistance = 5.42f;
		GetComponent<AudioSource>().playOnAwake = false;

		spriteRenderer = GetComponent<SpriteRenderer>();
		defaultSprite = spriteRenderer.sprite;
	}
	
	public void Spray(){
		if (emissionTimeout <= 0f){
			doSpray = true;
		}
	}
	public string Spray_desc(){
		return "Spray "+name;
	}

	void FixedUpdate(){
		if (doSpray){
			if(!GetComponent<AudioSource>().isPlaying){
				GetComponent<AudioSource>().clip = spraySound;
				GetComponent<AudioSource>().Play();
			}
			spriteRenderer.sprite = spraySprite;
			GameObject p = Instantiate(particle, transform.position, Quaternion.identity) as GameObject;
			Vector2 force = direction * emissionSpeed;
			
			Collider2D projectileCollider = p.GetComponent<Collider2D>();
//			Collider2D[] parentColliders = transform.parent.GetComponents<Collider2D>();
			Collider2D[] parentColliders = gameObject.GetComponentsInParent<Collider2D>();
			Physics2D.IgnoreCollision(GetComponent<Collider2D>(), projectileCollider, true);
			foreach (Collider2D collider in parentColliders)
				Physics2D.IgnoreCollision(collider, projectileCollider, true);
			
			p.GetComponent<Rigidbody2D>().AddForce(force * Time.deltaTime, ForceMode2D.Impulse);
			emissionTimeout += emissionRate;
		}
		doSpray = false;
	}

	public void SprayObject(Item item){
		if (emissionTimeout <= 0f){
			direction = Vector3.ClampMagnitude(item.transform.position - transform.position, 1f);
			Spray();
		}
	}
	public string SprayObject_desc(Item item){
		return "Spray "+name+" at "+item.itemName;
	}

	public bool SprayObject_Validation(Item item){
		if (item.gameObject != gameObject){
			return true;
		} else {
			return false;
		}
	}

	void Update(){
		if (emissionTimeout > -0.5f)
			emissionTimeout -= Time.deltaTime;
		if (emissionTimeout < -0.5f && spriteRenderer.sprite == spraySprite){
			spriteRenderer.sprite = defaultSprite;
			GetComponent<AudioSource>().Stop();
		}
	}

	public void DirectionChange(Vector2 d){
		direction = d;
	}
}
