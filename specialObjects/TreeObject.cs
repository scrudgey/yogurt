using UnityEngine;
using System.Collections.Generic;

public class TreeObject : Damageable {
    HingeJoint2D hinge;
    public float timer;
    JointMotor2D motor;
    private bool doShake;
    public GameObject leaf;
    private Transform leafSpawnPoint;
    public float treeHealth = 500f;
    public HingeJoint2D hinger;
    public SpringJoint2D springer;
    public bool cutdown;
    public Rigidbody2D body;
    public float refractoryPeriod;
    private static List<damageType> immunities = new List<damageType>(){
        damageType.asphyxiation,
        damageType.fire,
        damageType.acid
    };
    public override void Awake() {
        base.Awake();
        hinge = GetComponent<HingeJoint2D>();
        motor = hinge.motor;
        leafSpawnPoint = transform.Find("leafSpawnPoint");
        body = GetComponent<Rigidbody2D>();
    }
    override protected void Update() {
        base.Update();
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
    public override void CalculateDamage(MessageDamage message) {
        if (cutdown)
            return;
        if (immunities.Contains(message.type))
            return;

        hinge.useMotor = true;
        timer = 0;
        doShake = true;
        if (refractoryPeriod > 0) {
            refractoryPeriod -= Time.deltaTime;
        } else {
            Vector3 randomBump = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0);
            GameObject newLeaf = Instantiate(leaf, leafSpawnPoint.position + randomBump, Quaternion.identity) as GameObject;
            FallingLeaf newLeafScript = newLeaf.GetComponent<FallingLeaf>();
            newLeafScript.height = Random.Range(0.8f, 0.85f);
            refractoryPeriod = Random.Range(1f, 2f);
        }
        if (message.type == damageType.cutting) {
            treeHealth -= message.amount;
            if (treeHealth <= 0) {
                CutDown();
            }
        }
    }

    public void CutDown() {
        cutdown = true;
        hinger.useLimits = false;
        springer.enabled = false;
        body.gravityScale = 1f;
    }
}
