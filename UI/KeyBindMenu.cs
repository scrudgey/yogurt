using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyBindMenu : MonoBehaviour {
    public void CloseButtonCallback() {
        gameObject.SetActive(false);
    }
    public void OnEnable() {
        InputController.DisableControls();
    }

    public void OnDisable() {
        InputController.EnableControls();
    }
}
