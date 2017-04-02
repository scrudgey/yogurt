using UnityEngine;

public class TreeObject : MonoBehaviour, IMessagable {
	HingeJoint2D hinge;
	public float timer;
	JointMotor2D motor;
	private bool doShake;
	void Start () {
		hinge = GetComponent<HingeJoint2D>();
		motor = hinge.motor;
	}
	public void ReceiveMessage(Message incoming){
		if (incoming is MessageDamage){
			MessageDamage message = (MessageDamage)incoming;
			if (message.impactor)
					message.impactor.PlayImpactSound();
			StartShake();
		}
	}
	public void Update(){
		if (doShake){
			timer += Time.deltaTime;
			if (timer > 1){
				doShake = false;
				hinge.useMotor = false;
				timer = 0;
			} else {
				motor.motorSpeed = 100 * Mathf.Cos(timer*50) * Mathf.Exp(-timer * 7f);
				hinge.motor = motor;
			}
		}
	}
	public void StartShake(){
		hinge.useMotor = true;
		timer = 0;
		doShake = true;
		// HingeJoint2D.
		// JointMotor2D newMotor = hinge.motor;
		// motor.motorSpeed = 1;
		// hinge.motor = newMotor;
		// hinge.motor.motorSpeed = 1;
	}
}
