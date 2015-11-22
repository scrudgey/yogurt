using UnityEngine;
// using System.Collections;

public class Stimulus : MonoBehaviour {

	public float size;
	private CircleCollider2D circle;

	// Use this for initialization
	void Start () {
		circle = gameObject.GetComponent<CircleCollider2D>();
	}
	
	// Update is called once per frame
	void Update () {
		if (circle.radius < size){
			circle.radius += Time.deltaTime;
		} else {
			Destroy(gameObject);
		}
	}
}
