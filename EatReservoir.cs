using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatReservoir : Interactive {
    public GameObject edible;
    public string description;
    void Start() {
        Interaction drinker = new Interaction(this, "Eat", "Drink");
        drinker.selfOnOtherConsent = true;
        interactions.Add(drinker);
    }
    public void Drink(Eater eater) {
        if (eater) {
            GameObject sip = Instantiate(edible, transform.position, Quaternion.identity) as GameObject;
            eater.Eat(sip.GetComponent<Edible>());
            GameManager.Instance.CheckItemCollection(gameObject, eater.gameObject);
        }
    }
    public string Drink_desc(Eater eater) {
        return description;
    }
}
