using UnityEngine;
using System.Collections.Generic;
// using System.Collections;

public class PhysicalImpact : MonoBehaviour {
	public Vector2 direction;
	public AudioClip[] impactSounds;
	public List<GameObject> impactedObjects = new List<GameObject>();
	public float magnitude = 20f;
	public float size = 0.08f;

	void Start(){
		Destroy(gameObject, 0.5f);
		Toolbox.Instance.SetUpAudioSource(gameObject);
		CircleCollider2D circle = GetComponent<CircleCollider2D>();
		circle.radius = size;
	}

	void OnTriggerEnter2D(Collider2D collider){
		if (impactedObjects.Contains(collider.gameObject))
			return;
		impactedObjects.Add(collider.gameObject);
		LiquidContainer container = collider.GetComponent<LiquidContainer>();
		if (container){
			container.Spill();
		}

		float damage = direction.magnitude * 20f;
		MessageDamage message = new MessageDamage(damage, damageType.physical);
		message.force = new Vector2(direction.x / 5f, direction.y / 5f);
		message.impactor = this;
		Toolbox.Instance.SendMessage(collider.gameObject, this, message);
	}

	public void PlayImpactSound(){
		AudioSource source = GetComponent<AudioSource>();
		if (source.isPlaying)
			return;
		if (impactSounds.Length > 0){
			source.PlayOneShot(impactSounds[Random.Range(0, impactSounds.Length)]);
		}
	}
}
