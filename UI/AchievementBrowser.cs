﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class AchievementBrowser : MonoBehaviour {
    public Text title;
    public Text description;
    public Text date;
    public Text counter;
    public Text directive;
    public Transform scrollArea;
    public GameObject entryPrefab;
    public Image icon;

    public StartMenu startMenu;

    public void Initialize(GameData data){
        foreach (Transform child in scrollArea){
            Destroy(child.gameObject);
        }
        List<Achievement> allAchievements = LoadAchievements();
        bool initializeFirst = false;
        int numberComplete = 0;
        foreach (Achievement achievement in allAchievements){
            GameObject entry = GameObject.Instantiate(entryPrefab) as GameObject;
            entry.transform.SetParent(scrollArea, false);
            Image entryImage = entry.transform.Find("Image/Image").GetComponent<Image>();
            Text entryText = entry.transform.Find("Image/Text").GetComponent<Text>();

            entryText.text = achievement.title;
            Sprite icon = Resources.Load<Sprite>("achievements/icons/"+achievement.icon) as Sprite;
            entryImage.sprite = icon;

            AchievementEntry entryScript = entry.GetComponent<AchievementEntry>();
            entryScript.browser = this;
            entryScript.achievement = achievement;

            if (!initializeFirst){
                AchievementEntryCallback(achievement);
            }
            if (achievement.complete){
                numberComplete += 1;
            }
            // if i didn't collect this achievement
        }
        counter.text = numberComplete.ToString() + "/"+ allAchievements.Count.ToString() + "\nComplete";
    }
    public List<Achievement> LoadAchievements(){
        List<Achievement> achievements = new List<Achievement>();
        GameObject[] achievementPrefabs = Resources.LoadAll("achievements/", typeof(GameObject))
            .Cast<GameObject>()
            .ToArray();
        foreach (GameObject prefab in achievementPrefabs) {
            AchievementComponent component = prefab.GetComponent<AchievementComponent>();
            if (component) {
                achievements.Add(component.achivement);
            }
        }
        return achievements;
    }
    public void AchievementEntryCallback(Achievement achievement){
        // play sound

        title.text = achievement.title;
        description.text = achievement.description;
        directive.text = achievement.directive;
        date.text = "Achieved on 4/20/69";
        icon.sprite = Resources.Load<Sprite>("achievements/icons/"+achievement.icon) as Sprite;
    }
    public void CloseButtonCallback(){
        startMenu.CloseAchievementBrowser();
    }
}