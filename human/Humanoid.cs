﻿using UnityEngine;
// using System.Collections;
// using System.IO;

public class Humanoid : Controllable, IMessagable {
	private float baseSpeed;
	public float maxSpeed;
	public float maxAcceleration;
	public float friction;
	public float runTilt;
	private Quaternion rightTilt;
	private Quaternion leftTilt;
	private Quaternion forward;
	private Vector3 scaleVector; 
	public bool hitstun;

	private bool LoadInitialized = false;
	void Start () {
		if (!LoadInitialized)
			LoadInit();
		// this messy nonsense is just to fix the quaternions that indicate
		// direction tilt when running. there's probably a nicer way to do
		// this but I was learning unity's vectors / rotations / quaternions / 2D
		// at this time.
		Vector2 leftTiltVector = Vector2.zero;
		Vector2 rightTiltVector = Vector2.zero;
		
		leftTiltVector.x = Mathf.Cos( 1.57f - (runTilt * 6.28f / 360f));
		leftTiltVector.y = Mathf.Sin( 1.57f - (runTilt * 6.28f / 360f));
		
		rightTiltVector.x = Mathf.Cos((runTilt * 6.28f / 360f) + 1.57f);
		rightTiltVector.y = Mathf.Sin((runTilt * 6.28f / 360f) + 1.57f);
		
		rightTilt = Quaternion.LookRotation(Vector3.forward, rightTiltVector);
		leftTilt = Quaternion.LookRotation(Vector3.forward, leftTiltVector);
		forward = Quaternion.LookRotation(Vector3.forward, -1 * Vector3.forward);
	}

	void LoadInit(){
		baseSpeed = maxSpeed;
		scaleVector = Vector3.one;
	}
	

	void FixedUpdate(){
		Vector2 acceleration = Vector2.zero;
		Vector2 deceleration = Vector2.zero;
		if (hitstun){
			upFlag = false;
			downFlag = false;
			rightFlag = false;
			leftFlag = false;
			shootPressedFlag = false;
			shootHeldFlag = false;
			return;
		}
		// Do the normal controls stuff
		// set vertical force or damp if neither up nor down is held
		if (upFlag)
			acceleration.y = maxAcceleration;
		if (downFlag)
			acceleration.y = -1 * maxAcceleration;
		if (!upFlag && !downFlag){
			deceleration.y = -1 * friction * GetComponent<Rigidbody2D>().velocity.y;
		}
		// set horizontal force, or damp is neither left nor right held
		if (leftFlag){
			acceleration.x = -1 * maxAcceleration;
			transform.rotation = Quaternion.Lerp (transform.rotation,leftTilt,0.1f);
		}
		if (rightFlag){
			acceleration.x = maxAcceleration;
			transform.rotation = Quaternion.Lerp (transform.rotation,rightTilt,0.1f);
		}
		if (!rightFlag && !leftFlag){
			deceleration.x = -1 * friction * GetComponent<Rigidbody2D>().velocity.x;
			transform.rotation = Quaternion.Lerp (transform.rotation,forward,0.1f);
		}
		// apply force
		GetComponent<Rigidbody2D>().AddForce(acceleration+deceleration);

		// clamp velocity to maximum
		// there's probably a more efficient way to do this calculation but whatevs
		if (GetComponent<Rigidbody2D>().velocity.magnitude > maxSpeed)
			GetComponent<Rigidbody2D>().velocity = Vector2.ClampMagnitude(GetComponent<Rigidbody2D>().velocity, maxSpeed);

		// use the scale x trick for left-facing animations
		Vector2 vel = GetComponent<Rigidbody2D>().velocity;
		if (vel.x < -0.1){
			scaleVector.x = -1;
		}
		if (vel.x > 0.1){
			scaleVector.x = 1;
		}
		transform.localScale = scaleVector;
	}

	public override void ReceiveMessage(Message message){
		base.ReceiveMessage(message);

		if (message is MessageIntrinsic){
			MessageIntrinsic intrins = (MessageIntrinsic)message;
			if (intrins.netIntrinsic != null){
				maxSpeed = baseSpeed + intrins.netIntrinsic.speed.floatValue;
			}
		}
		if (message is MessageHitstun){
			MessageHitstun hits = (MessageHitstun)message;
			hitstun = hits.value;
		}
	}

	public void UpdateDirection(){
		float angle = Toolbox.Instance.ProperAngle(direction.x, direction.y);
		// change lastpressed because this is relevant to animation
		if (angle > 315 || angle < 45){
			lastPressed = "right";
			scaleVector.x = 1;
		} else if (angle >= 45 && angle <= 135) {
			lastPressed = "up";
		} else if (angle >= 135 && angle < 225) {
			lastPressed = "right";
			scaleVector.x = -1;
		} else if (angle >= 225 && angle < 315) {
			lastPressed = "down";
		}
		transform.localScale = scaleVector;
	}
	public override void SetDirection(Vector2 d){
		base.SetDirection(d);
		UpdateDirection();
	}

}
