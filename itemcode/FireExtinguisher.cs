using UnityEngine;
using System.Collections.Generic;
public class FireExtinguisher : Interactive, IDirectable {
    public GameObject particle;
    public float emissionSpeed;
    public float emissionRate;
    private float emissionTimeout;
    public List<AudioClip> spraySound;
    public Sprite spraySprite;
    private Sprite defaultSprite;
    private SpriteRenderer spriteRenderer;
    private Vector2 direction = Vector2.right;
    private AudioSource audioSource;
    private bool doSpray;
    void Awake() {
        Interaction spray = new Interaction(this, "Spray", "Spray");
        spray.defaultPriority = 2;
        spray.continuous = true;
        spray.dontWipeInterface = true;
        spray.otherOnSelfConsent = false;
        spray.selfOnOtherConsent = false;
        interactions.Add(spray);

        Interaction spray2 = new Interaction(this, "Spray", "SprayObject");
        spray2.continuous = true;
        spray2.selfOnSelfConsent = false;
        spray2.otherOnSelfConsent = false;

        spray2.unlimitedRange = true;
        spray2.dontWipeInterface = true;
        spray2.validationFunction = true;
        interactions.Add(spray2);

        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);

        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultSprite = spriteRenderer.sprite;
    }
    public void Spray() {
        if (emissionTimeout <= 0f) {
            doSpray = true;
        }
    }
    public string Spray_desc() {
        string myname = Toolbox.Instance.GetName(gameObject);
        return "Spray " + myname;
    }
    void FixedUpdate() {
        if (doSpray) {
            if (!audioSource.isPlaying) {
                audioSource.clip = spraySound[Random.Range(0, spraySound.Count)];
                audioSource.Play();
            }
            if (spraySprite != null)
                spriteRenderer.sprite = spraySprite;
            GameObject particles = Instantiate(particle, transform.position, Quaternion.identity) as GameObject;
            Vector2 force = direction * emissionSpeed;
            Collider2D projectileCollider = particles.GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), projectileCollider, true);
            // ignore collisions with parent object: so the player doesn't spray herself with fire extinguisher
            if (transform.parent != null) {
                Collider2D[] parentColliders = transform.root.GetComponentsInChildren<Collider2D>();
                foreach (Collider2D collider in parentColliders) {
                    Physics2D.IgnoreCollision(collider, projectileCollider, true);
                }
            }
            particles.GetComponent<Rigidbody2D>().AddForce(force * Time.fixedDeltaTime, ForceMode2D.Impulse);
            ChemicalSpray spray = particles.GetComponent<ChemicalSpray>();
            if (spray) {
                Pickup myPickup = GetComponent<Pickup>();
                if (myPickup) {
                    if (myPickup.holder)
                        spray.responsibleParty = myPickup.holder.gameObject;
                }
            }
            emissionTimeout += emissionRate;
        }
        doSpray = false;
    }
    public void SprayObject(GameObject item) {
        if (emissionTimeout <= 0f) {
            direction = (Vector2)(item.transform.position - transform.position).normalized;
            Spray();
        }
    }
    public string SprayObject_desc(GameObject item) {
        string myname = Toolbox.Instance.GetName(gameObject);
        string itemname = Toolbox.Instance.GetName(item.gameObject);
        return "Spray " + myname + " at " + itemname;
    }
    public bool SprayObject_Validation(GameObject item) {
        if (item != gameObject) {
            return true;
        } else {
            return false;
        }
    }
    void Update() {
        if (emissionTimeout > -0.5f)
            emissionTimeout -= Time.deltaTime;
        if (emissionTimeout < -0.5f && spriteRenderer.sprite == spraySprite) {
            spriteRenderer.sprite = defaultSprite;
            GetComponent<AudioSource>().Stop();
        }
    }
    public void DirectionChange(Vector2 d) {
        direction = d;
    }
}
