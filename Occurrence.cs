﻿using UnityEngine;
using System.Collections.Generic;

public class Occurrence : MonoBehaviour {
    public List<OccurrenceData> data = new List<OccurrenceData>();
}
public enum occurrenceType{
        eat, vomit, act
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
    
    public virtual bool Matches(OccurrenceData otherData){
        bool match = false;
        match = disturbing == otherData.disturbing;
        match &= disgusting == otherData.disgusting;
        match &= chaos == otherData.chaos;
        
        return match;
    }
}


public class OccurrenceEat : OccurrenceData {
    public float amount;
    public string food;
    public Liquid liquid;
    public OccurrenceEat(){
        myType = occurrenceType.eat;
    }
    public override bool Matches(OccurrenceData otherData){
        bool match = false;
        OccurrenceEat other = (OccurrenceEat)otherData;
        if (other == null)
            return false;
        
        if (liquid != null){
            if (other.liquid == null)
                return false;
            match = liquid.name == other.liquid.name;
        } else {
            match = food == other.food;
        }
        
        return match;
    }
}


public class OccurrenceVomit : OccurrenceData {
    public string vomit;
    public Liquid liquid;
    public OccurrenceVomit(){
        myType = occurrenceType.vomit;
    }
}

public class OccurrenceSpeech : OccurrenceData {
    public string line;
    public GameObject speaker;
    public OccurrenceSpeech(){
        myType = occurrenceType.act;
    }
    
    public override bool Matches(OccurrenceData otherData){
        bool match = false;
        OccurrenceSpeech other = (OccurrenceSpeech)otherData;
        if (other == null)
            return false;
        
        match = speaker == other.speaker;
        match &= line == other.line;
            
        return match;
    }
}



