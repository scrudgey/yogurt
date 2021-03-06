﻿using UnityEngine;
using System.Collections.Generic;
using System;
public class AchievementComponent : MonoBehaviour {
    public Achievement achivement;
}
public enum StatType {
    secondsPlayed,
    yogurtEaten,
    vomit,
    yogurtVomit,
    dollarsFlushed,
    dollarsDuplicated,
    dollarsBurned,
    swordsEaten,
    hatsEaten,
    immolations,
    selfImmolations,
    deathByCombat,
    deathByMisadventure,
    deathByAsphyxiation,
    deathByExplosion,
    deathByAcid,
    deathByExplodingHead,
    deathByPotion,
    deathByFire,
    mayorsSassed,
    actsOfCannibalism,
    heartsEaten,
    nullifications,
    othersSetOnFire,
    monstersKilled,
    murders,
    bedsMade,
    booksBurned,
    typesOfWaterCollected,
    headsExploded
}

[System.Serializable]
public class Stat {
    public StatType type;
    public float value;
    public Stat() {

    }
    public Stat(StatType t) {
        this.type = t;
    }
}

[System.Serializable]
public class Achievement {
    public string steamId;
    public bool steamAchieved;
    public string icon;
    public bool complete;
    public System.DateTime completedTime;
    [TextArea(3, 10)]
    public string title;
    [TextArea(3, 10)]
    public string description;
    [TextArea(3, 10)]
    public string directive;
    public List<Stat> statList = new List<Stat>();
    public SerializableDictionary<StatType, Stat> statDict {
        get {
            if (_statDict == null) {
                _statDict = SetStatDict();
            }
            return _statDict;
        }
        set {
            _statDict = value;
        }
    }
    private SerializableDictionary<StatType, Stat> _statDict;
    private SerializableDictionary<StatType, Stat> SetStatDict() {
        SerializableDictionary<StatType, Stat> dict = new SerializableDictionary<StatType, Stat>();
        foreach (Stat stat in statList) {
            dict[stat.type] = stat;
        }
        return dict;
    }
    public bool Evaluate(Dictionary<StatType, Stat> otherStats) {
        if (statDict.Count == 0) {
            return false;
        }
        bool pass = true;
        if (statDict == null) {
            statDict = SetStatDict();
        }
        foreach (KeyValuePair<StatType, Stat> kvp in statDict) {
            if (!otherStats.ContainsKey(kvp.Key)) {
                return false;
            }
            pass = pass && otherStats[kvp.Key].value >= kvp.Value.value;
            if (!pass)
                return false;
        }
        return true;
    }
    public Achievement() { }
    public Achievement(Achievement source) { // deepcopy
        icon = source.icon;
        complete = source.complete;
        title = source.title;
        description = source.description;
        statList = source.statList;
        statDict = source.statDict;
        directive = source.directive;
        steamId = source.steamId;

    }
}
