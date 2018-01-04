using UnityEngine;
using System.Collections.Generic;

public class PhysicalImpact : MonoBehaviour {
	public List<Transform> impactedObjects = new List<Transform>();
	public float size = 0.08f;
	public MessageDamage message;
	void Start(){
		Destroy(gameObject, 0.05f);
		CircleCollider2D circle = GetComponent<CircleCollider2D>();
		circle.radius = size;
	}
	void OnTriggerEnter2D(Collider2D collider){
		if (collider.isTrigger)
			return;
		if (collider.tag == "background" || collider.tag == "Puddle" || collider.tag == "fire")
			return;
		if (impactedObjects.Contains(collider.transform.root))
			return;
		impactedObjects.Add(collider.transform.root);
		message.impactor = this;
		Toolbox.Instance.SendMessage(collider.gameObject, this, message);
		OccurrenceViolence violence = new OccurrenceViolence();
		violence.attacker = message.responsibleParty;
		violence.victim = collider.gameObject;
		Toolbox.Instance.OccurenceFlag(gameObject, violence);
	}
}
