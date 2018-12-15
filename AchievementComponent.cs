using UnityEngine;
using System.Collections.Generic;
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
    mayorsSassed,
    actsOfCannibalism,
    heartsEaten,
    nullifications
}

[System.Serializable]
public class Stat {
    public StatType type;
    public float value;
    public Stat(){

    }
    public Stat(StatType t){
        this.type = t;
    }
}

[System.Serializable]
public class Achievement {
    public string icon;
    public bool complete;
    public string title;
    public string description;
    public string directive;
    public List<Stat> statList = new List<Stat>();
    public SerializableDictionary<StatType, Stat> statDict {
        get {
            if (_statDict == null){
                _statDict = SetStatDict();
            } 
            return _statDict;
        }
        set {
            _statDict = value;
        }
    }
    private SerializableDictionary<StatType, Stat> _statDict;
    private SerializableDictionary<StatType, Stat> SetStatDict(){
        SerializableDictionary<StatType, Stat> dict = new SerializableDictionary<StatType, Stat>();
        foreach(Stat stat in statList){
            dict[stat.type] = stat;
        }
        return dict;
    }
    public bool Evaluate(Dictionary<StatType, Stat> otherStats) {
        if (statDict.Count == 0){
            return false;
        }
        bool pass = true;
        if (statDict == null){
            statDict = SetStatDict();
        }
        foreach(KeyValuePair<StatType, Stat> kvp in statDict){
            if (!otherStats.ContainsKey(kvp.Key)){
                return false;
            }
            pass = pass && otherStats[kvp.Key].value >= kvp.Value.value;
            if (!pass)
                return false;
        }
        return true;
    }
    public Achievement() { }
    public Achievement(Achievement source) {
        icon = source.icon;
        complete = source.complete;
        title = source.title;
        description = source.description;
        statList = source.statList;
        statDict = source.statDict;
        directive = source.directive;
    }
}
