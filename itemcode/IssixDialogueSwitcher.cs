using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IssixDialogueSwitcher : MonoBehaviour {
    public Speech mySpeech;

    void Update() {
        if (GameManager.Instance.data == null)
            return;
        if (GameManager.Instance.data.days > GameManager.HellDoorClosesOnDay && mySpeech.defaultMonologue != "issix_closed") {
            mySpeech.defaultMonologue = "issix_closed";
        } else if (GameManager.Instance.data.days <= GameManager.HellDoorClosesOnDay && mySpeech.defaultMonologue != "issix_open") {
            mySpeech.defaultMonologue = "issix_open";
        }
    }
}
