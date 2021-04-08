using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandOfCarrunos : MonoBehaviour {
    public static List<string> forbiddenTags = new List<string>(new string[] {
        "occurrenceFlag",
        "occurrenceSound",
        "background",
        "sightcone",
        "sky",
        "horizon",
        "fire",
        "zombieSpawnZone",
        "projectile"
         });

    void OnTriggerEnter2D(Collider2D coll) {
        if (forbiddenTags.Contains(coll.tag))
            return;
        if (coll.transform.IsChildOf(transform.root))
            return;
        // damageQueue.Add(coll.gameObject);
        GameObject target = InputController.Instance.GetBaseInteractive(coll.transform);
        Toolbox.Instance.AddChildIntrinsics(target, this, gameObject);
    }
    void OnTriggerExit2D(Collider2D coll) {

        if (forbiddenTags.Contains(coll.tag))
            return;
        if (coll.transform.IsChildOf(transform.root))
            return;
        GameObject target = InputController.Instance.GetBaseInteractive(coll.transform);

        Toolbox.Instance.RemoveChildIntrinsics(target, this);
    }
}
