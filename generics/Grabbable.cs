using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : Item {
    public GameObject itemPrefab;
    public bool fire;
    // Use this for initialization
    virtual public void Start() {
        Interaction getAction = new Interaction(this, "Get", "Get", true, false);
        interactions.Add(getAction);
        Pickup pickup = itemPrefab.GetComponent<Pickup>();
        itemName = pickup.itemName;
        description = pickup.description;
        longDescription = pickup.longDescription;
    }

    // Update is called once per frame
    virtual public GameObject Get(Inventory inventory) {
        GameObject item = GameObject.Instantiate(itemPrefab, transform.position, Quaternion.identity);
        if (fire) {
            Flammable itemFlammable = item.GetComponent<Flammable>();
            if (itemFlammable) {
                itemFlammable.heat += 100f;
                itemFlammable.burnTimer = 1f;
                itemFlammable.fireRetardantBuffer = 0f;
                itemFlammable.onFire = true;
            }
        }
        inventory.GetItem(item.GetComponent<Pickup>());
        Destroy(gameObject);
        return item;
    }
    public string Get_desc(Inventory inventory) {
        return "Get " + itemName;
    }
}
