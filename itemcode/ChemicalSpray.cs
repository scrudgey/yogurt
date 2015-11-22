using UnityEngine;
// using System.Collections;

public class ChemicalSpray: MonoBehaviour {

	public GameObject collisionPlume;
	private float height;
//	private float vy = 0;

	void OnCollisionEnter2D(Collision2D coll){
		Instantiate(collisionPlume,transform.position,Quaternion.identity);
		Destroy(gameObject);
		Flammable flam = coll.gameObject.GetComponentInChildren<Flammable>();
		if (flam){
			if (flam.onFire){
				flam.onFire = false;
	//			flam.heat -= 5* Time.deltaTime;
				flam.heat = -200f;
			}
		}
	}

//	void Update(){
//		vy += Time.deltaTime * 0.1f;
//		height -= vy;
//		Vector3 temppos = transform.position;
//		temppos.y -= vy * Time.deltaTime;
//		transform.position = temppos;
//		if (height < 1f)
//			Destroy(gameObject);
//	}

}
