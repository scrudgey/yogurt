using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {
    public void MusicToggleChanged(Toggle change) {
        GameManager.settings.musicOn = change.isOn;
    }
}
