using UnityEngine;
using System.Collections;

public class Gibs : MonoBehaviour {
	public damageType damageCondition;
	public int number;
	public GameObject particle;
	public float forceMin;
	public float forceMax;
	public Color color = Color.white;
	public void Emit(damageType dam){
		// if (dam = damageCondition)
		for (int i =0; i<number; i++){
			GameObject bit = Instantiate(particle) as GameObject;
			bit.transform.position = transform.position;
			Rigidbody2D bitPhys = bit.GetComponent<Rigidbody2D>();
			if (bitPhys){
				Vector2 force = Random.insideUnitCircle;
				force = force * (forceMax * Random.value + forceMin );
				bitPhys.AddForce(force);
			}
			SpriteRenderer sprite = bit.GetComponent<SpriteRenderer>();
			sprite.color = color;
			Damageable damageable = bit.GetComponent<Damageable>();
			if (damageable){
				// StartCoroutine(damageable.EnableAfterSeconds(2f));
				Toolbox.Instance.DisableAndReenable(damageable, 0.1f);
			}
		}
	}
	public void CopyFrom(Gibs other){
		damageCondition = other.damageCondition;
		number = other.number;
		particle = other.particle;
		forceMin = other.forceMin;
		forceMax = other.forceMax;
		color = other.color;
	}
}
