using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiMayorDialogueSwitcher : MonoBehaviour {
    public Speech mySpeech;
    void Update() {
        if (GameManager.Instance.data.foughtSpiritToday && mySpeech.defaultMonologue != "antimayor_award") {
            mySpeech.defaultMonologue = "antimayor_award";
        }
    }
}
