using UnityEngine;
using System.Collections.Generic;

public class Occurrence : MonoBehaviour {
    public List<OccurrenceData> data = new List<OccurrenceData>();
    public static Dictionary<string, string> KeyDescriptions = new Dictionary<string, string>{
        {"yogurt", "yogurts eaten"},
        {"vomit", "vomit events"},
        {"vomit_eat", "vomit eaten"},
        {"yogurt_vomit", "yogurt emesis event"},
        {"yogurt_vomit_eat", "eating yogurt vomit"},
        {"yogurt_floor", "yogurt eaten off the floor"},
        {"table_fire", "table set on fire"},
        {"eggplant", "eggplants eaten"},
        {"death", "deaths"},
        {"cannibalism", "acts of cannibalism"}
	};
}
/*
        NOTE: IMPORTANT
        If i ever need to serialize child data classes, unity will store them only 
        as parent class, thus losing information. To get around this, I will need 
        to do some extra work with ScriptableObject.
*/
[System.Serializable]
public class EventData {
    public float disturbing;
    public float disgusting;
    public float chaos;
    public float offensive;
    public float positive;
    public string whatHappened;
    public string noun;
    public EventData(){
        // required for serialization
    }
    public EventData(float disturbing=0, float disgusting=0, float chaos=0, float offensive=0, float positive=0){
        this.disturbing = disturbing;
        this.disgusting = disgusting;
        this.chaos = chaos;
        this.offensive = offensive;
        this.positive = positive;
    }
    public bool MatchSpecific(EventData other){
        // TODO: include descriptions?
        // match on descriptions?
        bool match = true;
        match &= disturbing == other.disturbing;
        match &= disgusting == other.disgusting;
        match &= chaos == other.chaos;
        match &= offensive == other.offensive;
        match &= positive == other.positive;
        return match;
    }
}
public class OccurrenceData
{
    public Dictionary<string, float> flags = new Dictionary<string, float>();
    public EventData data = new EventData();
    public virtual void CalculateDescriptions(){
        Debug.Log("base calculatedescriptions was called.");
    }
    public OccurrenceData(){ }
}
public class OccurrenceFire : OccurrenceData {
    public string objectName;
    public bool extinguished;
    public override void CalculateDescriptions(){
        data = new EventData(chaos:100f);
        data.noun = "fire";
        data.whatHappened = "the "+objectName+" burned";

        if (objectName == "table"){
            if (extinguished == false){
                flags["table_fire"] = 1;
            }
        }
    }
}
public class OccurrenceEat : OccurrenceData {
    public float amount;
    public string food;
    public Liquid liquid;
    public bool vomit;
    public bool cannibalism;
    public Edible edible;
    public override void CalculateDescriptions() {
        if (edible.human){
			cannibalism = true;
		}
		if (edible.vomit){
            data.disgusting += 100f;
			data.disturbing += 75f;
			data.chaos += 125f;
		}
        if (edible.offal){
            data.disgusting += 75f;
            data.chaos += 50f;
        }
        if (edible.immoral){
            data.disturbing += 100f;
            data.chaos += 150f;
            data.offensive += 500f;
        }
        if (liquid != null){
            if (liquid.name == "yogurt"){
                flags["yogurt"] = 1f;
                if (liquid.vomit){
                    flags["yogurt_vomit_eat"] = 1f;
                    flags["vomit_eat"] = 1f;
                }
                if (food == "Puddle(Clone)")
                    flags["yogurt_floor"] = 1f;
            }
        } else {
            if (vomit)
                flags["vomit_eat"] = 1f;
        }
        if (Toolbox.Instance.CloneRemover(food) == "eggplant")
            flags["eggplant"] = 1f;
        if (cannibalism){
            flags["cannibalism"] = 1f;
        }
    }
}
public class OccurrenceDeath : OccurrenceData {
    public string nameOfTheDead;
    public override void CalculateDescriptions(){
        flags["death"] = 1f;
        data.noun = "death";
        data.whatHappened = nameOfTheDead + " died";
        data.chaos = 3575f;
		data.disgusting = 2000f;
		data.disturbing = 3500f;
		data.offensive = 8950f;
		data.positive = -5500f;
    }
}
public class OccurrenceVomit : OccurrenceData {
    public string vomit;
    public string vomiter;
    public Liquid liquid;
    public override void CalculateDescriptions(){
        data.noun = "vomiting";
        data.whatHappened = vomiter + " vomited up (name)";

        flags["vomit"] = 1f;
        if (liquid != null){
            if (liquid.name == "yogurt")
                flags["yogurt_vomit"] = 1f;
        } 
    }
}
public class OccurrenceSpeech : OccurrenceData {
    // TODO: include disturbingness for swearing;
    public string line;
    public string speaker;
    public string target;
    public bool threat;
    public bool insult;
    public override void CalculateDescriptions(){
        data.whatHappened = speaker + " said " + line;
        data.noun = "speech act";
        // insert bits here for script desc, transcript line
        if (threat){
            data.whatHappened = speaker + " threatened " + target;
        }
        if (insult){
            data.whatHappened = speaker + " insulted " + target;
        }
    }
}
public class OccurrenceViolence : OccurrenceData {
    public GameObject attacker;
    public GameObject victim;
    public string attackerName;
    public string victimName;
    public override void CalculateDescriptions(){
        EventData data = new EventData(disturbing:10f, chaos:10f);
        data.noun = "violence";
        data.whatHappened = attackerName + " attacked " + victimName;
    }
}
