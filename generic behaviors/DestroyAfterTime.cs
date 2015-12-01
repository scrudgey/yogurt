using UnityEngine;
// using System.Collections;

public class DestroyAfterTime : MonoBehaviour {

	public float lifetime;
	void Start () {
		Destroy(gameObject, lifetime);
	}

}
