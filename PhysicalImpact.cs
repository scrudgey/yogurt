﻿using UnityEngine;
using System.Collections.Generic;

public class PhysicalImpact : MonoBehaviour {
    public List<Transform> impactedObjects = new List<Transform>();
    public float size = 0.08f;
    public MessageDamage message;
    public CircleCollider2D circle;
    public BoxCollider2D box;
    public bool useBoxCollider;
    void Start() {
        Destroy(gameObject, 0.05f);
        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        circle.radius = size;
        if (useBoxCollider) {
            circle.enabled = false;
            box.enabled = true;
        } else {
            circle.enabled = true;
            box.enabled = false;
        }
    }
    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.isTrigger)
            return;
        if (collider.tag == "background" || collider.tag == "Puddle" || collider.tag == "fire")
            return;
        if (impactedObjects.Contains(collider.transform.root))
            return;
        GameObject victim = collider.gameObject;
        if (collider.name == "maincollider") {
            victim = collider.transform.parent.gameObject;
        }
        impactedObjects.Add(collider.transform.root);
        message.impactor = this;
        Toolbox.Instance.SendMessage(victim, this, message);
        OccurrenceViolence violence = new OccurrenceViolence();
        violence.amount = message.amount;
        violence.attacker = message.responsibleParty;
        violence.victim = victim;
        violence.type = message.type;
        HashSet<GameObject> involvedParties = new HashSet<GameObject>() { message.responsibleParty, victim };
        Toolbox.Instance.OccurenceFlag(message.responsibleParty, violence, involvedParties);
    }
}
