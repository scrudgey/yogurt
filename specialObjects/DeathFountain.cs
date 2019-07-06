using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathFountain : Interactive {
    public MessageDamage message = new MessageDamage(999, damageType.cosmic);
    void Start() {
        Interaction drink = new Interaction(this, "Drink", "Drink");
        drink.descString = "Drink from fountain";
        interactions.Add(drink);
    }

    public void Drink(Eater eater) {
        Toolbox.Instance.SendMessage(eater.gameObject, this, message);
    }

}
