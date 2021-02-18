using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DamageThreshhold {
    public enum ThreshholdType { total, impulse }
    public damageType type;
    public ThreshholdType threshholdType;
    public float threshholdAmount;
    public float impulseFactor;
    public float amount;
    public void TakeDamage(MessageDamage message) {
        if (message.type != type && type != damageType.any) {
            return;
        }
        amount += message.amount;
    }
    public void Update() {
        if (threshholdType == ThreshholdType.impulse) {
            if (amount > 0)
                amount -= impulseFactor * Time.deltaTime;
        }
    }
    public bool Explode() {
        return amount >= threshholdAmount;
    }
    public DamageThreshhold(damageType type, ThreshholdType threshholdType, float amount, float impulse) {
        this.type = type;
        this.threshholdType = threshholdType;
        this.threshholdAmount = amount;
        this.impulseFactor = impulse;
    }
}
public class Explosive : MonoBehaviour {
    public bool exploding = false;
    public List<DamageThreshhold> threshHolds = new List<DamageThreshhold>();
    public Dictionary<BuffType, Buff> netBuffs = new Dictionary<BuffType, Buff>();
    public Intrinsics intrinsics;
    public bool ignoreDamage;
    public List<AudioClip> explosionSounds = new List<AudioClip>();
    void Start() {
        Toolbox.RegisterMessageCallback<MessageDamage>(this, HandleMessageDamage);
        intrinsics = Toolbox.GetOrCreateComponent<Intrinsics>(gameObject);
    }
    void Update() {
        foreach (DamageThreshhold threshHold in threshHolds) {
            threshHold.Update();
        }
    }
    void HandleMessageDamage(MessageDamage message) {
        if (ignoreDamage)
            return;
        if (intrinsics == null)
            intrinsics = Toolbox.GetOrCreateComponent<Intrinsics>(gameObject);
        netBuffs = intrinsics.NetBuffs();
        ImpactResult result = Damageable.Vulnerable(message, netBuffs);
        if (Damageable.DamageResults.Contains(result)) {
            foreach (DamageThreshhold threshHold in threshHolds) {
                threshHold.TakeDamage(message);
                if (threshHold.Explode())
                    Explode();
            }
        }
    }
    public virtual void HandleNetIntrinsic(MessageNetIntrinsic message) {
        // Debug.Log("netbuffs changed");
        netBuffs = message.netBuffs;
    }
    public void Explode() {
        if (exploding)
            return;
        exploding = true;
        GameObject explosion = GameObject.Instantiate(Resources.Load("PhysicalImpact"), transform.position, Quaternion.identity) as GameObject;
        PhysicalImpact impact = explosion.GetComponent<PhysicalImpact>();
        MessageDamage message = new MessageDamage(500f, damageType.explosion);
        message.responsibleParty = gameObject;
        impact.size = 0.70f;
        impact.message = message;

        if (explosionSounds.Count > 0) {
            AudioClip explosionSound = explosionSounds[Random.Range(0, explosionSounds.Count)];
            Toolbox.Instance.AudioSpeaker(explosionSound, transform.position);
        }

        GameObject fx = GameObject.Instantiate(Resources.Load("particles/explosion")) as GameObject;
        fx.transform.position = transform.position;
        fx.transform.rotation = Quaternion.AngleAxis(-120.9f, new Vector3(1, 0, 0));

        CameraControl cam = GameObject.FindObjectOfType<CameraControl>();
        float distanceToCamera = Vector2.Distance(transform.position, cam.transform.position) * 10;
        // float amount = Mathf.Min(0.25f, 0.25f / (Mathf.Pow(distanceToCamera, 2)));
        float amount = Mathf.Min(0.25f, 0.25f / distanceToCamera);
        // Debug.Log(amount);
        cam.Shake(amount);

        Toolbox.Instance.OccurenceFlag(gameObject, EventData.Explosion(gameObject));

        // Debug.Break();
        MessageDamage selfDestruct = new MessageDamage(200f, damageType.explosion);

        foreach (Gibs gib in GetComponents<Gibs>()) {
            selfDestruct.force = Random.insideUnitCircle * 100f;
            selfDestruct.angleAboveHorizontal = 1.52f;
            gib.Emit(selfDestruct);
        }

        PhysicalBootstrapper phys = GetComponent<PhysicalBootstrapper>();
        if (phys) {
            phys.DestroyPhysical();
        }

        ClaimsManager.Instance.WasDestroyed(gameObject);
        Destroy(gameObject);
    }
}
