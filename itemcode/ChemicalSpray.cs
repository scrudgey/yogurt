using UnityEngine;
public class ChemicalSpray: MonoBehaviour {
	public GameObject collisionPlume;
	private float height;
	void OnCollisionEnter2D(Collision2D coll){
		Instantiate(collisionPlume, transform.position,Quaternion.identity);
		foreach(Flammable flam in coll.gameObject.GetComponentsInParent<Flammable>()){
			if (flam.onFire){
				flam.onFire = false;
				flam.heat = -10f;
				OccurrenceFire fireData = new OccurrenceFire();
				fireData.flamingObject = coll.gameObject;
				fireData.extinguished = true;
				Toolbox.Instance.OccurenceFlag(gameObject, fireData);
			}
		}
		Destroy(gameObject);
	}
}
