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
    public void Done(){
        // Time.timeScale = 1f;
        Destroy(gameObject);
    }
    
    private void SetRefs(){
        if (!titleText){
            titleText = transform.Find("Image/TitleText").GetComponent<Text>();
            descriptionText = transform.Find("Image/WhatText").GetComponent<Text>();
            rewardText = transform.Find("Image/RewardText").GetComponent<Text>();
        }
    }
    
    public void Report(Commercial commercial){
        SetRefs();
        titleText.text = commercial.name;
        descriptionText.text = commercial.description;
        rewardText.text = commercial.reward.ToString();
    }
    
}
