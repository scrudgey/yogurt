﻿using UnityEngine;
public class ChemicalSpray: MonoBehaviour {
	public GameObject collisionPlume;
	private float height;
	void OnCollisionEnter2D(Collision2D coll){
		Instantiate(collisionPlume,transform.position,Quaternion.identity);
		Destroy(gameObject);
		Flammable flam = coll.gameObject.GetComponentInChildren<Flammable>();
		if (flam){
			if (flam.onFire){
				flam.onFire = false;
				flam.heat = -10f;

				OccurrenceFire fireData = new OccurrenceFire();
				fireData.objectName = Toolbox.Instance.CloneRemover(coll.gameObject.name);
				fireData.extinguished = true;
				Toolbox.Instance.OccurenceFlag(gameObject, fireData);
			}
		}
	}
}
