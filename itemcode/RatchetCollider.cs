using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatchetCollider : MonoBehaviour {
    public BoxCollider2D ratchet;
    void OnTriggerEnter2D(Collider2D collider) {
        if (InputController.forbiddenTags.Contains(collider.tag))
            return;
        if (collider.transform.root.gameObject == GameManager.Instance.playerObject)
            ratchet.enabled = true;
    }
}
