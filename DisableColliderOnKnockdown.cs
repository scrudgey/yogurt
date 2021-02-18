using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DisableColliderOnKnockdown : MonoBehaviour {

    //  AKA the most terrible unforgivable hack

    public Collider2D target;
    void Start() {
        if (SceneManager.GetActiveScene().name != "forest") {
            Destroy(this);
        }
    }
    public void OnKnockDown() {
        target.enabled = false;
    }
    public void OnGetUp() {
        target.enabled = true;
    }
}
