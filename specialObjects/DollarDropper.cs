using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollarDropper : MonoBehaviour {
    public Vector3 initLocation = Vector3.zero;
    public GameObject dollarPrefab;
    public float dropinterval;
    public Collider2D[] myColliders;

    void Start() {
        myColliders = transform.root.GetComponentsInChildren<Collider2D>();
    }
    // Update is called once per frame
    void Update() {
        if (initLocation == Vector3.zero) {
            initLocation = transform.position;
            return;
        }
        if (Vector3.Distance(initLocation, transform.position) > dropinterval) {
            DropDollar();
        }
    }
    void DropDollar() {
        myColliders = transform.root.GetComponentsInChildren<Collider2D>();
        initLocation = transform.position;
        GameObject dollar = GameObject.Instantiate(dollarPrefab, transform.position, Quaternion.identity) as GameObject;
        PhysicalBootstrapper phys = dollar.GetComponent<PhysicalBootstrapper>();
        phys.initHeight = 0.1f;
        phys.impactsMiss = true;
        phys.initVelocity = Vector3.zero;
        phys.silentImpact = true;
        phys.noCollisions = true;
        phys.InitPhysical(0.1f, Vector3.zero);
        foreach (Collider2D collider in dollar.transform.root.GetComponentsInChildren<Collider2D>()) {
            foreach (Collider2D myCollider in myColliders) {
                Physics2D.IgnoreCollision(collider, myCollider, true);
            }
        }
    }
}
