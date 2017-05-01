// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

public class FallingLeaf : MonoBehaviour {
	public float height;
	public float fallSpeed;
	public Sprite finalSprite;
	private AnimateFrames animate;
	private float period;
	private float xInit;
	private float xAmplitude;
	private float timer;
	void Start(){
		animate = GetComponent<AnimateFrames>();
		period = animate.frameTime * 4f;
		xInit = transform.position.x;
		xAmplitude = Random.Range(0.05f, 0.1f);
	}
	void Update () {
		if (height > 0){
			timer += Time.deltaTime;
			Vector3 newPos = transform.position;
			newPos.y -= fallSpeed * Mathf.Abs(Mathf.Sin(timer * (2f * 3.14f) / period));
			newPos.x = xInit + xAmplitude * Mathf.Cos(timer * (2f * 3.14f) / period);
			// Debug.Log(period);
			// Debug.Log(Mathf.Cos((2f * 3.14f) / period));
			transform.position = newPos;
			height -= fallSpeed * Mathf.Abs(Mathf.Sin(timer * (2f * 3.14f) / period));
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
