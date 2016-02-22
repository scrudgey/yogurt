﻿using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;

public enum CommercialComparison{
		equal,
		notequal,
		greater,
		less,
		greaterEqual,
		lessEqual
	}
// [System.Serializable]

[XmlRoot("Commercial")]
public class Commercial {
    public string name = "default";
    public string path = "default";
    
    public string description = "default";
    public float reward = 0;
	public SerializableDictionary<string, CommercialProperty> properties = new SerializableDictionary<string, CommercialProperty>(); 
	
    public List<string> unlockUponCompletion;
    
	public Commercial(){
		properties["yogurt"] = new CommercialProperty();
        unlockUponCompletion = new List<string>();
        // unlockUponCompletion.Add("test");
	}
	
	public bool Evaluate(Commercial other){
		bool requirementsMet = true;
		foreach(string key in other.properties.Keys){
            
			CommercialProperty myProperty = null;
			CommercialProperty otherProperty = null;
			other.properties.TryGetValue(key, out otherProperty);
            properties.TryGetValue(key, out myProperty);
            
			if (myProperty == null){
				requirementsMet = false;
                Debug.Log("did not find key in other");
				break;
			}
			switch (otherProperty.comp)
			{
				case CommercialComparison.equal:
				requirementsMet = myProperty.val == otherProperty.val;
				break;
				case CommercialComparison.notequal:
				requirementsMet = myProperty.val != otherProperty.val;
				break;
				case CommercialComparison.greater:
				requirementsMet = myProperty.val > otherProperty.val;
				break;
				case CommercialComparison.less:
				requirementsMet = myProperty.val < otherProperty.val;
				break;
				case CommercialComparison.greaterEqual:
				requirementsMet = myProperty.val >= otherProperty.val;
				break;
				case CommercialComparison.lessEqual:
				requirementsMet = myProperty.val <= otherProperty.val;
				break;
				default:
				break;
			}
			if (!requirementsMet)
			break;
		}
		
		return requirementsMet;
	}
}

[System.Serializable]
public class CommercialProperty {
	// public string name;
	public float val;
	public CommercialComparison comp;
	public CommercialProperty(){
		// name = "default";
		val = 0;
		comp = CommercialComparison.equal;
	}
	public CommercialProperty(float val, bool truth, CommercialComparison comp){
		this.val = val;
		this.comp = comp;
	}
}