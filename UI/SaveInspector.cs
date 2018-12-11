﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SaveInspector : MonoBehaviour {
    public Text gameNameText;
    public Text lastPlayedText;
    public Text totalTimeText;
    public Text completionText;
    public Text itemCountText;
    public Image headShot;
    public GameData data;
    public string saveName;
    public StartMenu startMenu;
    public void Initialize(StartMenu menu, SaveGameSelectorScript save){
        this.startMenu = menu;
        this.data = save.data;
        this.saveName = save.saveName;
        gameNameText.text = save.saveName;
        
        TimeSpan t = TimeSpan.FromSeconds(0f);
        if (data != null) {
            t = TimeSpan.FromSeconds(data.secondsPlayed);
            lastPlayedText.text = "Last played: " + data.saveDate.ToString();
        }
        totalTimeText.text = "Total time: " + string.Format("{0:D2}:{1:D2}:{2:D2}s",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds).ToString();

        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/" + data.headSpriteSheet);
        headShot.sprite = sprites[0];

        itemCountText.text = data.collectedObjects.Count.ToString() + "/100";
        completionText.text = "Completion: " + Completion(data).ToString("0")+"%";
    }
    public float Completion(GameData data){
        float completion = 0;
        completion += data.collectedObjects.Count / 100f;
        completion += data.completeCommercials.Count / 5f;
        return completion*100f;
    }
    public void ItemCollectionCallback(){
        startMenu.InspectItemCollection(data);
    }
    public void AchievementButtonCallback(){
        startMenu.InspectAchievements(data);
    }
    public void StatsButtonCallback(){

    }
    public void LoadButtonCallback(){
        GameManager.Instance.LoadGameDataIntoMemory(saveName);
    }
    public void CloseButtonCallback(){
        startMenu.LoadButton();
    }
}