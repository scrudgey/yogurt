using UnityEngine;
using UnityEngine.UI;
// using System.Collections;

public class CommercialReportMenu : MonoBehaviour {
    // Text titleText;
    Text descriptionText;
    Text rewardText;
    Text positiveScore, chaosScore, disgustingScore, disturbingScore, offensiveScore;
    Text transcript;
    Text eventText;
    private Canvas canvas;
    public void Start(){
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = GameManager.Instance.cam;
        SetRefs();
        Controller.Instance.suspendInput = true;
    }

    public void ContinueButton(){
        Controller.Instance.suspendInput = false;
        GameManager.Instance.activeCommercial = null;
        Destroy(gameObject);
        UINew.Instance.EnableRecordButtons(false);
        ScriptDirector director = GameObject.FindObjectOfType<ScriptDirector>();
        if (director){
            director.Disable();
        }
    }
    public void NewDayButton(){
        Controller.Instance.suspendInput = false;
        GameManager.Instance.NewDayCutscene();
    }
    
    private void SetRefs(){
        if (!descriptionText){
            // titleText = transform.Find("Image/titles/TitleText").GetComponent<Text>();
            descriptionText = transform.Find("Image/desc/WhatText").GetComponent<Text>();
            rewardText = transform.Find("Image/buttons/rew/RewardText").GetComponent<Text>();
            positiveScore = transform.Find("Image/Center/RightHalf/RatingsPanel/Positive/Amount").GetComponent<Text>();
            chaosScore = transform.Find("Image/Center/RightHalf/RatingsPanel/Chaos/Amount").GetComponent<Text>();
            disgustingScore = transform.Find("Image/Center/RightHalf/RatingsPanel/Disgusting/Amount").GetComponent<Text>();
            disturbingScore = transform.Find("Image/Center/RightHalf/RatingsPanel/Disturbing/Amount").GetComponent<Text>();
            offensiveScore = transform.Find("Image/Center/RightHalf/RatingsPanel/Offensive/Amount").GetComponent<Text>();
            transcript = transform.Find("Image/Center/TranscriptPanel/Transcript").GetComponent<Text>();
            eventText = transform.Find("Image/Center/RightHalf/EventsPanel/Text").GetComponent<Text>();
        }
    }
    
    public void Report(Commercial activeCommercial, Commercial commercial){
        SetRefs();
        // titleText.text = commercial.name;
        descriptionText.text = activeCommercial.description;
        rewardText.text = activeCommercial.reward.ToString();

        positiveScore.text = commercial.data.positive.ToString();
        chaosScore.text = commercial.data.chaos.ToString();
        disgustingScore.text = commercial.data.disgusting.ToString();
        disturbingScore.text = commercial.data.disturbing.ToString();
        offensiveScore.text = commercial.data.offensive.ToString();

        foreach (string line in commercial.transcript){
            transcript.text = transcript.text + line + "\n";
        }

        eventText.text = "";
        foreach (string key in commercial.properties.Keys){
            CommercialProperty prop = commercial.properties[key];
            string elaboration = VideoCamera.KeyDescriptions[key];
            string line = elaboration + ": " + prop.val.ToString() + "\n";
            eventText.text = eventText.text + line;
        }
    }
    
}
