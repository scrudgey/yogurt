using System.Collections.Generic;
using System.Xml.Serialization;
// using System;
using UnityEngine;

public enum CommercialComparison{
		equal, notequal, greater, less, greaterEqual, lessEqual
	}

[System.Serializable]
[XmlRoot("Commercial")]
public class Commercial {
    public string name = "default";
    public string description = "default";	
	public string cutscene = "default";
	public SerializableDictionary<string, CommercialProperty> properties = new SerializableDictionary<string, CommercialProperty>(); 
    public List<string> unlockUponCompletion;
	public List<EventData> eventData;
	// public List<string> outfitsWorn;
	public static Commercial LoadCommercialByFilename(string filename){
		Commercial c = new Commercial();
		TextAsset dataFile = Resources.Load("data/commercials/"+filename) as TextAsset;
		string[] lineArray = dataFile.text.Split('\n');
		System.Array.Reverse(lineArray);
		Stack<string> lines = new Stack<string>(lineArray);
		// lines = lines.Reverse;
		// lines.Reverse();
		// lines.re
		// Debug.Log(lines.Peek());
		// Debug.Log("loading "+filename);
		// Debug.Log(lines.Peek());
		c.name = lines.Pop();
		c.description = lines.Pop();
		c.cutscene = lines.Pop();
		while(lines.Count > 0){
			CommercialProperty prop = new CommercialProperty();
			string line = lines.Pop();
			string[] bits = line.Split(',');
			// Debug.Log(bits[0]);
			if (bits[0] == "unlock"){
				c.unlockUponCompletion.Add(bits[1]);
			} else {
				prop.val = float.Parse(bits[2]);
				switch (bits[1]){
					case "=":
					prop.comp = CommercialComparison.equal;
					break;
					case ">":
					prop.comp = CommercialComparison.greater;
					break;
					case "<":
					prop.comp = CommercialComparison.less;
					break;
					case "≥":
					prop.comp = CommercialComparison.greaterEqual;
					break;
					case "≤":
					prop.comp = CommercialComparison.lessEqual;
					break;
					default:
					break;
				}
				c.properties[bits[0]] = prop;
			}
		}
		return c;
    }
	public Commercial(){
        unlockUponCompletion = new List<string>();
	}
	public List<string> transcript = new List<string>();
	// public void SetValue( string valname, float val){
	// 	CommercialProperty property = null;
    //     properties.TryGetValue(valname, out property);
	// 	float incrementval = val;
    //     if (property != null){
    //         incrementval = val - property.val;
    //     }
	// 	IncrementValue(valname, incrementval);
	// }
	public EventData Total(){
		EventData total = new EventData();
		foreach(EventData data in eventData){
			total.disturbing += data.disturbing;
			total.disgusting += data.disgusting;
			total.chaos += data.chaos;
			total.offensive += data.offensive;
			total.positive += data.positive;
		}
		return total;
	}
	public void IncrementValue(EventData data){
		if (data.val == 0)
			return;
        CommercialProperty property = null;
        properties.TryGetValue(data.key, out property);
        if (property == null){
            properties[data.key] = new CommercialProperty();
        }
        float initvalue = properties[data.key].val;
        float finalvalue = initvalue + data.val;
        // string poptext = "default";
        // Occurrence.KeyDescriptions.TryGetValue(data.key, out poptext);
        // if (poptext != "default"){
		UINew.Instance.PopupCounter(data.desc, initvalue, finalvalue, this);
        // } else {
        //     // UI check if commercial is complete
        //     UINew.Instance.UpdateRecordButtons(this);
        // }
        properties[data.key].val = finalvalue;
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
	public float val;
	public string desc;
	public CommercialComparison comp;
	public CommercialProperty(){
		val = 0;
		comp = CommercialComparison.equal;
	}
	public CommercialProperty(float val, bool truth, CommercialComparison comp){
		this.val = val;
		this.comp = comp;
	}
}