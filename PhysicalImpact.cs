using UnityEngine;
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
        MessageDamage messageToSend = new MessageDamage(message);
        if (messageToSend.force == Vector2.zero) {
            // this might have unintended side effects
            messageToSend.force = collider.transform.position - transform.position;
            messageToSend.force = messageToSend.force.normalized;
            float distance = Vector2.Distance(transform.position, collider.transform.position);
            messageToSend.force *= (1f / distance) * 10f;
            messageToSend.angleAboveHorizontal = Mathf.Atan(0.2f / Vector2.Distance(transform.position, collider.transform.position));
        }
        messageToSend.impactor = this;
        Toolbox.Instance.SendMessage(victim, this, messageToSend);
        OccurrenceViolence violence = new OccurrenceViolence();
        violence.amount = messageToSend.amount;
        violence.attacker = messageToSend.responsibleParty;
        violence.victim = victim;
        violence.type = messageToSend.type;
        if (messageToSend.responsibleParty != null) {
            Toolbox.Instance.OccurenceFlag(messageToSend.responsibleParty, violence);
        } else {
            Toolbox.Instance.OccurenceFlag(gameObject, violence);
        }
    }
}
