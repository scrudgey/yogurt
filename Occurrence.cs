using UnityEngine;
using System.Collections.Generic;

public class Occurrence : MonoBehaviour {
    public List<OccurrenceData> data = new List<OccurrenceData>();
    public static Dictionary<string, string> KeyDescriptions = new Dictionary<string, string>{
        {"yogurt", "yogurts eaten"},
        {"vomit", "vomit events"},
        {"yogurt_vomit", "yogurt emesis event"},
        {"yogurt_vomit_eat", "eating yogurt vomit"},
        {"yogurt_floor", "yogurt eaten off the floor"},
        {"table_fire", "table set on fire"},
        {"eggplant", "eggplants eaten"}
	};
}
/*
        NOTE: IMPORTANT
        If i ever need to serialize child data classes, unity will store them only 
        as parent class, thus losing information. To get around this, I will need 
        to do some extra work with ScriptableObject.
*/
[System.Serializable]
public class OccurrenceData
{
    public float disturbing;
    public float disgusting;
    public float chaos;
    public float offensive;
    public float positive;
    public virtual void UpdateCommercialOccurrences(Commercial commercial){ }
    public bool Matches(OccurrenceData otherData){
        if (this.GetType() == otherData.GetType()){
            return MatchSpecific(otherData);
        } else {
            return false;
        }
    }
    protected virtual bool MatchSpecific(OccurrenceData otherData){
        bool match = true;
        match &= disturbing == otherData.disturbing;
        match &= disgusting == otherData.disgusting;
        match &= chaos == otherData.chaos;
        match &= offensive == otherData.offensive;
        match &= positive == otherData.positive;
        return match;
    }
    public void AddData(OccurrenceData otherData){
        disturbing += otherData.disturbing;
        disgusting += otherData.disgusting;
        chaos += otherData.chaos;
        offensive += otherData.offensive;
        positive += otherData.positive;
    }
}
[System.Serializable]
public class OccurrenceFire : OccurrenceData {
    public string objectName;
    public bool extinguished;
    protected override bool MatchSpecific(OccurrenceData data){
        OccurrenceFire otherData = (OccurrenceFire)data;
        return objectName == otherData.objectName;
    }
    public override void UpdateCommercialOccurrences(Commercial commercial){
        // I can eventually modify this to only allow values 0, 1 if necessary
        if (objectName == "table"){
            if (extinguished == false){
                commercial.SetValue("table_fire", 1);
            } else {
                commercial.SetValue("table_fire", 0);
            }
        }
    }
}
[System.Serializable]
public class OccurrenceEat : OccurrenceData {
    public float amount;
    public string food;
    public Liquid liquid;
    public override void UpdateCommercialOccurrences(Commercial commercial){
        if (liquid != null){
            if (liquid.name == "yogurt"){
                commercial.IncrementValue("yogurt", 1f);
                if (liquid.vomit)
                    commercial.IncrementValue("yogurt_vomit_eat", 1f);
                if (food == "Puddle(Clone)")
                    commercial.IncrementValue("yogurt_floor", 1f);
            }
        }
        if (Toolbox.Instance.CloneRemover(food) == "eggplant"){
            commercial.IncrementValue("eggplant", 1f);
        }

    }
    protected override bool MatchSpecific(OccurrenceData data){
        OccurrenceEat otherData = (OccurrenceEat)data;
        bool match = false;
        if (liquid != null){
            if (otherData.liquid == null)
                return false;
            match = liquid.name == otherData.liquid.name;
        } else {
            match = food == otherData.food;
        }
        return match;
    }
}
[System.Serializable]
public class OccurrenceVomit : OccurrenceData {
    public string vomit;
    public Liquid liquid;
    public override void UpdateCommercialOccurrences(Commercial commercial){
        commercial.IncrementValue("vomit", 1f);
        if (liquid.name == "yogurt"){
            commercial.IncrementValue("yogurt_vomit", 1f);
        }
    }
}
[System.Serializable]
public class OccurrenceSpeech : OccurrenceData {
    public string line;
    public GameObject speaker;
    public override void UpdateCommercialOccurrences(Commercial commercial){
        commercial.transcript.Add(line);
    }
    protected override bool MatchSpecific(OccurrenceData data){
        OccurrenceSpeech otherData = (OccurrenceSpeech)data;
        return line == otherData.line;
    }
}