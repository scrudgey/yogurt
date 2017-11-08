using Nimrod;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
//TODO: not all of these things need static constructors?
public enum Rating{disturbing, disgusting, chaos, offensive, positive};
public class Occurrence : MonoBehaviour {
    // An occurrence is a little bit of code that lives on a temporarily persistent flag in the world
    // that knows how to describe an event in terms of EventData. 
    // occurrences can also be noticed by perceptive components which use the flag properties to compose 
    // a stimulus.
    public List<OccurrenceData> data = new List<OccurrenceData>();
    public static EventData Yogurt(GameObject eater){
        EventData data = new EventData(positive:1);
        data.key = "yogurt";
        data.val = 1f;
        data.desc = "yogurts eaten";
        data.noun = "yogurt eating";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate yogurt";
        return data;
    }
    public static EventData Vomit(GameObject vomiter, GameObject vomited){
        EventData data = new EventData(disturbing:1, disgusting:2, chaos:1);
        data.key = "vomit";
        data.val = 1f;
        data.desc = "vomit events";
        data.noun = "vomiting";
        data.whatHappened = Toolbox.Instance.GetName(vomiter) + " vomited up "+Toolbox.Instance.GetName(vomited);
        return data;
    }
    public static EventData VomitYogurt(GameObject vomiter){
        EventData data = new EventData(disturbing:1, disgusting:2, chaos:2, positive:-2);
        data.key = "yogurt_vomit";
        data.val = 1f;
        data.desc = "yogurt emesis event";
        data.noun = "vomiting yogurt";
        data.whatHappened = Toolbox.Instance.GetName(vomiter) + " vomited up yogurt";
        return data;
    }
    public static EventData VomitEat(GameObject eater){
        EventData data = new EventData(disturbing:2, disgusting:2, chaos:2);
        data.key = "vomit_eat";
        data.val = 1f;
        data.desc = "vomit eaten";
        data.noun = "eating vomit";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate vomit";
        return data;
    }
    public static EventData YogurtVomitEat(GameObject eater){
        EventData data = new EventData(disturbing:2, disgusting:2, chaos:2, positive:-1);
        data.key = "yogurt_vomit_eat";
        data.val = 1f;
        data.desc = "eating yogurt vomit";
        data.noun = "eating vomited-up yogurt";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate vomited-up yogurt";
        return data;
    }
    public static EventData YogurtFloor(GameObject eater){
        EventData data = new EventData(disgusting:1, chaos:1);
        data.key = "yogurt_floor";
        data.val = 1f;
        data.desc = "yogurt eaten off the floor";
        data.noun = "eating of yogurt on the floor";
        data.whatHappened = Toolbox.Instance.GetName(eater) + " ate yogurt off the floor";
        return data;
    }
    public static EventData TableFire(){
        EventData data = new EventData(disturbing:1, chaos:2);
        data.key = "table_fire";
        data.val = 1f;
        data.desc = "table set on fire";
        data.noun = "setting fire to the table";
        data.whatHappened = "the table was set on fire";
        return data;
    }
    public static EventData Cannibalism(GameObject cannibal){
        EventData data = new EventData(offensive:4, disgusting:3, disturbing:3, chaos:3, positive:-3);
        data.key = "cannibalism";
        data.val = 1f;
        data.desc = "acts of cannibalism";
        data.noun = "cannibalism";
        data.whatHappened = Toolbox.Instance.GetName(cannibal) + " commited cannibalism";
        return data;
    }
    public static EventData Death(GameObject dead){
        EventData data = new EventData(offensive:4, disgusting:3, disturbing:4, chaos:4, positive:-3);
        data.key = "death";
        data.val = 1f;
        data.desc = "deaths";
        data.noun = "death";
        data.whatHappened = Toolbox.Instance.GetName(dead) + " died";
        return data;
    }
    public static EventData Eggplant(GameObject eater){
        EventData data = new EventData(positive:1);
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
    public SerializableDictionary<Rating, float> ratings = new SerializableDictionary<Rating, float>(){
        {Rating.disgusting, 0f},
        {Rating.disturbing, 0f},
        {Rating.chaos, 0f},
        {Rating.offensive, 0f},
        {Rating.positive, 0f}
    };
    public EventData(){
    }
    public EventData(float disturbing=0, float disgusting=0, float chaos=0, float offensive=0, float positive=0){
        ratings[Rating.disturbing] = disturbing;
        ratings[Rating.disgusting] = disgusting;
        ratings[Rating.chaos] = chaos;
        ratings[Rating.offensive] = offensive;
        ratings[Rating.positive] = positive;
        // this.disturbing = disturbing;
    }
    public bool MatchSpecific(EventData other){
        // TODO: include descriptions?
        // match on descriptions?
        bool match = true;
        foreach(Rating key in ratings.Keys){
            match &= ratings[key] == other.ratings[key];
        }
        return match;
    }
    public Rating Quality(){
        Dictionary<Rating, float> absRates = new Dictionary<Rating, float>();
        foreach(Rating key in ratings.Keys){
            absRates[key] = Mathf.Abs(ratings[key]);
        }
        return absRates.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
    }
    public string Adjective(){
        Grammar grammar = new Grammar();
		grammar.Load("ratings");
        Rating rate = Quality();
        if (ratings[rate] == 0)
            return "none";
        string key = rate.ToString()+"_"+ratings[rate].ToString();
        return grammar.Parse("{"+key+"}");
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
        EventData data = new EventData(chaos:2);
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
            data.ratings[Rating.disgusting] = 2;
            data.ratings[Rating.chaos] = 2;
        }
        if (edible.immoral){
            data.ratings[Rating.disturbing] = 3;
            data.ratings[Rating.offensive] = 3;
            data.ratings[Rating.chaos] = 3;
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
            data.ratings[Rating.chaos] = 1;
            data.ratings[Rating.offensive] = Random.Range(2, 3);
            data.ratings[Rating.disturbing] = 2;
        }
        if (insult){
            data.whatHappened = speakerName + " insulted " + targetName;
            data.noun = "insults";
            data.ratings[Rating.chaos] = 1;
            data.ratings[Rating.offensive] = Random.Range(2, 3);
            data.ratings[Rating.disturbing] = 2;
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
        EventData data = new EventData(disturbing:2, chaos:2);
        data.noun = "violence";
        data.whatHappened = attackerName + " attacked " + victimName;
        events.Add(data);
    }
}
