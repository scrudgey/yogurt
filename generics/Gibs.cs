using UnityEngine;

public class Gibs : MonoBehaviour {
	public damageType damageCondition;
	public int number;
	public GameObject particle;
	public float forceMin;
	public float forceMax;
	public Color color = Color.white;
	public void Emit(damageType dam){
		if (!DamageTypeMatch(damageCondition, dam))
			return;
		for (int i =0; i<number; i++){
			GameObject bit = Instantiate(particle) as GameObject;
			Vector3 randomWalk = 0.1f * Random.insideUnitCircle.normalized;
			randomWalk.z = 0;
			bit.transform.position = transform.position + randomWalk;
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
				Toolbox.Instance.DisableAndReenable(damageable, 0.2f);
			}
		}
	}
	public static bool DamageTypeMatch(damageType dam1, damageType dam2){
		if (dam1 == dam2)
			return true;
		if (dam1 == damageType.any || dam2 == damageType.any)
			return true;
		if (dam1 == damageType.physical && (dam2 == damageType.cutting || dam2 == damageType.physical || dam2 == damageType.piercing))
			return true;
		if (dam2 == damageType.physical && (dam1 == damageType.cutting || dam1 == damageType.physical || dam1 == damageType.piercing))
			return true;
		return false;
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
