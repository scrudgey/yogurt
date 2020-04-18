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
        Interaction f = new Interaction(this, "Fire", "Fire");
        f.holdingOnOtherConsent = false;
        f.otherOnSelfConsent = false;
        f.descString = "Use lighter";
        f.defaultPriority = 2;
        interactions.Add(f);
        flameRadius = transform.Find("flameRadius").GetComponent<Collider2D>();
        flameRadius.enabled = false;
        Toolbox.RegisterMessageCallback<MessageDamage>(this, HandleMessageDamage);
    }
    public void HandleMessageDamage(MessageDamage message) {
        if (message.type == damageType.asphyxiation && flameon) {
            StopFire();
        }
    }
    public void Fire() {
        flameon = !flameon;
        if (flameon) {
            StartFire();
        } else {
            StopFire();
        }
    }
    private void StartFire() {
        fire.Play();
        flameRadius.enabled = true;
    }
    private void StopFire() {
        fire.Stop();
        flameRadius.enabled = false;
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
            // flammables[coll.gameObject].heat += Time.deltaTime * 2f;
            flammables[coll.gameObject].burnTimer = 1f;
            if (pickup != null) {
                if (pickup.holder != null) {
                    flammables[coll.gameObject].responsibleParty = pickup.holder.gameObject;
                }
            }
        }
    }
}