using UnityEngine;
using System.Collections.Generic;

public class Occurrence : MonoBehaviour {
    public List<OccurrenceData> data = new List<OccurrenceData>();
}
public enum occurrenceType{
        eat, vomit
}

/*
        NOTE: IMPORTANT
        If i ever need to serialize child data classes, unity will store them only 
        as parent class, thus losing information. To get around this, I will need 
        to do some extra work with ScriptableObject.
*/

[System.Serializable]
public class OccurrenceData {
    public occurrenceType myType;
    public float disturbing;
    public float disgusting;
    public float chaos;
    
    public OccurrenceData(){}
    public OccurrenceData(occurrenceType thisType){
        myType = thisType;
    }
    public void AddData(OccurrenceData otherData){
        disturbing += otherData.disturbing;
        disgusting += otherData.disgusting;
        chaos += otherData.chaos;
    }
}


public class OccurrenceEat : OccurrenceData {
    public float amount;
    public string food;
    public Liquid liquid;
    public OccurrenceEat(){
        myType = occurrenceType.eat;
    }
}


public class OccurrenceVomit : OccurrenceData {
    public string vomit;
    public Liquid liquid;
    
    public OccurrenceVomit(){
        myType = occurrenceType.vomit;
    }
}