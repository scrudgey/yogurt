using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputKeyIndicator : MonoBehaviour {
    public Text text;
    public InputActionReference actionReference;
    public int bindingIndex;
    void Update() {
        var displayString = string.Empty;
        var deviceLayoutName = default(string);
        var controlPath = default(string);
        // Get display string from action.
        if (actionReference != null) {
            var action = actionReference.action;
            if (action != null) {
                displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath);//, displayStringOptions);
            }
            // Debug.Log("setting displayname " + displayString);
            text.text = displayString;
        }
    }
}
