using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiMayorDialogueSwitcher : MonoBehaviour {
    public Speech mySpeech;

    public void OnInputClicked() {
        Inventory playerInventory = GameManager.Instance.playerObject.GetComponent<Inventory>();
        if (playerInventory != null && playerInventory.holding != null) {
            string holdingName = Toolbox.Instance.GetName(playerInventory.holding.gameObject);
            if (holdingName.ToLower().Contains("silver dagger")) {
                mySpeech.defaultMonologue = "antimayor_award";
            } else {
                mySpeech.defaultMonologue = "anti_mayor";
            }
        } else {
            mySpeech.defaultMonologue = "anti_mayor";
        }
    }
}
