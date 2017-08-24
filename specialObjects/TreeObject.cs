using UnityEngine;

public class TreeObject : MonoBehaviour, IMessagable {
	HingeJoint2D hinge;
	public float timer;
	JointMotor2D motor;
	private bool doShake;
	public GameObject leaf;
	private Transform leafSpawnPoint;
	void Start () {
		hinge = GetComponent<HingeJoint2D>();
		motor = hinge.motor;
		leafSpawnPoint = transform.Find("leafSpawnPoint");
	}
	public void ReceiveMessage(Message incoming){
		if (incoming is MessageDamage){
			MessageDamage message = (MessageDamage)incoming;
			if (message.type != damageType.physical)
				return;
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
		Vector3 randomBump = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0);
		GameObject newLeaf = Instantiate(leaf, leafSpawnPoint.position + randomBump, Quaternion.identity) as GameObject;
		FallingLeaf newLeafScript = newLeaf.GetComponent<FallingLeaf>();
		newLeafScript.height = 0.9f;
	}
}
