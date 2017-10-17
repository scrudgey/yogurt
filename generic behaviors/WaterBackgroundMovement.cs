// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

public class WaterBackgroundMovement : MonoBehaviour {
	public float amplitude = 0.1f;
	public float angularVelocity = 1f;
	public float phaseOffset;
	private float timer;
	private Vector3 initialPosition;
	void Start () {
		initialPosition = transform.localPosition;
	}
	void Update () {
		timer += Time.deltaTime;
		Vector3 newPosition = initialPosition;
		newPosition.x += amplitude * Mathf.Sin(angularVelocity * timer + phaseOffset);
		transform.localPosition = newPosition;
	}
}
