using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingCutsceneController : MonoBehaviour {
    [Header("Base Objects")]
    public GameObject objLongShot;
    public GameObject objViewingRoom;
    public GameObject objStreet;
    public GameObject objNews;
    public GameObject objHQ;
    public GameObject objCanvas;
    public Text settingText;
    [Header("Viewing")]
    public Speech ViewingMoeSpeech;
    public Speech ViewingCurlySpeech;
    public Speech ViewingLarrySpeech;
    public Speech ViewingSatanSpeech;
    [Header("Street")]
    public Speech StreetMoeSpeech;
    public Speech StreetCurlySpeech;
    public Speech StreetSatanSpeech;
    [Header("News")]
    public Speech NewsMoeSpeech;
    [Header("HQ")]
    public Speech HQMoeSpeech;
    public Speech HQCurlySpeech;
    public Speech HQLarrySpeech;
    public Speech HQCEOSpeech;
    // public void CleanUp() {
    // moeControl.Dispose();
    // larryControl.Dispose();
    // curlyControl.Dispose();
    // ceoControl.Dispose();
    // bartenderControl.Dispose();
    // }
}
