using UnityEngine;
using UnityEngine.UI;
// using System.Collections;

public class CommercialReportMenu : MonoBehaviour {
    Text titleText;
    Text descriptionText;
    Text rewardText;
    private Canvas canvas;
    public void Start(){
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = GameManager.Instance.cam;
        SetRefs();
        // Time.timeScale = 0.0f;
    }

    public void ContinueButton(){
        GameManager.Instance.activeCommercial = null;
        Destroy(gameObject);
    }
    public void NewDayButton(){
        GameManager.Instance.NewDay();
    }
    
    private void SetRefs(){
        if (!titleText){
            titleText = transform.Find("Image/titles/TitleText").GetComponent<Text>();
            descriptionText = transform.Find("Image/desc/WhatText").GetComponent<Text>();
            rewardText = transform.Find("Image/rew/RewardText").GetComponent<Text>();
        }
    }
    
    public void Report(Commercial commercial){
        SetRefs();
        titleText.text = commercial.name;
        descriptionText.text = commercial.description;
        rewardText.text = commercial.reward.ToString();
    }
    
}
