﻿using UnityEngine;
using UnityEngine.UI;

public class CommercialReportMenu : MonoBehaviour {
    // TODO: unique events so that the top 3 aren't all different cases of cannibalism
    Text descriptionText;
    Text positiveScore, chaosScore, disgustingScore, disturbingScore, offensiveScore;
    Text transcript;
    Text eventText;
    public Commercial commercial;
    public CommercialDescription eventSet;
    private Canvas canvas;
    public void Start() {
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = GameManager.Instance.cam;
        SetRefs();
        // Controller.Instance.suspendInput = true;
    }
    public void NewDayButton() {
        UINew.Instance.CloseActiveMenu();
        MySaver.Save();
        // MySaver.SaveObjectDatabase();
        if (GameManager.Instance.activeCommercial.cutscene != "none") {
            GameManager.Instance.BoardRoomCutscene();
        } else {
            GameManager.Instance.NewDayCutscene();
        }
    }
    public void ReviewButton() {
        GameObject menuObject = Instantiate(Resources.Load("UI/FocusGroupMenu")) as GameObject;
        FocusGroupMenu menu = menuObject.GetComponent<FocusGroupMenu>();
        menu.commercial = commercial;
    }
    private void SetRefs() {
        if (!descriptionText) {
            descriptionText = transform.Find("Image/desc/WhatText").GetComponent<Text>();
            positiveScore = transform.Find("Image/Center/RightHalf/RatingsPanel/Positive/Amount").GetComponent<Text>();
            chaosScore = transform.Find("Image/Center/RightHalf/RatingsPanel/Chaos/Amount").GetComponent<Text>();
            disgustingScore = transform.Find("Image/Center/RightHalf/RatingsPanel/Disgusting/Amount").GetComponent<Text>();
            disturbingScore = transform.Find("Image/Center/RightHalf/RatingsPanel/Disturbing/Amount").GetComponent<Text>();
            offensiveScore = transform.Find("Image/Center/RightHalf/RatingsPanel/Offensive/Amount").GetComponent<Text>();

            transcript = transform.Find("Image/Center/Scroll View/Viewport/content/Transcript").GetComponent<Text>();
            eventText = transform.Find("Image/Center/RightHalf/EventsPanel/Text").GetComponent<Text>();
        }
    }
    public void Report(Commercial activeCommercial) {
        commercial.WriteReport();
        SetRefs();
        descriptionText.text = commercial.SentenceReview();

        EventData total = commercial.Total();

        positiveScore.text = total.ratings[Rating.positive].ToString();
        chaosScore.text = total.ratings[Rating.chaos].ToString();
        disgustingScore.text = total.ratings[Rating.disgusting].ToString();
        disturbingScore.text = total.ratings[Rating.disturbing].ToString();
        offensiveScore.text = total.ratings[Rating.offensive].ToString();

        transcript.text = "";
        foreach (string line in commercial.transcript) {
            transcript.text = transcript.text + line + "\n";
        }
        eventText.text = "";
        foreach (string key in commercial.properties.Keys) {
            CommercialProperty prop = commercial.properties[key];
            string line = prop.desc + ": " + prop.val.ToString() + "\n";
            eventText.text = eventText.text + line;
        }
    }
}
