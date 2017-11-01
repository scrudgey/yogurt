using UnityEngine;
using System.Collections.Generic;
//TODO: not all of these things need static constructors?

public class Occurrence : MonoBehaviour {
    // An occurrence is a little bit of code that lives on a temporarily persistent flag in the world
    // that knows how to describe an event in terms of EventData. 
    // occurrences can also be noticed by perceptive components which use the flag properties to compose 
    // a stimulus.
    public List<OccurrenceData> data = new List<OccurrenceData>();
    public static EventData Yogurt(GameObject eater){
        EventData data = new EventData(positive:5f);
        data.key = "yogurt";
        data.val = 1f;
        data.desc = "yogurts eaten";
        data.noun = "yogurt eating";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate yogurt";
        return data;
    }
    public static EventData Vomit(GameObject vomiter, GameObject vomited){
        EventData data = new EventData(disturbing:5f, disgusting:30f, chaos:40f);
        data.key = "vomit";
        data.val = 1f;
        data.desc = "vomit events";
        data.noun = "vomiting";
        data.whatHappened = Toolbox.Instance.GetName(vomiter) + " vomited up "+Toolbox.Instance.GetName(vomited);
        return data;
    }
    public static EventData VomitYogurt(GameObject vomiter){
        EventData data = new EventData(disturbing:5f, disgusting:30f, chaos:40f);
        data.key = "yogurt_vomit";
        data.val = 1f;
        data.desc = "yogurt emesis event";
        data.noun = "vomiting yogurt";
        data.whatHappened = Toolbox.Instance.GetName(vomiter) + " vomited up yogurt";
        return data;
    }
    public static EventData VomitEat(GameObject eater){
        EventData data = new EventData(disturbing:15f, disgusting:50f, chaos:40f);
        data.key = "vomit_eat";
        data.val = 1f;
        data.desc = "vomit eaten";
        data.noun = "eating vomit";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate vomit";
        return data;
    }
    public static EventData YogurtVomitEat(GameObject eater){
        EventData data = new EventData(disturbing:10f, disgusting:75f, chaos:50f);
        data.key = "yogurt_vomit_eat";
        data.val = 1f;
        data.desc = "eating yogurt vomit";
        data.noun = "eating vomited-up yogurt";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate vomited-up yogurt";
        return data;
    }
    public static EventData YogurtFloor(GameObject eater){
        EventData data = new EventData(disgusting:35f, chaos:10f);
        data.key = "yogurt_floor";
        data.val = 1f;
        data.desc = "yogurt eaten off the floor";
        data.noun = "eating of yogurt on the floor";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate yogurt off the floor";
        return data;
    }
    public static EventData TableFire(){
        EventData data = new EventData(disturbing:10f, chaos:10f);
        data.key = "table_fire";
        data.val = 1f;
        data.desc = "table set on fire";
        data.noun = "setting fire to the table";
        data.whatHappened = "the table was set on fire";
        return data;
    }
    public static EventData Cannibalism(GameObject cannibal){
        EventData data = new EventData(offensive:250f, disgusting:500f, disturbing:350f, chaos:150f, positive:-300f);
        data.key = "cannibalism";
        data.val = 1f;
        data.desc = "acts of cannibalism";
        data.noun = "cannibalism";
        data.whatHappened = Toolbox.Instance.GetName(cannibal) + " commited cannibalism";
        return data;
    }
    public static EventData Death(GameObject dead){
        EventData data = new EventData(offensive:50f, disgusting:130f, disturbing:200f, chaos:525f, positive:-200f);
        data.key = "death";
        data.val = 1f;
        data.desc = "deaths";
        data.noun = "death";
        data.whatHappened = Toolbox.Instance.GetName(dead) + " died";
        return data;
    }
    public static EventData Eggplant(GameObject eater){
        EventData data = new EventData(positive:1f);
        data.key = "eggplant";
        data.val = 1f;
        data.desc = "eggplants eaten";
        data.noun = "eggplant eating";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate an eggplant";
        return data;
    }
}

