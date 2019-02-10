using UnityEngine;
using System.Collections.Generic;

public class TreeObject : Damageable {
    HingeJoint2D hinge;
    public float timer;
    JointMotor2D motor;
    private bool doShake;
    public GameObject leaf;
    private Transform leafSpawnPoint;
    private static List<damageType> immunities = new List<damageType>(){
        damageType.asphyxiation,
        damageType.fire
    };
    public override void Awake() {
        base.Awake();
        hinge = GetComponent<HingeJoint2D>();
        motor = hinge.motor;
        leafSpawnPoint = transform.Find("leafSpawnPoint");
    }
    public override void NetIntrinsicsChanged(MessageNetIntrinsic message){
        
    }
    public void Update() {
        if (doShake) {
            timer += Time.deltaTime;
            if (timer > 1) {
                doShake = false;
                hinge.useMotor = false;
                timer = 0;
            } else {
                motor.motorSpeed = 100 * Mathf.Cos(timer * 50) * Mathf.Exp(-timer * 7f);
                hinge.motor = motor;
            }
        }
    }
    public override float CalculateDamage(MessageDamage message) {
        if (immunities.Contains(message.type))
            return 0;
        hinge.useMotor = true;
        timer = 0;
        doShake = true;
        Vector3 randomBump = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0);
        GameObject newLeaf = Instantiate(leaf, leafSpawnPoint.position + randomBump, Quaternion.identity) as GameObject;
        FallingLeaf newLeafScript = newLeaf.GetComponent<FallingLeaf>();
        newLeafScript.height = 0.9f;
        return 1f;
    }
}
