using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saboteur : Interactive {
    void Start() {
        // if (GameManager.Instance.data.sabotageMissionsUnlocked && GameManager.Instance.data.visitedStudio) {
        Interaction enableAct = new Interaction(this, "Sabotage...", "Enable");
        enableAct.descString = "Sabotage mission...";
        enableAct.validationFunction = true;
        enableAct.holdingOnOtherConsent = false;
        interactions.Add(enableAct);

        Interaction cancelAct = new Interaction(this, "Stop", "Cancel");
        cancelAct.descString = "Abort mission";
        cancelAct.validationFunction = true;
        cancelAct.holdingOnOtherConsent = false;
        interactions.Add(cancelAct);

        if (GameManager.Instance.data != null && GameManager.Instance.data.completeCommercials.Count < 2) {
            Destroy(gameObject);
        }
    }

    public void Enable() {
        GameObject scriptSelector = UINew.Instance.ShowMenu(UINew.MenuType.scriptSelect);
        scriptSelector.GetComponent<ScriptSelectionMenu>().sabotage = true;
    }
    public bool Enable_Validation() {
        if (GameManager.Instance.data == null)
            return false;
        return !GameManager.Instance.data.recordingCommercial;
    }
    public void Cancel() {
        GameManager.Instance.SetRecordingStatus(false);
        UINew.Instance.ClearObjectives();
    }
    public bool Cancel_Validation() {
        return GameManager.Instance.data != null && GameManager.Instance.data.recordingCommercial;
    }

}
