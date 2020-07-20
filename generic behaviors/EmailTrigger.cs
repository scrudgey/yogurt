using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmailTrigger : MonoBehaviour {
    public enum EmailTriggerType { none, onDie, onKnockDown, onClick, onWear }
    public EmailTriggerType triggerType;
    public string emailFilename;
    public bool isQuitting = false;

    void OnApplicationQuit() {
        isQuitting = true;
    }
    void OnDestroy() {
        if (isQuitting)
            return;
    }

    public void OnDie() {
        // Debug.Log("On die");
        if (triggerType == EmailTriggerType.onDie) {
            SendEmail();
        }
    }
    public void OnKnockDown() {
        // Debug.Log("On knock down");
        if (triggerType == EmailTriggerType.onKnockDown) {
            SendEmail();
        }
    }
    public void SendEmail() {
        Debug.Log($"Send email {emailFilename}");
        GameManager.Instance.ReceiveEmail(emailFilename);
    }
    public void OnInputClicked() {
        if (triggerType == EmailTriggerType.onClick) {
            SendEmail();
        }
    }
}
