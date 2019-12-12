using UnityEngine;
using System.Collections.Generic;
public class ChemicalSpray : MonoBehaviour {
    public GameObject collisionPlume;
    public GameObject responsibleParty;
    private float height;
    void OnCollisionEnter2D(Collision2D coll) {
        Instantiate(collisionPlume, transform.position, Quaternion.identity);
        foreach (Flammable flam in coll.gameObject.GetComponentsInParent<Flammable>()) {
            if (flam.onFire) {
                flam.onFire = false;
                flam.heat = -10f;
                flam.burnTimer = 0;
                OccurrenceFire fireData = new OccurrenceFire();
                fireData.flamingObject = coll.gameObject;
                fireData.extinguished = true;

                Toolbox.Instance.OccurenceFlag(coll.gameObject, fireData);
            }
        }
        Toolbox.Instance.AddLiveBuffs(coll.gameObject, gameObject);
        MessageDamage impact = new MessageDamage();
        impact.amount = 0;
        impact.responsibleParty = responsibleParty;
        impact.suppressImpactSound = true;
        Toolbox.Instance.SendMessage(coll.gameObject, this, impact);
        Destroy(gameObject);
    }
}
