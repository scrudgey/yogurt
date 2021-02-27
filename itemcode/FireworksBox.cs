using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireworksBox : MonoBehaviour {
    public AudioClip[] chaosSounds;
    public AudioSource audioSource;
    public Intrinsics intrinsics;
    public Flammable flammable;
    public Rigidbody2D body;
    public PhysicalBootstrapper pb;
    public Dictionary<BuffType, Buff> netBuffs = new Dictionary<BuffType, Buff>();
    float health;
    public bool ignited;
    public float emitCountdown;
    void Start() {
        flammable = GetComponent<Flammable>();
        body = GetComponent<Rigidbody2D>();
        pb = GetComponent<PhysicalBootstrapper>();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        Toolbox.RegisterMessageCallback<MessageDamage>(this, HandleMessageDamage);
        health = 50f;
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
    void Ignite() {
        ignited = true;

        flammable.SpontaneouslyCombust();
    }
    void FixedUpdate() {
        if (ignited) {
            emitCountdown -= Time.fixedDeltaTime;
            if (!audioSource.isPlaying) {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(chaosSounds[Random.Range(0, chaosSounds.Length)]);
            }
            if (emitCountdown <= 0) {
                emitCountdown = Random.Range(1f, 2f);
                Emit();
            }
        }
    }
    void Emit() {
        foreach (Gibs gib in GetComponents<Gibs>()) {
            if (gib.impactEmitExpectedPer100 > 0) {
                MessageDamage message = new MessageDamage(10f, damageType.fire) {
                    force = Random.insideUnitCircle
                };
                foreach (GameObject rocket in gib.Emit(message)) {
                    Fireworks fireworks = rocket.GetComponent<Fireworks>();
                    if (fireworks != null) {
                        fireworks.Ignite();
                    }
                }
            }
        }
        // body.AddForce(transform.up * 1000f, ForceMode2D.Force);

        Vector3 force = Random.insideUnitCircle * Random.Range(1f, 2f);
        float phi = Random.Range(0, 3.14f);
        force.z = force.magnitude * Mathf.Sin(phi);
        force.x = force.x * Mathf.Cos(phi);
        force.y = force.y * Mathf.Cos(phi);
        pb.physical.StartFlyMode();
        pb.Set3Motion(force);
        body.AddTorque(Random.Range(-10f, 10f));
    }

    // emit random fireworks
    // box jumps with each one
    // set on fire
}
