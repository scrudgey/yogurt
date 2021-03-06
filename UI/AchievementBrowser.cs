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
    public GameObject badge;
    public Transform scrollArea;
    public GameObject entryPrefab;
    public Image icon;
    public StartMenu startMenu;
    private UIButtonEffects effects;
    public Button closeButton;

    public void Initialize(GameData data) {
        effects = GetComponent<UIButtonEffects>();
        effects.buttons = new List<Button>() { closeButton };
        foreach (Transform child in scrollArea) {
            Destroy(child.gameObject);
        }
        List<Achievement> allAchievements = LoadAchievements();
        bool initializeFirst = false;
        int numberComplete = 0;
        foreach (Achievement achievement in allAchievements) {
            GameObject entry = GameObject.Instantiate(entryPrefab) as GameObject;
            AchievementEntry entryScript = entry.GetComponent<AchievementEntry>();
            entryScript.Initialize(achievement, this);

            effects.buttons.Add(entryScript.entryButton);
            entry.transform.SetParent(scrollArea, false);


            foreach (Achievement savedAchievement in data.achievements) {
                if (savedAchievement.title != achievement.title)
                    continue;

                entryScript.achievement = savedAchievement;
                if (savedAchievement.complete) {
                    numberComplete += 1;
                    entryScript.Complete();
                }
                if (!initializeFirst) {
                    initializeFirst = true;
                    AchievementEntryCallback(savedAchievement);
                }
            }
        }
        effects.Configure();
        counter.text = numberComplete.ToString() + "/" + allAchievements.Count.ToString() + "\nComplete";
    }
    public List<Achievement> LoadAchievements() {
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
    public void AchievementEntryCallback(Achievement achievement) {
        // play sound
        title.text = achievement.title;
        description.text = achievement.description;
        directive.text = achievement.directive;
        if (achievement.complete) {
            date.gameObject.SetActive(true);
            date.text = "Achieved on " + achievement.completedTime.ToShortDateString();
        } else {
            date.gameObject.SetActive(false);
            date.text = "Achieved on 4/20/69";
        }
        icon.sprite = Resources.Load<Sprite>("achievements/icons/" + achievement.icon) as Sprite;
        if (achievement.complete) {
            badge.SetActive(true);
        } else {
            badge.SetActive(false);
        }
    }
    public void CloseButtonCallback() {
        startMenu.CloseAchievementBrowser();
    }
}
