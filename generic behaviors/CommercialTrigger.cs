using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommercialTrigger : MonoBehaviour {
    public enum CommercialTriggerType { none, onDie, onKnockDown, onClick, onWear, onEnter, onStart }
    public CommercialTriggerType triggerType;
    public string commercialFilename;
    public bool isQuitting = false;
    public BoxCollider2D zone;
    void Start() {
        // Debug.Log("starting");
        if (triggerType == CommercialTriggerType.onStart) {
            SendEmail();
        }
    }
    void OnApplicationQuit() {
        isQuitting = true;
    }
    void OnDestroy() {
        if (isQuitting)
            return;
    }

    public void OnDie() {
        // Debug.Log("On die");
        if (triggerType == CommercialTriggerType.onDie) {
            SendEmail();
        }
    }
    public void OnKnockDown() {
        // Debug.Log("On knock down");
        if (triggerType == CommercialTriggerType.onKnockDown) {
            SendEmail();
        }
    }
    public void SendEmail() {
        // Debug.Log($"Send email {commercialFilename}");
        // GameManager.Instance.ReceiveEmail(commercialFilename);
        GameManager.Instance.UnlockCommercial(commercialFilename);
    }
    public void OnInputClicked() {
        if (triggerType == CommercialTriggerType.onClick) {
            SendEmail();
        }
    }
    void Update() {
        if (zone != null && GameManager.Instance.playerObject != null) {
            if (triggerType == CommercialTriggerType.onEnter) {
                if (zone.bounds.Contains(GameManager.Instance.playerObject.transform.position)) {
                    SendEmail();
                    Destroy(gameObject);
                }
            }
        }

    }
}
