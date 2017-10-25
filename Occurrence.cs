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
    // public void AddData(EventData other){
    //     disturbing += other.disturbing;
    //     disgusting += other.disgusting;
    //     chaos += other.chaos;
    //     offensive += other.offensive;
    //     positive += other.positive;
    // }
}
public interface OccurrenceData
{
    Dictionary<string, float> Events();
    EventData Data();
}
public class OccurrenceMisc : OccurrenceData {
    // TODO: include disturbingness for swearing;
    public float disturbing;
    public float disgusting;
    public float chaos;
    public float offensive;
    public float positive;
    public string noun="";
    public string whatHappened="";
    public OccurrenceMisc(string noun="event", string whatHappened="a thing happened", float disturbing=0, float disgusting=0, float chaos=0, float offensive=0, float positive=0){
        this.disturbing = disturbing;
        this.disgusting = disgusting;
        this.chaos = chaos;
        this.offensive = offensive;
        this.positive = positive;
        this.noun = noun;
        this.whatHappened = whatHappened;
    }
    public virtual EventData Data(){
        EventData data = new EventData();
        data.whatHappened = whatHappened;
        data.noun = noun;
        data.disturbing = disturbing;
        data.disgusting = disgusting;
        data.chaos = chaos;
        data.offensive = offensive;
        data.positive = positive;
        return data;
    }
    public virtual Dictionary<string, float> Events(){
        return new Dictionary<string, float>();
    }
}


[System.Serializable]
public class OccurrenceFire : OccurrenceData {
    public string objectName;
    public bool extinguished;
    public Dictionary<string, float> Events(){
        // TODO: make this binary again
        Dictionary<string, float> events = new Dictionary<string, float>();
        if (objectName == "table"){
            if (extinguished == false){
                events["table_fire"] = 1;
            }
        }
        return events;
    }
    public EventData Data(){
        EventData data = new EventData(chaos:100f);
        data.noun = "fire";
        data.whatHappened = "the "+objectName+" burned";
        return data;
    }
}
[System.Serializable]
public class OccurrenceEat : OccurrenceMisc {
    public float amount;
    public string food;
    public Liquid liquid;
    public bool vomit;
    public bool cannibalism;
    // EventData dat = new EventData();
    public override Dictionary<string, float> Events(){
        Dictionary<string, float> events = new Dictionary<string, float>();
        if (liquid != null){
            if (liquid.name == "yogurt"){
                events["yogurt"] = 1f;
                if (liquid.vomit){
                    events["yogurt_vomit_eat"] = 1f;
                    events["vomit_eat"] = 1f;
                }
                if (food == "Puddle(Clone)")
                    events["yogurt_floor"] = 1f;
            }
        } else {
            if (vomit)
                events["vomit_eat"] = 1f;
        }
        if (Toolbox.Instance.CloneRemover(food) == "eggplant")
            events["eggplant"] = 1f;
        if (cannibalism){
            events["cannibalism"] = 1f;
        }
        return events;
    }
}
public class OccurrenceDeath : OccurrenceData {
    public string nameOfTheDead;
    public Dictionary<string, float> Events(){
        Dictionary<string, float> events = new Dictionary<string, float>();
        events["death"] = 1f;
        return new Dictionary<string, float>{{"death", 1f}};
    }
    public EventData Data(){
        EventData data = new EventData(chaos:3575f, disgusting:2000f, disturbing:3500f, offensive:8950f, positive:-5500f);
        data.noun = "death";
        data.whatHappened = nameOfTheDead + " died";
        return data;
    }
}
[System.Serializable]
public class OccurrenceVomit : OccurrenceData {
    public string vomit;
    public string vomiter;
    public Liquid liquid;
    public Dictionary<string, float> Events(){
        Dictionary<string, float> events = new Dictionary<string, float>();
        events["vomit"] = 1f;
        if (liquid != null){
            if (liquid.name == "yogurt")
                events["yogurt_vomit"] = 1f;
        } 
        return events;
    }
    public EventData Data(){
        EventData data = new EventData(disgusting:350f);
        data.noun = "vomiting";
        data.whatHappened = vomiter + " vomited up (name)";
        return data;
    }
}

[System.Serializable]
public class OccurrenceSpeech : OccurrenceMisc {
    // TODO: include disturbingness for swearing;
    public string line;
    public string speaker;
    public bool threat;
    public bool insult;
    public string target;
    public OccurrenceSpeech(string noun="speech", string whatHappened="someone said something", float disturbing=0, float disgusting=0, float chaos=0, float offensive=0, float positive=0):
        base(noun:noun, whatHappened:whatHappened, disturbing:disturbing, disgusting:disgusting, chaos:chaos, offensive:offensive, positive:positive){}
    public override EventData Data(){
        EventData data = base.Data();
        data.whatHappened = speaker + " said " + line;
        data.noun = "speech act";
        if (threat){
            data.chaos += 15;
            data.offensive += 10;
            data.positive -= 20;
            data.offensive = Random.Range(20, 30);
            data.whatHappened = speaker + " threatened " + target;
        }
        if (insult){
            data.chaos += 10;
            data.offensive += 20;
            data.positive -= 20;
            data.offensive = Random.Range(20, 30);
            data.whatHappened = speaker + " insulted " + target;
        }
        return data;
    }
}

[System.Serializable]
public class OccurrenceViolence : OccurrenceData {
    public GameObject attacker;
    public GameObject victim;
    public string attackerName;
    public string victimName;
    public Dictionary<string, float> Events(){
        return new Dictionary<string, float>();
    }
    public EventData Data(){
        EventData data = new EventData(disturbing:10f, chaos:10f);
        // violence.disturbing = 10f;
		// violence.chaos = 10f;
        data.noun = "violence";
        data.whatHappened = attackerName + " attacked " + victimName;
        return data;
    }
}
