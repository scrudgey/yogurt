using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManualButton : MonoBehaviour {

    public string actionString;
    public void MouseOver() {
        UINew.Instance.actionButtonText = actionString;
    }
}
