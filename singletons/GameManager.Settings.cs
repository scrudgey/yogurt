using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections;
using UnityEngine.Audio;

public partial class GameManager : Singleton<GameManager> {
    static readonly string prefsKey_MusicVolume = "GameManager_MusicVolume";
    static readonly string prefsKey_SFXVolume = "GameManager_SFXVolume";
    static readonly string prefsKey_MusicOn = "GameManager_MusicOn";
    static readonly string prefsKey_Duration = "GameManager_DurationCoefficient";
    AudioMixer sfxMixer;
    AudioMixer musicMixer;
    public void InitSettings() {
        sfxMixer = Resources.Load("mixers/SoundEffectMixer") as AudioMixer;
        musicMixer = Resources.Load("mixers/MusicMixer") as AudioMixer;

        float musicVolume = PlayerPrefs.GetFloat(prefsKey_MusicVolume, 0.8f);
        float sfxVolume = PlayerPrefs.GetFloat(prefsKey_SFXVolume, 0.8f);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }
    public void SetMusicVolume(float vol) {
        musicMixer.SetFloat("Volume", Mathf.Log10(vol) * 20);
        PlayerPrefs.SetFloat(prefsKey_MusicVolume, vol);
    }
    public void SetSFXVolume(float vol) {
        sfxMixer.SetFloat("Volume", Mathf.Log10(vol) * 20);
        PlayerPrefs.SetFloat(prefsKey_SFXVolume, vol);
    }
    public void SetMusicOn(bool value) {
        PlayerPrefs.SetInt(prefsKey_MusicOn, value ? 1 : 0);
        if (value) {
            MusicController.Instance.RestartMusic();
        } else {
            MusicController.Instance.StopTrack();
        }
    }


    public float GetMusicVolume() {
        return PlayerPrefs.GetFloat(prefsKey_MusicVolume, 1f);
    }
    public float GetSFXVolume() {
        return PlayerPrefs.GetFloat(prefsKey_SFXVolume, 1f);
    }
    public bool GetMusicState() {
        return PlayerPrefs.GetInt(prefsKey_MusicOn, 1) == 1;
    }
    public float GetDurationCoefficient() {
        return PlayerPrefs.GetFloat(prefsKey_Duration, 0.05f);
    }
    public void SetDurationCoefficient(float val) {
        PlayerPrefs.SetFloat(prefsKey_Duration, val);
    }
}

