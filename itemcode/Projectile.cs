using UnityEngine;

public class Projectile : MonoBehaviour {
    public damageType damageType;
    public float damage;
    public MessageDamage message;
    public AudioClip[] wallImpactSounds;
    public AudioClip[] hurtableImpactSounds;
    private Rigidbody2D body;
    private RotateTowardMotion rotator;
    public bool destroyOnImpact;
    public GameObject destroyEffect;
    public Liquid liquid;
    public GameObject responsibleParty;
    void Awake() {
        message = new MessageDamage(damage, damageType);
        message.impactSounds = hurtableImpactSounds;
        message.responsibleParty = gameObject;
        body = GetComponent<Rigidbody2D>();
        rotator = GetComponent<RotateTowardMotion>();
    }
    void OnTriggerEnter2D(Collider2D coll) {
        if (coll.isTrigger)
            return;
        Damageable hurtable = coll.gameObject.GetComponentInParent<Damageable>();
        // Debug.Log($"collision {coll} {hurtable}");
        if (responsibleParty != null) {
            message.responsibleParty = responsibleParty;
        }
        if (hurtable) {
            Toolbox.Instance.SendMessage(coll.gameObject, this, message);
            Toolbox.Instance.AudioSpeaker(hurtableImpactSounds[Random.Range(0, hurtableImpactSounds.Length)], transform.position);
        } else {
            Toolbox.Instance.AudioSpeaker(wallImpactSounds[Random.Range(0, wallImpactSounds.Length)], transform.position);
        }
        OnPhysicalImpact();
    }
    void OnCollisionEnter2D(Collision2D coll) {
        Damageable hurtable = coll.gameObject.GetComponent<Damageable>();
        if (responsibleParty != null) {
            message.responsibleParty = responsibleParty;
        }
        if (hurtable) {
            Toolbox.Instance.SendMessage(coll.gameObject, this, message);
            ClaimsManager.Instance.WasDestroyed(gameObject);
            Dispose();
            if (destroyEffect != null) {
                GameObject.Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }

        } else {
            if (!destroyOnImpact) {
                Rebound(coll);
            }
        }
        if (liquid.name != "") {
            Eater eater = coll.gameObject.GetComponent<Eater>();
            if (eater != null) {
                GameObject sip = Instantiate(Resources.Load("prefabs/droplet"), transform.position, Quaternion.identity) as GameObject;
                Liquid.MonoLiquidify(sip, liquid);
                eater.Eat(sip.GetComponent<Edible>());
            }
        }
        OnPhysicalImpact();
    }
    public void OnPhysicalImpact() {
        if (destroyOnImpact) {
            ClaimsManager.Instance.WasDestroyed(gameObject);
            Destroy(gameObject);
            if (destroyEffect != null) {
                GameObject.Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }
        }
    }
    void Dispose() {
        // gameObject.SetActive(false);
        // foreach (Collider2D collider in gameObject.GetComponents<Collider2D>()) {
        //     Destroy(collider);
        // }
        Destroy(gameObject, 5);
    }

    void Rebound(Collision2D coll) {
        Vector2 normal = coll.contacts[0].normal;

        if (Vector2.Angle(body.velocity, normal) < 45f) {
            if (rotator)
                rotator.enabled = false;
            if (body) {
                body.AddTorque((Random.Range(0, 2) * 2 - 1) * Random.Range(2f, 4f), ForceMode2D.Force);
                if (body.velocity.magnitude > 2f)
                    body.velocity = Vector2.ClampMagnitude(body.velocity, 2f);
            }
        }
        if (coll.relativeVelocity.magnitude > 0.4f) {
            if (wallImpactSounds.Length > 0)
                Toolbox.Instance.AudioSpeaker(wallImpactSounds[Random.Range(0, wallImpactSounds.Length)], transform.position);
        }
    }
}
