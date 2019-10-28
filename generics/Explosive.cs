﻿using System.Collections;
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
}
public class Explosive : MonoBehaviour {
    public List<DamageThreshhold> threshHolds = new List<DamageThreshhold>();
    void Start() {
        Toolbox.RegisterMessageCallback<MessageDamage>(this, HandleMessageDamage);
    }
    void Update() {
        foreach (DamageThreshhold threshHold in threshHolds) {
            threshHold.Update();
        }
    }
    void HandleMessageDamage(MessageDamage message) {
        foreach (DamageThreshhold threshHold in threshHolds) {
            threshHold.TakeDamage(message);
            if (threshHold.Explode())
                Explode();
        }
    }
    public void Explode() {

        GameObject explosion = GameObject.Instantiate(Resources.Load("PhysicalImpact"), transform.position, Quaternion.identity) as GameObject;
        PhysicalImpact impact = explosion.GetComponent<PhysicalImpact>();
        MessageDamage message = new MessageDamage(500f, damageType.explosion);
        message.responsibleParty = gameObject;
        impact.size = 0.70f;
        impact.message = message;

        Toolbox.Instance.AudioSpeaker("explosion/cannon", transform.position);

        GameObject fx = GameObject.Instantiate(Resources.Load("particles/explosion")) as GameObject;
        fx.transform.position = transform.position;
        fx.transform.rotation = Quaternion.AngleAxis(-120.9f, new Vector3(1, 0, 0));

        CameraControl cam = GameObject.FindObjectOfType<CameraControl>();
        float distanceToCamera = Vector2.Distance(transform.position, cam.transform.position) * 10;
        // float amount = Mathf.Min(0.25f, 0.25f / (Mathf.Pow(distanceToCamera, 2)));
        float amount = Mathf.Min(0.25f, 0.25f / distanceToCamera);
        // Debug.Log(amount);
        cam.Shake(amount);



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
        DestroyImmediate(gameObject);
    }
}
