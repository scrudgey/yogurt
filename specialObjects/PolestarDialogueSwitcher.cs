using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolestarDialogueSwitcher : MonoBehaviour {
    public Speech mySpeech;

    void Update() {
        if (GameManager.Instance.data.teleporterUnlocked && mySpeech.defaultMonologue != "polestar") {
            mySpeech.defaultMonologue = "polestar";
        }
    }
}
