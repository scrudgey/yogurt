using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StatsBrowser : MonoBehaviour {
    public Dictionary<StatType, string> statDescriptions = new Dictionary<StatType, string>(){
        {StatType.secondsPlayed, "seconds played"},
        {StatType.yogurtEaten, "yogurts eaten"},
        {StatType.vomit, "number of times vomited"},
        {StatType.yogurtVomit, "number of times vomited yogurt"},
        {StatType.dollarsFlushed, "dollars flushed down the toilet"},
        {StatType.dollarsDuplicated, "dollars duplicated"},
        {StatType.dollarsBurned, "dollars burned"},
        {StatType.swordsEaten, "swords eaten"},
        {StatType.hatsEaten, "hats eaten"},
        {StatType.immolations, "times immolated"},
        {StatType.selfImmolations, "self immolations"},
        {StatType.deathByCombat, "death by combat"},
        {StatType.deathByMisadventure, "death by misadventure"},
        {StatType.deathByAsphyxiation, "death by asphyxiation"},
        {StatType.mayorsSassed, "mayors sassed"},
        {StatType.actsOfCannibalism, "acts of cannibalism"},
        {StatType.heartsEaten, "hearts eaten"},
        {StatType.nullifications, "self nullifications"},
        {StatType.othersSetOnFire, "others set on fire"},
        {StatType.monstersKilled, "monsters killed"},
        {StatType.murders, "murders"},
        {StatType.deathByExplosion, "death by explosion"},
        {StatType.bedsMade, "beds made"},
        {StatType.booksBurned, "books burned"},
    };
    public Text title;
    public Text content;
    public Text count;
    public StartMenu startMenu;
    public void Initialize(GameData data) {
        content.text = "";
        count.text = "";
        foreach (KeyValuePair<StatType, string> kvp in statDescriptions) {
            content.text = content.text + kvp.Value.ToString() + "\n";
            if (data.stats.ContainsKey(kvp.Key)) {
                int value = (int)data.stats[kvp.Key].value;
                count.text = count.text + value.ToString() + "\n";
            } else {
                count.text = count.text + "0\n";
            }
        }
    }
    public void CloseButtonCallback() {
        startMenu.CloseStatsBrowser();
    }
}
