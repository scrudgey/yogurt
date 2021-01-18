using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundEffect : MonoBehaviour {
    private string words;
    public bool speaking = false;
    public string[] randomPhrases;
    private float speakTime;

    private FollowGameObjectInCamera follower;
    private GameObject flipper;
    private Text bubbleText;
    void Awake() {
        if (transform.Find("SpeechChild") == null) {
            GameObject speechFramework = Instantiate(Resources.Load("UI/SpeechChild"), transform.position, Quaternion.identity) as GameObject;
            speechFramework.name = "SpeechChild";
            speechFramework.transform.SetParent(transform, false);
            speechFramework.transform.localPosition = Vector3.zero;
        }
        flipper = transform.Find("SpeechChild").gameObject;
        Transform bubbleParent = transform.Find("SpeechChild/Speechbubble");
        Canvas bubbleCanvas = bubbleParent.GetComponent<Canvas>();
        bubbleText = bubbleParent.transform.Find("Text").gameObject.GetComponent<Text>();

        bubbleText.text = "";
        follower = bubbleText.GetComponent<FollowGameObjectInCamera>();
        follower.target = gameObject;
        follower.offset = new Vector3(0f, 0.05f, 0);
        if (bubbleCanvas) {
            bubbleCanvas.worldCamera = Camera.main;
        }
        if (flipper.transform.localScale != transform.localScale.normalized) {
            Vector3 tempscale = transform.localScale.normalized;
            flipper.transform.localScale = tempscale;
        }
        foreach (Outline outline in bubbleParent.transform.Find("Text").gameObject.GetComponents<Outline>()) {
            outline.effectColor = Color.black;
        }
    }
    void Update() {
        if (speakTime > 0) {
            speakTime -= Time.deltaTime;
            speaking = true;
            follower.PreemptiveUpdate();
            bubbleText.text = words;
        }
        if (speakTime < 0) {
            Stop();
        }
    }
    public void LateUpdate() {
        // if the parent scale is flipped, we need to flip the flipper back to keep
        // the text properly oriented.
        float scale = transform.root.localScale.magnitude / 1.73205f;
        Vector3 tempscale = (1f / scale) * transform.root.localScale / scale;
        if (flipper.transform.localScale != tempscale) {
            flipper.transform.localScale = tempscale;
        }
    }
    public void Stop() {
        speakTime = 0;
        speaking = false;
        bubbleText.text = "";
        speakTime = 0;
    }
    public void Say(string message) {
        if (message == "")
            return;
        speakTime = Speech.DurationHold(message);
        words = message;
        bubbleText.color = Color.green;
    }
}