[System.Serializable]
public class EventData {
    public SerializableDictionary<string, float> flags = new SerializableDictionary<string, float>();
    public string key;
    public float val;
    public string desc;
    public string whatHappened;
    public string transcriptLine;
    public string noun;
    public float disturbing;
    public float disgusting;
    public float chaos;
    public float offensive;
    public float positive;
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
    public List<EventData> events = new List<EventData>();
    public virtual void CalculateDescriptions(){
        Debug.Log("base calculatedescriptions was called.");
    }
    public OccurrenceData(){ }
}
public class OccurrenceFire : OccurrenceData {
    public GameObject flamingObject;
    public bool extinguished;
    public override void CalculateDescriptions(){
        EventData data = new EventData(chaos:100f);
        string objectName = Toolbox.Instance.GetName(flamingObject);
        data.noun = "fire";
        data.whatHappened = "the "+objectName+" burned";
        events.Add(data);
        if (objectName == "table"){
            if (extinguished == false){
                events.Add(Occurrence.TableFire());
            }
        }
    }
}
public class OccurrenceEat : OccurrenceData {
    public Liquid liquid;
    public Edible edible;
    public GameObject eater;
    public override void CalculateDescriptions() {
        EventData data = new EventData();
        data.noun = "eating";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate "+edible.name;
        if (edible.offal){
            data.disgusting += 45f;
            data.chaos += 50f;
        }
        if (edible.immoral){
            data.disturbing += 100f;
            data.chaos += 150f;
            data.offensive += 500f;
        }
        events.Add(data);
        if (edible.vomit){
            events.Add(Occurrence.VomitEat(eater));
		}
        if (liquid != null){
            if (liquid.name == "yogurt"){
                events.Add(Occurrence.Yogurt(eater));
                if (liquid.vomit){
                    events.Add(Occurrence.YogurtVomitEat(eater));
                }
                if (edible.gameObject.name == "Puddle(Clone)"){
                    events.Add(Occurrence.YogurtFloor(eater));
                }
            }
        } 
        if (Toolbox.Instance.GetName(edible.gameObject) == "eggplant"){
            events.Add(Occurrence.Eggplant(eater));
        }
        if (edible.human){
            events.Add(Occurrence.Cannibalism(eater));
		}
    }
}
public class OccurrenceDeath : OccurrenceData {
    public GameObject dead;
    public override void CalculateDescriptions(){
        events.Add(Occurrence.Death(dead));
    }
}
public class OccurrenceVomit : OccurrenceData {
    public GameObject vomiter;
    public GameObject vomit;
    public override void CalculateDescriptions(){
        events.Add(Occurrence.Vomit(vomiter, vomit));
        MonoLiquid mliquid = vomit.GetComponent<MonoLiquid>();
        if (mliquid.liquid != null){
            if (mliquid.liquid.name == "yogurt")
                events.Add(Occurrence.VomitYogurt(vomiter));
        } 
    }
}
public class OccurrenceSpeech : OccurrenceData {
    // TODO: include disturbingness for swearing;
    public GameObject speaker;
    public GameObject target;
    public string line;
    public bool threat;
    public bool insult;
    public override void CalculateDescriptions(){
        string speakerName = Toolbox.Instance.GetName(speaker);
        string targetName = "";
        if (target != null)
            targetName = Toolbox.Instance.GetName(target);
        EventData data = null;
        if (events.Count > 0){
            data = events[0];
        } else {
            data = new EventData();
        }
        data.whatHappened = speakerName + " said " + line;
        data.noun = "dialogue";
        data.transcriptLine = speakerName + ": "+line;
        // insert bits here for script desc, transcript line
        if (threat){
            data.whatHappened = speakerName + " threatened " + targetName;
            data.noun = "threats";
            data.chaos += 15f;
            data.offensive = Random.Range(20, 30);
            data.disturbing = 10f;
        }
        if (insult){
            data.whatHappened = speakerName + " insulted " + targetName;
            data.noun = "insults";
            data.chaos += 15f;
            data.offensive = Random.Range(20, 30);
            data.disgusting = 10f;
        }
        if (events.Count == 0){
            events.Add(data);
        }
    }
}
public class OccurrenceViolence : OccurrenceData {
    public GameObject attacker;
    public GameObject victim;
    public override void CalculateDescriptions(){
        string attackerName = Toolbox.Instance.GetName(attacker);
        string victimName = Toolbox.Instance.GetName(victim);

        EventData data = new EventData(disturbing:10f, chaos:10f);
        data.noun = "violence";
        data.whatHappened = attackerName + " attacked " + victimName;
        events.Add(data);
    }
}
