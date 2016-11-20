using UnityEngine;
using System.Collections.Generic;
// using System.Collections;

public class PhysicalImpact : MonoBehaviour {
	public Vector2 direction;
	public AudioClip[] impactSounds;
	public List<Transform> impactedObjects = new List<Transform>();
	public float magnitude = 20f;
	public float size = 0.08f;
	public List<GameObject> responsibleParty = new List<GameObject>();

	void Start(){
		Destroy(gameObject, 0.5f);
		Toolbox.Instance.SetUpAudioSource(gameObject);
		CircleCollider2D circle = GetComponent<CircleCollider2D>();
		circle.radius = size;
	}

	void OnTriggerEnter2D(Collider2D collider){
		if (collider.tag == "background" || collider.tag == "Puddle")
			return;
		if (impactedObjects.Contains(collider.transform.root))
			return;
		impactedObjects.Add(collider.transform.root);
		LiquidContainer container = collider.GetComponent<LiquidContainer>();
		if (container){
			container.Spill();
		}
		MessageDamage message = new MessageDamage(magnitude, damageType.physical);
		message.force = new Vector2(direction.x * magnitude / 100f, direction.y * magnitude / 100f);
		message.impactor = this;
		message.responsibleParty = responsibleParty;
		Toolbox.Instance.SendMessage(collider.gameObject, this, message);
	}

	public void PlayImpactSound(){
		AudioSource source = GetComponent<AudioSource>();
		// if (source.isPlaying)
		// 	return;
		if (impactSounds.Length > 0){
			source.PlayOneShot(impactSounds[Random.Range(0, impactSounds.Length)]);
		}
	}
}
