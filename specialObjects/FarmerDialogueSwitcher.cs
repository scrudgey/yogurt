using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmerDialogueSwitcher : MonoBehaviour {
    public Speech mySpeech;

    public void OnInputClicked() {
        Inventory playerInventory = GameManager.Instance.playerObject.GetComponent<Inventory>();
        if (playerInventory != null && playerInventory.holding != null) {
            string holdingName = Toolbox.Instance.GetName(playerInventory.holding.gameObject);
            if (holdingName.ToLower().Contains("money")) {
                mySpeech.defaultMonologue = "farmer_money";
            } else {
                mySpeech.defaultMonologue = "farmer";
            }
        } else {
            mySpeech.defaultMonologue = "farmer";
        }
    }
}
