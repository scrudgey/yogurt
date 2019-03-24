using System.Collections;
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
            foreach (Achievement savedAchievement in data.achievements){
                if (savedAchievement.title != achievement.title)
                    continue;
                
                entryScript.achievement = savedAchievement;
                if (savedAchievement.complete){
                    numberComplete += 1;
                    Button entryButton = entry.GetComponent<Button>();
                    ColorBlock cb = entryButton.colors;
                    cb.normalColor = new Color(114f/255f, 185f/255f, 255f/255f, 1f);
                    entryButton.colors = cb;
                }
                if (!initializeFirst){
                    initializeFirst = true;
                    AchievementEntryCallback(savedAchievement);
                }
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
        if (achievement.complete){
            date.gameObject.SetActive(true);
            date.text = "Achieved on "+achievement.completedTime.ToShortDateString();
        } else {
            date.gameObject.SetActive(false);
            date.text = "Achieved on 4/20/69";
        }
        icon.sprite = Resources.Load<Sprite>("achievements/icons/"+achievement.icon) as Sprite;
        if (achievement.complete){
            badge.SetActive(true);
        } else {
            badge.SetActive(false);
        }
    }
    public void CloseButtonCallback(){
        startMenu.CloseAchievementBrowser();
    }
}
