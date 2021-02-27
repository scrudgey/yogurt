using System.Collections;
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
    public Text commercialCountText;
    public Text locationCountText;
    public Text recipeCountText;
    public Text achievementCountText;

    public Text itemPercentText;
    public Text commercialPercentText;
    public Text locationPercentText;
    public Text recipePercentText;
    public Text achievementPercentText;

    public Image headShot;
    public GameData data;
    public string saveName;
    public StartMenu startMenu;
    public void Initialize(StartMenu menu, SaveGameSelectorScript save) {
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
        headShot.sprite = Toolbox.ApplySkinToneToSprite(sprites[0], data.headSkinColor);

        SetCompletionText(data);

        completionText.text = "Completion: " + Completion(data).ToString("0") + "%";
    }
    public void SetCompletionText(GameData gameData) {
        void SetText(Text text, Text percentText, CompletionStat stat, bool divideByTwo = false) {
            float readableFrac = stat.fraction * 100f;
            if (divideByTwo) {
                text.text = $"{stat.count / 2} / {stat.max / 2}";
            } else {
                text.text = $"{stat.count} / {stat.max}";
            }
            percentText.text = $"{readableFrac.ToString("0")}%";
        }

        CompletionData data = new CompletionData(gameData);

        SetText(itemCountText, itemPercentText, data.objectStat);
        SetText(achievementCountText, achievementPercentText, data.achievementStat);
        SetText(locationCountText, locationPercentText, data.levelStat);
        SetText(recipeCountText, recipePercentText, data.recipeStat, divideByTwo: true);
        SetText(commercialCountText, commercialPercentText, data.commercialStat);
    }
    public static float Completion(GameData gameData) {

        CompletionData data = new CompletionData(gameData);

        float completion = 0;

        completion += 0.2f * data.objectStat.fraction;
        completion += 0.2f * data.commercialStat.fraction;
        completion += 0.2f * data.achievementStat.fraction;
        completion += 0.2f * data.levelStat.fraction;
        completion += 0.2f * data.recipeStat.fraction;

        return completion * 100f;
    }
    public struct CompletionStat {
        public int count;
        public int max;
        public float fraction;
        public CompletionStat(int count, int max) {
            this.count = count;
            this.max = max;
            this.fraction = (1.0f * count) / max;
        }
    }
    public struct CompletionData {
        public CompletionStat objectStat;
        public CompletionStat commercialStat;
        public CompletionStat achievementStat;
        public CompletionStat levelStat;
        public CompletionStat recipeStat;
        public CompletionData(GameData data) {
            int completeAchievements = 0;
            foreach (Achievement achievement in data.achievements) {
                if (achievement.complete)
                    completeAchievements += 1;
            }
            int completeIngredients = 0;
            foreach (MutablePotionData potionData in data.collectedPotions.Values) {
                if (potionData.unlockedIngredient1) completeIngredients += 1;
                if (potionData.unlockedIngredient2) completeIngredients += 1;
            }
            this.objectStat = new CompletionStat(data.collectedObjects.Count, 153); // note: regenerate these numbers
            this.commercialStat = new CompletionStat(data.completeCommercials.Count, 35);
            this.achievementStat = new CompletionStat(completeAchievements, data.achievements.Count);
            this.levelStat = new CompletionStat(data.unlockedScenes.Count, GameManager.sceneNames.Count);
            this.recipeStat = new CompletionStat(completeIngredients, 13 * 2);
        }
    }
    public void ItemCollectionCallback() {
        startMenu.InspectItemCollection(data);
    }
    public void AchievementButtonCallback() {
        startMenu.InspectAchievements(data);
    }
    public void StatsButtonCallback() {
        startMenu.InspectStats(data);
    }
    public void LoadButtonCallback() {
        GameManager.Instance.LoadGameDataIntoMemory(saveName);
    }
    public void CloseButtonCallback() {
        startMenu.LoadButton();
    }
}
