using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Television : Item {

    // Use this for initializationite
    void Start() {
        Interaction power = new Interaction(this, "Watch TV...", "Power");
        power.descString = "Watch TV";
        interactions.Add(power);
    }
    public void Power() {
        UINew.Instance.ShowMenu(UINew.MenuType.tv);
    }
}
