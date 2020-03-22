using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour {
    AudioMixer sfxMixer;
    AudioMixer musicMixer;
    void Start() {
        sfxMixer = Resources.Load("mixers/SoundEffectMixer") as AudioMixer;
        musicMixer = Resources.Load("mixers/MusicMixer") as AudioMixer;
    }
    public void MusicToggleChanged(Toggle change) {
        GameManager.settings.musicOn = change.isOn;
    }
    public void MusicVolumeControl(float vol) {
        musicMixer.SetFloat("Volume", Mathf.Log10(vol) * 20);
    }
    public void SFXVolumeControl(float vol) {
        sfxMixer.SetFloat("Volume", Mathf.Log10(vol) * 20);
    }
}
