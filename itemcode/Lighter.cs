using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Lighter : Interactive {
	public ParticleSystem fire;
	private bool flameon;
	private Collider2D flameRadius;
//	private List<Flammable> flammables;
	private Dictionary<GameObject,Flammable> flammables = new Dictionary<GameObject,Flammable>();
	private bool LoadInitialized = false;

	void Start () {

//		flameon = false;
		if (!LoadInitialized)
			LoadInit();
	}

	public void LoadInit(){
		Interaction f = new Interaction(this,"Fire","Fire",false,true);
		f.defaultPriority = 1;
		interactions.Add(f);
		flameRadius = transform.FindChild("flameRadius").GetComponent<Collider2D>();
		flameRadius.enabled = false;

		LoadInitialized = true;
	}

	public void Fire(){
		flameon = !flameon;
		if (flameon){
			fire.Play();
			flameRadius.enabled = true;
		} else {
			fire.Stop();
			flameRadius.enabled = false;
		}
	}

	void OnTriggerEnter2D(Collider2D coll){
		if (!flammables.ContainsKey(coll.gameObject)){
			Flammable flammable = coll.GetComponent<Flammable>();
			if (flammable != null){
				flammables.Add(coll.gameObject,flammable);
			}
		}
	}	

	void OnTriggerStay2D(Collider2D coll){
		if (flammables.ContainsKey(coll.gameObject) && flameRadius.enabled ){
			flammables[coll.gameObject].heat += Time.deltaTime * 2f;
		}
	}

}