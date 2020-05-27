using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour {
    public Canvas controlsCanvas;
    public Canvas mainCanvas;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle musicToggle;
    public void Start() {
        float musicVolume = GameManager.Instance.GetMusicVolume();
        float sfxVolume = GameManager.Instance.GetSFXVolume();
        bool musicState = GameManager.Instance.GetMusicState();

        musicVolumeSlider.SetValueWithoutNotify(musicVolume);
        sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
        musicToggle.SetIsOnWithoutNotify(musicState);
    }
    public void MusicToggleChanged(Toggle change) {
        GameManager.Instance.SetMusicOn(change.isOn);
    }
    public void MusicVolumeControl(float vol) {
        GameManager.Instance.SetMusicVolume(vol);
    }
    public void SFXVolumeControl(float vol) {
        GameManager.Instance.SetSFXVolume(vol);
    }
    public void GraphicButtonCallback() {
        controlsCanvas.enabled = true;
        mainCanvas.enabled = false;
    }
}
