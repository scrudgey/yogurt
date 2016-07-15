using UnityEngine;
using System.Collections.Generic;
// using System.Collections;

public class Slasher : MonoBehaviour {
	public AudioClip[] impactSounds;
	public Vector2 direction; 
	public List<GameObject> owners = new List<GameObject>();

	public float damage;
	
	void SlashOn(){
		Vector3 startPoint = transform.position;
		startPoint.x += direction.x / 4f;
		startPoint.y += direction.y / 4f;

		GameObject impactObj = GameManager.Instantiate(Resources.Load("PhysicalImpact"), startPoint, Quaternion.identity) as GameObject;
		PhysicalImpact impact = impactObj.GetComponent<PhysicalImpact>();
		impact.impactSounds = impactSounds;
		impact.direction = direction;
		impact.size = 0.11f;
		impact.magnitude = damage;

		CircleCollider2D impactCollider = impact.GetComponent<CircleCollider2D>();
		foreach (GameObject owner in owners){
			Collider2D[] ownerColliders = owner.GetComponentsInChildren<Collider2D>();
			foreach (Collider2D ownerCollider in ownerColliders){
				Physics2D.IgnoreCollision(impactCollider, ownerCollider, true);
			}
		}
		
	}
	void SlashOff(){
		// GetComponent<Collider2D>().enabled = false;
	}
	void SlashEnd(){
		Destroy(gameObject);
	}
}
