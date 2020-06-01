using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyBindMenu : MonoBehaviour {
    public List<RebindActionUI> buttons;
    public void CloseButtonCallback() {
        gameObject.SetActive(false);
    }
    public void OnEnable() {
        InputController.Instance.DisableControls();
        // trigger button update
        foreach (RebindActionUI button in buttons) {
            if (button != null)
                button.UpdateBindingDisplay();
        }
    }

    public void OnDisable() {
        InputController.Instance.EnableControls();
    }
}
