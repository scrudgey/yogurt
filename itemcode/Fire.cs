using UnityEngine;
using System.Collections.Generic;

public class Fire : MonoBehaviour {
    public Flammable flammable;
    static private Dictionary<GameObject, Flammable> flammables = new Dictionary<GameObject, Flammable>();
    private MessageDamage message = new MessageDamage();
    private HashSet<GameObject> damageQueue = new HashSet<GameObject>();
    private float damageTimer;
    static List<string> forbiddenTags = new List<string>(new string[] { "occurrenceFlag", "background", "sightcone" });
    void Start() {
        message = new MessageDamage(0.15f, damageType.fire);
        message.impersonal = true;
    }
    void Update() {
        damageTimer += Time.deltaTime;
        if (damageTimer > 0.15f) {
            damageTimer = 0;
            if (flammable)
                message.responsibleParty = flammable.responsibleParty;
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
    void OnTriggerEnter2D(Collider2D coll) {
        if (!flammables.ContainsKey(coll.gameObject)) {
            GameObject baseInteractive = Controller.Instance.GetBaseInteractive(coll.gameObject.transform);
            Flammable flam = baseInteractive.GetComponentInChildren<Flammable>();
            Fire otherFire = baseInteractive.GetComponentInChildren<Fire>();
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
            // flam.heat += Time.deltaTime;
            flam.burnTimer = 1f;
            if (flammable.responsibleParty != null) {
                flam.responsibleParty = flammable.responsibleParty;
            }
        }
    }
}
