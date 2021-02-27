using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireworks : MonoBehaviour {



    public bool ignited;
    public Rigidbody2D body;
    public Intrinsics intrinsics;
    public Dictionary<BuffType, Buff> netBuffs = new Dictionary<BuffType, Buff>();
    float health;
    public AudioSource audioSource;
    public AudioClip[] whistleSounds;
    public AudioClip[] popSounds;
    public ParticleSystem fuseFx;
    private RotateTowardMotion rotator;

    public float timer;
    public float lifetime;
    public GameObject[] explosionFx;
    void Awake() {
        body = GetComponent<Rigidbody2D>();
        rotator = GetComponent<RotateTowardMotion>();
        Toolbox.RegisterMessageCallback<MessageDamage>(this, HandleMessageDamage);
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        lifetime = Random.Range(1.5f, 2f);

        Vector2 direction = Random.insideUnitCircle;
        float rot_z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rot_z);

    }
    void FixedUpdate() {
        if (ignited) {
            timer += Time.fixedDeltaTime;

            Vector3 force = 5000f * transform.right * Time.fixedDeltaTime;
            Vector3 pos = transform.position - (0.02f * transform.right);

            // body.AddForceAtPosition(55f * transform.right * Time.fixedDeltaTime, transform.forward * -0.05f, ForceMode2D.Impulse);
            body.AddForceAtPosition(force, pos, ForceMode2D.Force);
            body.gravityScale = 0;
            // explode after time
            if (timer > lifetime) {
                Explode();
            }
        }

    }
    void OnDestruct() {
        // explode when destroyed if ignited
        if (ignited)
            Explode();
    }
    void HandleMessageDamage(MessageDamage message) {
        if (intrinsics == null)
            intrinsics = Toolbox.GetOrCreateComponent<Intrinsics>(gameObject);
        netBuffs = intrinsics.NetBuffs();
        ImpactResult result = Damageable.Vulnerable(message, netBuffs);
        if (Damageable.DamageResults.Contains(result)) {
            TakeDamage(message);
        }
    }
    void TakeDamage(MessageDamage message) {
        health -= message.amount;
        if (health <= 0 || message.type == damageType.fire) {
            Ignite();
        }
    }

    public void Ignite() {
        // ignite
        //  sound: whistle
        //  velocity from behind when ignited
        //  fuse fx if ignited
        //  possible sparkler effect if ignited

        ignited = true;

        // play sound
        ListPlay(whistleSounds);

        // start fuse fx
        fuseFx.Play();

        PhysicalBootstrapper pb = GetComponent<PhysicalBootstrapper>();
        if (pb) {
            pb.DestroyPhysical();
            Destroy(pb);
        }

        gameObject.layer = LayerMask.NameToLayer("background");
    }
    static Color[] colors = new Color[]{
        Color.red ,
        Color.green,
        Color.blue
    };
    void Explode() {
        // explode when destroyed if ignited
        // explode after time
        // explode
        //  randomize color
        //  randomize pattern
        //  sound: pop

        // create particle effect

        // play pop sound
        Toolbox.Instance.AudioSpeaker(popSounds[Random.Range(0, popSounds.Length)], transform.position);

        Destroy(gameObject);

        GameObject fx = GameObject.Instantiate(explosionFx[Random.Range(0, explosionFx.Length)], transform.position, Quaternion.identity) as GameObject;
        ParticleSystem particles = fx.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule module = particles.main;
        module.startColor = colors[Random.Range(0, colors.Length)];
        // particles.main = module

        // apply damage
    }
    void ListPlay(AudioClip[] list) {
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(list[Random.Range(0, list.Length)]);
    }

    void OnCollisionEnter2D(Collision2D coll) {
        Rebound(coll);
    }
    void Rebound(Collision2D coll) {
        Vector2 normal = coll.contacts[0].normal;
        // Debug.Log($"{body.velocity} {1.1f * Vector3.Reflect(body.velocity, normal)}");
        body.velocity = 1.1f * Vector3.Reflect(body.velocity, normal);
        body.AddTorque((Random.Range(0, 2) * 2 - 1) * Random.Range(2f, 5f), ForceMode2D.Force);

        // if (wallImpactSounds.Length > 0)
        //     Toolbox.Instance.AudioSpeaker(wallImpactSounds[Random.Range(0, wallImpactSounds.Length)], transform.position);
    }
}
