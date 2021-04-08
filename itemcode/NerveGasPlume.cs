using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NerveGasPlume : MonoBehaviour {
    // start with random velocity
    // x fast but rapidly slowing 
    // x flipping tied to velocity
    // trigger collider applies the following intrinsics  
    //     coughing
    //     oxygen damage(?)
    //     acid damage
    Rigidbody2D body;
    public GameObject responsibleParty;
    // TODO: periodically clean this up
    public static HashSet<GameObject> triggeredObjects = new HashSet<GameObject>();
    void Start() {
        body = GetComponent<Rigidbody2D>();
        body.velocity = Random.insideUnitCircle * 5f;
    }
    void OnTriggerEnter2D(Collider2D coll) {
        if (WandOfCarrunos.forbiddenTags.Contains(coll.tag))
            return;
        if (coll.transform.IsChildOf(transform.root))
            return;
        // damageQueue.Add(coll.gameObject);
        GameObject target = InputController.Instance.GetBaseInteractive(coll.transform);
        Toolbox.Instance.AddLiveBuffs(target, gameObject);

        if (!triggeredObjects.Contains(target)) {
            triggeredObjects.Add(target);
            EventData headExpldeData = EventData.ChemicalWeapon(target, responsibleParty);
            Toolbox.Instance.OccurenceFlag(target, headExpldeData);
        }
    }
}
