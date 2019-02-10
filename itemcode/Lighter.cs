using UnityEngine;
using System.Collections.Generic;

public class Lighter : Interactive {
    public ParticleSystem fire;
    private Pickup pickup;
    private bool flameon;
    private Collider2D flameRadius;
    private Dictionary<GameObject, Flammable> flammables = new Dictionary<GameObject, Flammable>();
    static List<string> forbiddenTags = new List<string>(new string[] { "occurrenceFlag", "background", "sightcone" });
    void Start() {
        pickup = GetComponent<Pickup>();
        Interaction f = new Interaction(this, "Fire", "Fire", false, true);
        f.defaultPriority = 1;
        interactions.Add(f);
        flameRadius = transform.Find("flameRadius").GetComponent<Collider2D>();
        flameRadius.enabled = false;
    }
    public void Fire() {
        flameon = !flameon;
        if (flameon) {
            fire.Play();
            flameRadius.enabled = true;
        } else {
            fire.Stop();
            flameRadius.enabled = false;
        }
    }
    void OnTriggerEnter2D(Collider2D coll) {
        
        if (!flammables.ContainsKey(coll.gameObject)) {
            Flammable flammable = coll.GetComponent<Flammable>();
            if (flammable != null) {
                flammables.Add(coll.gameObject, flammable);
            }
        }
    }
    void OnTriggerStay2D(Collider2D coll) {
        if (forbiddenTags.Contains(coll.tag))
            return;
        if (coll.transform.IsChildOf(transform.root))
            return;
        if (flammables.ContainsKey(coll.gameObject) && flameRadius.enabled) {
            flammables[coll.gameObject].heat += Time.deltaTime * 2f;
            if (pickup != null) {
                if (pickup.holder != null)
                    flammables[coll.gameObject].responsibleParty = pickup.holder.gameObject;
            }
        }
    }
}