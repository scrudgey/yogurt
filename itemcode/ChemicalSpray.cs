﻿using UnityEngine;
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
                OccurrenceFire fireData = new OccurrenceFire();
                fireData.flamingObject = coll.gameObject;
                fireData.extinguished = true;

                Toolbox.Instance.OccurenceFlag(coll.gameObject, fireData, new HashSet<GameObject>(){coll.gameObject});
            }
        }
        Toolbox.Instance.AddLiveBuffs(coll.gameObject, gameObject);
        MessageDamage impact = new MessageDamage();
        impact.responsibleParty = responsibleParty;
        impact.silentImpact = true;
        Toolbox.Instance.SendMessage(coll.gameObject, this, impact);
        Destroy(gameObject);
    }
}
