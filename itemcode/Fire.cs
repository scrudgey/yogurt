using UnityEngine;
using System.Collections.Generic;

public class Fire : MonoBehaviour {
    public Flammable flammable;
    static private Dictionary<GameObject, Flammable> flammables = new Dictionary<GameObject, Flammable>();
    private MessageDamage message = new MessageDamage();
    private HashSet<GameObject> damageQueue = new HashSet<GameObject>();
    // private HashSet<GameObject> collidedObjects = new HashSet<GameObject>();
    private float damageTimer;
    static List<string> forbiddenTags = new List<string>(new string[] { "occurrenceFlag", "background", "sightcone" });
    void Start() {
        message = new MessageDamage(0.15f, damageType.fire);
    }
    void Update() {
        damageTimer += Time.deltaTime;
        if (damageTimer > 0.15f) {
            damageTimer = 0;
            if (flammable)
                message.responsibleParty = flammable.responsibleParty;
            message.impersonal = true;
            // foreach (GameObject obj in collidedObjects) {
            //     if (flammables.ContainsKey(obj)) {
            //         Flammable targetFlam = flammables[obj];
            //         // targetFlam.heat += Time.deltaTime * 2f;
            //         // if (flammable.responsibleParty != null) {
            //         //     targetFlam.responsibleParty = flammable.responsibleParty;
            //         // }
            //     }
            // }
            // collidedObjects = new HashSet<GameObject>();

            // process the objects in the damage queue.
            // do not send a damage message to anything above me in the transform tree:
            // this is so that the player can hold a flaming object without being hurt.
            foreach (GameObject obj in damageQueue) {
                if (obj == null)
                    continue;
                if (transform.IsChildOf(obj.transform.root))
                    continue;
                if (obj != null) {
                    Toolbox.Instance.SendMessage(obj, this, message);
                }
            }
            damageQueue = new HashSet<GameObject>();
        }
    }

    // }
    // void Update(){
    //     _transforms = (Transform[])transform.root.GetComponentsInChildren( typeof(Transform), true);
    // }
    void OnTriggerEnter2D(Collider2D coll) {
        if (!flammables.ContainsKey(coll.gameObject)) {
            Flammable flam = coll.GetComponentInParent<Flammable>();
            Fire otherFire = coll.GetComponentInParent<Fire>();
            if (flam != null && flam != flammable) {
                flammables.Add(coll.gameObject, flam);
                return;
            }
            if (otherFire != null && otherFire != this) {
                flammables.Add(coll.gameObject, otherFire.flammable);
            }
        }
    }

    void OnTriggerStay2D(Collider2D coll) {
        if (!flammable.onFire)
            return;
        if (forbiddenTags.Contains(coll.tag))
            return;
        if (coll.transform.IsChildOf(transform.root))
            return;
        Flammable flam = null;
        damageQueue.Add(coll.gameObject);
        if (flammables.TryGetValue(coll.gameObject, out flam)) {
            flam.heat += Time.deltaTime;
            if (flammable.responsibleParty != null) {
                flam.responsibleParty = flammable.responsibleParty;
            }
        }
    }
}
