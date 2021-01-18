using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gunpowder : MonoBehaviour {
    public GameObject gunpowderPile;
    void OnGroundImpact(Physical phys) {
        // EventData data = Toolbox.Instance.DataFlag(gameObject, chaos: 1, disgusting: 1);
        // data.whatHappened = liquid.name + " was spilled";
        // data.noun = "spilling";

        GameObject puddle = Instantiate(gunpowderPile, transform.position, Quaternion.identity) as GameObject;
        PhysicalBootstrapper pb = GetComponent<PhysicalBootstrapper>();
        pb.DestroyPhysical();
        // Liquid.MonoLiquidify(puddle, liquid);
        // Edible puddleEdible = puddle.GetComponent<Edible>();
        // Edible edible = GetComponent<Edible>();
        // if (edible) {
        //     puddleEdible.human = edible.human;
        //     puddleEdible.offal = true;
        // }
        Destroy(gameObject);
        // ClaimsManager.Instance.WasDestroyed(gameObject);
    }
}
