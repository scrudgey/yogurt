using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour {
    public Canvas controlsCanvas;
    public KeyBindMenu keybindingMenu;
    public Canvas mainCanvas;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider textSpeedSlider;
    public Toggle musicToggle;
    public void Start() {
        musicVolumeSlider.SetValueWithoutNotify(GameManager.Instance.GetMusicVolume());
        sfxVolumeSlider.SetValueWithoutNotify(GameManager.Instance.GetSFXVolume());
        textSpeedSlider.SetValueWithoutNotify(GameManager.Instance.GetDurationCoefficient());
        musicToggle.SetIsOnWithoutNotify(GameManager.Instance.GetMusicState());
    }
    public void MusicToggleChanged(Toggle change) {
        GameManager.Instance.SetMusicOn(change.isOn);
    }
    public void MusicVolumeControl(System.Single vol) {
        GameManager.Instance.SetMusicVolume(vol);
    }
    public void SFXVolumeControl(System.Single vol) {
        GameManager.Instance.SetSFXVolume(vol);
    }
    public void TextSpeedControl(System.Single vol) {
        GameManager.Instance.SetDurationCoefficient(vol);
    }
    public void GraphicButtonCallback() {
        controlsCanvas.enabled = true;
        mainCanvas.enabled = false;
    }
    public void ControlButtonCallback() {
        keybindingMenu.gameObject.SetActive(true);
    }
}
