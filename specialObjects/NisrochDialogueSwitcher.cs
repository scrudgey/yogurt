using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NisrochDialogueSwitcher : MonoBehaviour {

    public Speech mySpeech;

    public void OnInputClicked() {
        if (GameManager.Instance.playerObject.gameObject.name.ToLower().StartsWith("demon")) {
            Debug.Log("switch dialogue: demon player");
            mySpeech.defaultMonologue = "chef_demon";
        } else {
            Debug.Log("switch dialogue: demon player");
            mySpeech.defaultMonologue = "chef";
        }
    }
}
