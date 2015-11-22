using UnityEngine;
// using System.Collections;

//[RequireComponent (typeof (Rigidbody2D))]

public class TerminateOnStop : MonoBehaviour {

	// Use this for initialization
//	void Start () {
//	
//	}
	
	// Update is called once per frame
	void Update () {

		if (GetComponent<Rigidbody2D>().velocity.magnitude < 0.01){

			if (GetComponent<Rigidbody2D>())
				Destroy(GetComponent<Rigidbody2D>());
			if (GetComponent<Collider2D>())
				Destroy (GetComponent<Collider2D>());
			Destroy(this);
		}
	}
}
