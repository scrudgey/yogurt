using Nimrod;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class EventData {
    public string id = System.Guid.Empty.ToString();
    public string key;
    public float val;
    public string popupDesc;
    public string whatHappened;
    public string transcriptLine;
    public string noun;
    public SerializableDictionary<Rating, float> ratings = new SerializableDictionary<Rating, float>(){
        {Rating.disgusting, 0f},
        {Rating.disturbing, 0f},
        {Rating.chaos, 0f},
        {Rating.offensive, 0f},
        {Rating.positive, 0f}
    };
    public EventData() {
    }
    public EventData(EventData other) {
        this.key = other.key;
        this.val = other.val;
        this.popupDesc = other.popupDesc;
        this.whatHappened = other.whatHappened;
        this.transcriptLine = other.transcriptLine;
        this.noun = other.noun;
        this.ratings = new SerializableDictionary<Rating, float>();
        foreach (KeyValuePair<Rating, float> kvp in other.ratings) {
            this.ratings[kvp.Key] = kvp.Value;
        }
    }
    public EventData(float disturbing = 0, float disgusting = 0, float chaos = 0, float offensive = 0, float positive = 0) {
        ratings[Rating.disturbing] = disturbing;
        ratings[Rating.disgusting] = disgusting;
        ratings[Rating.chaos] = chaos;
        ratings[Rating.offensive] = offensive;
        ratings[Rating.positive] = positive;
    }
    public bool MatchSpecific(EventData other) {
        // TODO: include descriptions?
        // match on descriptions?
        bool match = true;
        foreach (Rating key in ratings.Keys) {
            match &= ratings[key] == other.ratings[key];
        }
        return match;
    }
    public Rating Quality() {
        Dictionary<Rating, float> absRates = new Dictionary<Rating, float>();
        foreach (Rating key in ratings.Keys) {
            absRates[key] = Mathf.Abs(ratings[key]);
        }
        return absRates.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
    }
    public string Adjective() {
        Grammar grammar = new Grammar();
        grammar.Load("ratings");
        Rating rate = Quality();
        if (ratings[rate] == 0)
            return "none";
        float severity = Mathf.Min(ratings[rate], 3);
        string key = rate.ToString() + "_" + severity.ToString();
        return grammar.Parse("{" + key + "}");
    }
    public static EventData Yogurt(GameObject eater) {
        EventData data = new EventData(positive: 1);
        data.key = "yogurt";
        data.val = 1f;
        data.popupDesc = "yogurts eaten";
        data.noun = "yogurt eating";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate yogurt";
        return data;
    }
    public static EventData Gravy(GameObject eater) {
        EventData data = new EventData(positive: 1);
        data.key = "gravy";
        data.val = 1f;
        data.popupDesc = "gravy eaten";
        data.noun = "gravy eating";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate gravy";
        return data;
    }
    public static EventData Vomit(GameObject vomiter, GameObject vomited) {
        EventData data = new EventData(disturbing: 1, disgusting: 2, chaos: 1);
        data.key = "vomit";
        data.val = 1f;
        data.popupDesc = "vomit events";
        data.noun = "vomiting";
        data.whatHappened = Toolbox.Instance.GetName(vomiter) + " vomited up " + Toolbox.Instance.GetName(vomited);
        return data;
    }
    public static EventData VomitYogurt(GameObject vomiter) {
        EventData data = new EventData(disturbing: 1, disgusting: 2, chaos: 2, positive: -2);
        data.key = "yogurt_vomit";
        data.val = 1f;
        data.popupDesc = "yogurt emesis event";
        data.noun = "vomiting yogurt";
        data.whatHappened = Toolbox.Instance.GetName(vomiter) + " vomited up yogurt";
        return data;
    }
    public static EventData VomitEat(GameObject eater) {
        EventData data = new EventData(disturbing: 2, disgusting: 2, chaos: 2);
        data.key = "vomit_eat";
        data.val = 1f;
        data.popupDesc = "vomit eaten";
        data.noun = "eating vomit";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate vomit";
        return data;
    }
    public static EventData YogurtVomitEat(GameObject eater) {
        EventData data = new EventData(disturbing: 2, disgusting: 2, chaos: 2, positive: -1);
        data.key = "yogurt_vomit_eat";
        data.val = 1f;
        data.popupDesc = "eating yogurt vomit";
        data.noun = "eating vomited-up yogurt";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate vomited-up yogurt";
        return data;
    }
    public static EventData YogurtFloor(GameObject eater) {
        EventData data = new EventData(disgusting: 1, chaos: 1);
        data.key = "yogurt_floor";
        data.val = 1f;
        data.popupDesc = "yogurt eaten off the floor";
        data.noun = "eating of yogurt on the floor";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate yogurt off the floor";
        return data;
    }
    public static EventData TableFire() {
        EventData data = new EventData(disturbing: 1, chaos: 2);
        data.key = "table_fire";
        data.val = 1f;
        data.popupDesc = "table set on fire";
        data.noun = "setting fire to the table";
        data.whatHappened = "the table was set on fire";
        return data;
    }
    public static EventData Cannibalism(GameObject cannibal) {
        EventData data = new EventData(offensive: 4, disgusting: 3, disturbing: 3, chaos: 3, positive: -3);
        data.key = "cannibalism";
        data.val = 1f;
        data.popupDesc = "acts of cannibalism";
        data.noun = "cannibalism";
        data.whatHappened = Toolbox.Instance.GetName(cannibal) + " commited cannibalism";
        return data;
    }
    public static EventData Death(
        GameObject dead,
        GameObject lastAttacker,
        damageType lastDamage,
        bool monster,
        bool suicide,
        bool assailant
        ) {
        EventData data = null;
        string victimName = Toolbox.Instance.GetName(dead);
        if (monster) {
            data = new EventData(offensive: 1, disgusting: 1, disturbing: 0, chaos: 0, positive: 2);
            data.key = "killing";
            data.val = 1f;
            data.popupDesc = "deaths";
            data.noun = "death";
            data.whatHappened = Toolbox.Instance.GetName(dead) + " was killed";
            GameManager.Instance.IncrementStat(StatType.monstersKilled, 1);
        } else if (suicide) {
            data.whatHappened = victimName + " committed suicide";
            data.noun = "suicide";
            data.popupDesc = "suicides";
            if (lastDamage == damageType.fire) {
                data.whatHappened = victimName + " self-immolated";
                GameManager.Instance.IncrementStat(StatType.immolations, 1);
                GameManager.Instance.IncrementStat(StatType.selfImmolations, 1);
            }
        } else {

            data = new EventData(offensive: 4, disgusting: 3, disturbing: 4, chaos: 4, positive: -3);
            data.key = "death";
            data.val = 1f;
            if (assailant) {
                GameManager.Instance.IncrementStat(StatType.murders, 1);
                string attackerName = Toolbox.Instance.GetName(lastAttacker);
                data.whatHappened = attackerName + " murdered " + victimName;
                data.noun = "murder";
                data.popupDesc = "murders";
                if (lastDamage == damageType.fire) {
                    data.whatHappened += " with fire";
                } else if (lastDamage == damageType.asphyxiation) {
                    data.whatHappened = attackerName + " strangled " + victimName + " to death";
                } else if (lastDamage == damageType.cosmic) {
                    data.whatHappened = attackerName + " annihilated " + victimName + " with cosmic energy";
                }
            } else {
                data.noun = "death";
                data.popupDesc = "deaths";
                if (lastDamage == damageType.fire) {
                    data.whatHappened = victimName + " burned to death";
                    GameManager.Instance.IncrementStat(StatType.immolations, 1);
                } else if (lastDamage == damageType.asphyxiation) {
                    data.whatHappened = victimName + " asphyxiated";
                } else if (lastDamage == damageType.cosmic) {
                    data.whatHappened = victimName + " was annihilated by cosmic energy";
                } else if (lastDamage == damageType.cutting || lastDamage == damageType.piercing) {
                    data.whatHappened = victimName + " was stabbed to death";
                }
            }
        }
        return data;
    }
    public static EventData Eggplant(GameObject eater) {
        EventData data = new EventData(positive: 1);
        data.key = "eggplant";
        data.val = 1f;
        data.popupDesc = "eggplants eaten";
        data.noun = "eggplant eating";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate an eggplant";
        return data;
    }
    public static EventData EldritchHorror() {
        EventData data = new EventData(disturbing: 3, disgusting: 1, chaos: 1, offensive: 2, positive: -2);
        data.key = "horror";
        data.val = 1f;
        data.popupDesc = "horrors";
        data.noun = "horrors";

        Grammar grammar = new Grammar();
        grammar.Load("horror");
        data.whatHappened = grammar.Parse("{main}");
        return data;
    }
}