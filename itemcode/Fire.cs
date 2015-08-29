using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Fire : MonoBehaviour {

	public Flammable flammable;
	private Dictionary<GameObject,Flammable> flammables = new Dictionary<GameObject,Flammable>();



	void OnTriggerEnter2D(Collider2D coll){
		
		if (!flammables.ContainsKey(coll.gameObject) ){
			Flammable flam = coll.GetComponentInParent<Flammable>();
			if (flam != null && flam != flammable){
				flammables.Add(coll.gameObject,flam);
			}
		}
		
	}	
	
	void OnTriggerStay2D(Collider2D coll){

		if (flammables.ContainsKey(coll.gameObject) && flammable.onFire){
			flammables[coll.gameObject].heat += Time.deltaTime * 2f;
		}
	}
}
