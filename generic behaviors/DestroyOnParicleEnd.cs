using UnityEngine;
// using System.Collections;

public class DestroyOnParicleEnd : MonoBehaviour {

	
	// Update is called once per frame
	void Update () {
		if (!GetComponent<ParticleSystem>().IsAlive()){
			Destroy(gameObject);
		}
	}
}
