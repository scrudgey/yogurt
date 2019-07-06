using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VampireDialogueStarter : MonoBehaviour {
    public Speech vampireSpeech;
    public void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.name == "vampyr") {
            vampireSpeech = collider.GetComponent<Speech>();
        }
        if (collider.gameObject == GameManager.Instance.playerObject) {
            vampireSpeech.SpeakWith();
            Destroy(gameObject);
        }
    }
}
