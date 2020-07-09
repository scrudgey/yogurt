using Nimrod;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using analysis;

[System.Serializable]
public class EventData : Describable {
    public string key;
    public float val;
    public string popupDesc;
    public string transcriptLine;
    public string noun;
    public EventData() { } // needed for serialization, should not be used
    public EventData(string noun, string whatHappened) {
        this.noun = noun;
        this.whatHappened = whatHappened;
        quality = new SerializableDictionary<Rating, float>(){
            {Rating.disgusting, 0f},
            {Rating.disturbing, 0f},
            {Rating.chaos, 0f},
            {Rating.offensive, 0f},
            {Rating.positive, 0f}
        };
    }
    public EventData(EventData other) {
        this.quality = new SerializableDictionary<Rating, float>(){
            {Rating.disgusting, 0f},
            {Rating.disturbing, 0f},
            {Rating.chaos, 0f},
            {Rating.offensive, 0f},
            {Rating.positive, 0f}
        };
        this.key = other.key;
        this.val = other.val;
        this.popupDesc = other.popupDesc;
        this.whatHappened = other.whatHappened;
        this.transcriptLine = other.transcriptLine;
        this.noun = other.noun;
        foreach (KeyValuePair<Rating, float> kvp in other.quality) {
            this.quality[kvp.Key] = kvp.Value;
        }
    }
    override public string ToString() {
        return noun + " " + key + " " + val.ToString() + " " + popupDesc + " " + whatHappened + " " + transcriptLine + " ";
    }
    public EventData(float disturbing = 0, float disgusting = 0, float chaos = 0, float offensive = 0, float positive = 0) {
        quality[Rating.disturbing] = disturbing;
        quality[Rating.disgusting] = disgusting;
        quality[Rating.chaos] = chaos;
        quality[Rating.offensive] = offensive;
        quality[Rating.positive] = positive;
    }
    public bool MatchSpecific(EventData other) {
        // TODO: include descriptions?
        // match on descriptions?
        bool match = true;
        foreach (Rating key in quality.Keys) {
            match &= quality[key] == other.quality[key];
        }
        return match;
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
        string vomitedName = Toolbox.Instance.GetName(vomited);
        string vomiterName = Toolbox.Instance.GetName(vomiter);
        if (vomitedName != "") {
            data.whatHappened = $"{vomiterName} vomited up {vomitedName}";
        } else {
            data.whatHappened = $"{vomiterName} vomited!";
        }
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
        EventData data = new EventData(disgusting: 2, chaos: 2);
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
            data = new EventData(offensive: 3, disgusting: 3, disturbing: 3, chaos: 2, positive: -3);
            data.whatHappened = victimName + " committed suicide";
            data.noun = "suicide";
            data.popupDesc = "suicides";
            if (lastDamage == damageType.fire) {
                data.whatHappened = victimName + " self-immolated";
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
                } else if (lastDamage == damageType.explosion) {
                    data.whatHappened = attackerName + " vaporized " + victimName + " in an explosion";
                } else if (lastDamage == damageType.acid) {
                    data.whatHappened = $"{attackerName} dissolved {victimName} in acid";
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
                } else if (lastDamage == damageType.explosion) {
                    data.whatHappened = victimName + " exploded into bloody chunks";
                } else if (lastDamage == damageType.acid) {
                    data.whatHappened = $"{victimName} was dissolved in acid";
                }
            }
        }
        return data;
    }
    public static EventData Destruction(
        GameObject destroyed,
        GameObject lastAttacker,
        MessageDamage lastMessage) {
        string victimName = Toolbox.Instance.GetName(destroyed);
        EventData data = new EventData(offensive: 1, disgusting: 1, disturbing: 1, chaos: 2, positive: -1);
        data.key = "destruction";
        data.val = 1f;
        data.noun = "destruction";
        data.popupDesc = "objects destroyed";

        if (Toolbox.Instance.CloneRemover(destroyed.name) == "book" && lastMessage.type == damageType.fire) {
            data = new EventData(offensive: 2, disgusting: 0, disturbing: 1, chaos: 2, positive: -2);
            data.noun = "book burning";
            data.key = "book burning";
            data.val = 1f;
            data.popupDesc = "book burnings";
        }

        data.whatHappened = "the " + victimName + " was destroyed";
        if (lastMessage.type == damageType.fire) {
            data.whatHappened = "the " + victimName + " was incinerated";
        } else if (lastMessage.type == damageType.asphyxiation) {
            data.whatHappened = "the " + victimName + " broke due to lack of oxygen";
        } else if (lastMessage.type == damageType.cosmic) {
            data.whatHappened = "the " + victimName + " was annihilated by cosmic energy";
        } else if (lastMessage.type == damageType.cutting || lastMessage.type == damageType.piercing) {
            data.whatHappened = "the " + victimName + " was chopped into pieces";
        } else if (lastMessage.type == damageType.explosion) {
            data.whatHappened = "the " + victimName + " was destroyed in an explosion";
        } else if (lastMessage.type == damageType.acid) {
            data.whatHappened = $"the {victimName} dissolved in acid";
        }

        if (!lastMessage.impersonal && lastAttacker != null) {
            string attackerName = Toolbox.Instance.GetName(lastAttacker);
            data.whatHappened = attackerName + " destroyed the " + victimName;
            if (lastMessage.type == damageType.fire) {
                data.whatHappened = attackerName + " incinerated the " + victimName;
            } else if (lastMessage.type == damageType.cosmic) {
                data.whatHappened = attackerName + " annihilated the " + victimName + " with cosmic energy";
            } else if (lastMessage.type == damageType.cutting || lastMessage.type == damageType.piercing) {
                data.whatHappened = attackerName + " chopped the " + victimName + " into little pieces";
            } else if (lastMessage.type == damageType.explosion) {
                data.whatHappened = attackerName + " exploded the " + victimName;
            } else if (lastMessage.type == damageType.acid) {
                data.whatHappened = $"{attackerName} dissolved the {victimName} in acid";
            }
        }
        return data;
    }
    public static EventData Nullification(GameObject nullified) {
        string victimName = Toolbox.Instance.GetName(nullified);
        EventData data = new EventData(offensive: 1, disgusting: 0, disturbing: 2, chaos: 0, positive: 0);
        data.key = "nullification";
        data.val = 1f;
        data.noun = "nullification";
        data.popupDesc = "objects nullified";

        data.whatHappened = $"the {victimName} was vaporized";
        if (victimName.ToLower().Contains("effigy")) {
            data = new EventData(offensive: 1, disgusting: 0, disturbing: 2, chaos: 0, positive: 0);
            data.noun = "effigy nullification";
            data.key = "nullify_hatred";
            data.val = 1f;
            data.popupDesc = "effigy nullifications";
            data.whatHappened = $"an offensive effigy was vaporized";
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
    public static EventData Explosion(GameObject explosive) {
        EventData data = new EventData(disturbing: 0, disgusting: 0, chaos: 3, offensive: 0, positive: 1);
        data.key = "explosions";
        data.val = 1f;
        data.popupDesc = "explosions";
        data.noun = "explosion";
        data.whatHappened = "the " + Toolbox.Instance.GetName(explosive) + " exploded";
        return data;
    }

}