using UnityEngine;
using System.Collections.Generic;

//TODO: not all of these things need static constructors?

public class Occurrence : MonoBehaviour {
    public List<OccurrenceData> data = new List<OccurrenceData>();
    // public static Dictionary<string, string> KeyDescriptions = new Dictionary<string, string>{
    //     // {"yogurt", "yogurts eaten"},
    //     // {"vomit", "vomit events"},
    //     // {"vomit_eat", "vomit eaten"},
    //     // {"yogurt_vomit", "yogurt emesis event"},
    //     // {"yogurt_vomit_eat", "eating yogurt vomit"},
    //     // {"yogurt_floor", "yogurt eaten off the floor"},
    //     // {"table_fire", "table set on fire"},
    //     // {"eggplant", "eggplants eaten"},
    //     // {"death", "deaths"},
    //     // {"cannibalism", "acts of cannibalism"}
	// };
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
        EventData data = new EventData(offensive:100f, disgusting:500f, disturbing:350f, chaos:100f, positive:-300f);
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
    public Dictionary<string, float> flags = new Dictionary<string, float>();
    public string key;
    public float val;
    public string desc;
    public string whatHappened;
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
    public string objectName;
    public bool extinguished;
    public override void CalculateDescriptions(){
        EventData data = new EventData(chaos:100f);
        data.noun = "fire";
        data.whatHappened = "the "+objectName+" burned";
        events.Add(data);

        if (objectName == "table"){
            if (extinguished == false){
                // eventData.flags["table_fire"] = 1;
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
            data.disgusting += 75f;
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
                // data.flags["yogurt"] = 1f;
                events.Add(Occurrence.Yogurt(eater));
                if (liquid.vomit){
                    // data.flags["yogurt_vomit_eat"] = 1f;
                    events.Add(Occurrence.YogurtVomitEat(eater));
                }
                if (edible.gameObject.name == "Puddle(Clone)"){
                    // data.flags["yogurt_floor"] = 1f;
                    events.Add(Occurrence.YogurtFloor(eater));
                }
            }
        } 
        if (Toolbox.Instance.GetName(edible.gameObject) == "eggplant"){
            // data.flags["eggplant"] = 1f;
            events.Add(Occurrence.Eggplant(eater));
        }
        if (edible.human){
            // data.noun = "cannibalism";
            // data.flags["cannibalism"] = 1f;
            events.Add(Occurrence.Cannibalism(eater));
		}
    }
}
public class OccurrenceDeath : OccurrenceData {
    // public string nameOfTheDead;
    public GameObject dead;
    public override void CalculateDescriptions(){
        events.Add(Occurrence.Death(dead));
        // eventData.flags["death"] = 1f;
        // eventData.noun = "death";
        // eventData.whatHappened = nameOfTheDead + " died";
        // eventData.chaos = 3575f;
		// eventData.disgusting = 2000f;
		// eventData.disturbing = 3500f;
		// eventData.offensive = 8950f;
		// eventData.positive = -5500f;
    }
}
public class OccurrenceVomit : OccurrenceData {
    // public string vomit;
    // public string vomiter;
    // public Liquid liquid;
    public GameObject vomiter;
    public GameObject vomit;
    public override void CalculateDescriptions(){
        // eventData.noun = "vomiting";
        // eventData.whatHappened = vomiter + " vomited up (name)";
        // eventData.flags["vomit"] = 1f;
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
    public string line;
    public string speaker;
    public string target;
    public bool threat;
    public bool insult;
    public override void CalculateDescriptions(){
        EventData data = new EventData();
        data.whatHappened = speaker + " said " + line;
        data.noun = "speech act";
        // insert bits here for script desc, transcript line
        if (threat){
            data.whatHappened = speaker + " threatened " + target;
            data.chaos += 15f;
            data.offensive = Random.Range(20, 30);
            data.disturbing = 10f;
        }
        if (insult){
            data.whatHappened = speaker + " insulted " + target;
            data.chaos += 15f;
            data.offensive = Random.Range(20, 30);
            data.disgusting = 10f;
        }
        events.Add(data);
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
        events.Add(data);
    }
}
