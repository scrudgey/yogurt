// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

public class FallingLeaf : MonoBehaviour {
	public float height;
	public float fallSpeed;
	public Sprite finalSprite;
	private AnimateFrames animate;
	private float period;
	private float initX;
	private float timer;
	void Start(){
		animate = GetComponent<AnimateFrames>();
		period = animate.frameTime * 4f;
		initX = transform.position.x;
	}
	void Update () {
		if (height > 0){
			timer += Time.deltaTime;
			Vector3 newPos = transform.position;
			newPos.y -= fallSpeed;
			newPos.x = initX + 0.05f * Mathf.Cos(timer * (2f * 3.14f) / period);
			// Debug.Log(period);
			// Debug.Log(Mathf.Cos((2f * 3.14f) / period));
			transform.position = newPos;
			height -= fallSpeed;
		} else {
			FinalState();
		}
	}
	void FinalState(){
		Destroy(this);
		// SpriteRenderer
		GetComponent<SpriteRenderer>().sprite = finalSprite;
		// AnimateFrames anim = GetComponent<AnimateFrames>();
		Destroy(animate);
	}
}
