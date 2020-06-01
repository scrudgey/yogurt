using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DefaultButtonIndicator : MonoBehaviour {
    public Text text;
    void Start() {
        var displayString = string.Empty;
        var deviceLayoutName = default(string);
        var controlPath = default(string);
        // Get display string from action.
        var action = InputController.Instance.FireAction.action;
        if (action != null) {
            displayString = action.GetBindingDisplayString(4, out deviceLayoutName, out controlPath);//, displayStringOptions);
        }
        // Debug.Log("setting displayname " + displayString);
        text.text = displayString;
    }

}
