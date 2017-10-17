using UnityEngine;
using System.Collections.Generic;

public class PhysicalImpact : MonoBehaviour {
	public AudioClip[] impactSounds;
	public AudioClip[] repelSounds;
	public AudioClip[] strongImpactSounds;
	public List<Transform> impactedObjects = new List<Transform>();
	public float size = 0.08f;
	public MessageDamage message;
	void Start(){
		Destroy(gameObject, 0.5f);
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
		violence.disturbing = 10f;
		violence.chaos = 10f;
		Toolbox.Instance.OccurenceFlag(gameObject, violence);
	}
	public void PlayImpactSound(ImpactResult impactType){
		if (message.impactSounds != null){
			impactSounds = message.impactSounds;
		}
		// AudioSource source = GetComponent<AudioSource>();
		switch(impactType){
			case ImpactResult.normal:
				if (impactSounds.Length > 0){
					Toolbox.Instance.AudioSpeaker(impactSounds[Random.Range(0, impactSounds.Length)], transform.position);
				}
			break;
			case ImpactResult.repel:
				if (repelSounds.Length > 0){
					Toolbox.Instance.AudioSpeaker(repelSounds[Random.Range(0, repelSounds.Length)], transform.position);
				}
			break;
			case ImpactResult.strong:
				if (strongImpactSounds.Length > 0){
					Toolbox.Instance.AudioSpeaker(strongImpactSounds[Random.Range(0, strongImpactSounds.Length)], transform.position);
				}
				Instantiate(Resources.Load("particles/explosion1"), transform.position, Quaternion.identity);
			break;
			default:
			break;
		}
	}
}
