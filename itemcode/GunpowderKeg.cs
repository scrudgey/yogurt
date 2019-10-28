using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunpowderKeg : Interactive {
    public GameObject gunpowder;
    private PhysicalBootstrapper pBoot;
    private Pickup pickup;
    public int amount = 5;
    public float spaceInterval = 0.2f;
    private Vector3 lastDropPoint = Vector3.zero;
    void Start() {
        pBoot = GetComponent<PhysicalBootstrapper>();
        pickup = GetComponent<Pickup>();
        if (pBoot == null) {
            Debug.Log("no bootstrapper found for powder keg");
            Destroy(this);
        }
    }
    void Update() {
        if (Vector3.Distance(transform.position, lastDropPoint) > spaceInterval) {
            if (pBoot != null && pBoot.physical != null && pBoot.physical.height > 0.08) {
                Drip(pBoot.physical.height);
            }
            if (pickup != null && pickup.holder != null) {
                Drip(pickup.holder.dropHeight);
            }
            lastDropPoint = transform.position;
        }
    }
    void Drip(float height) {
        if (amount <= 0)
            return;
        amount -= 1;
        // timer = 0;
        Vector3 initPosition = transform.position;
        initPosition.z = height;
        GameObject droplet = GameObject.Instantiate(gunpowder, initPosition, Quaternion.identity);
        PhysicalBootstrapper phys = droplet.GetComponent<PhysicalBootstrapper>();
        phys.initHeight = height;
        phys.impactsMiss = true;
        phys.silentImpact = true;
        if (pickup != null) {
            if (pickup.holder != null) {
                foreach (Collider2D holderCollider in pickup.holder.GetComponentsInChildren<Collider2D>()) {
                    Collider2D dropCollider = droplet.GetComponent<Collider2D>();
                    Physics2D.IgnoreCollision(holderCollider, dropCollider, true);
                }
            }
        }
    }
    public void SaveData(PersistentComponent data) {
        data.ints["amount"] = amount;
    }
    public void LoadData(PersistentComponent data) {
        amount = data.ints["amount"];
    }
}
