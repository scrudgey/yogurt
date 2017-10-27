using UnityEngine;
using UnityEngine.UI;

public class CommercialReportMenu : MonoBehaviour {
    // TODO: unique events so that the top 3 aren't all different cases of cannibalism
    Text descriptionText;
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
        Destroy(gameObject);
        ScriptDirector director = GameObject.FindObjectOfType<ScriptDirector>();
        if (director){
            director.ResetScript();
        }
    }
    public void NewDayButton(){
        UINew.Instance.CloseActiveMenu();
        Controller.Instance.suspendInput = false;
		MySaver.Save();
        GameManager.Instance.BoardRoomCutscene();
    }
    private void SetRefs(){
        if (!descriptionText){
            descriptionText = transform.Find("Image/desc/WhatText").GetComponent<Text>();
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
        commercial.WriteReport();
        EventSet eventSet = new EventSet(commercial.eventData);

        SetRefs();
        descriptionText.text = activeCommercial.description;
        
        EventData total = commercial.Total();

        positiveScore.text = total.positive.ToString();
        chaosScore.text = total.chaos.ToString();
        disgustingScore.text = total.disgusting.ToString();
        disturbingScore.text = total.disturbing.ToString();
        offensiveScore.text = total.offensive.ToString();

        foreach (string line in commercial.transcript){
            transcript.text = transcript.text + line + "\n";
        }
        eventText.text = "";
        foreach (string key in commercial.properties.Keys){
            CommercialProperty prop = commercial.properties[key];
            string line = prop.desc + ": " + prop.val.ToString() + "\n";
            eventText.text = eventText.text + line;
        }
    }
}
