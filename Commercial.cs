﻿using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
// using System;
using System.IO;

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
		c.name = lines.Pop();
		c.description = lines.Pop();
		c.cutscene = lines.Pop();
		while(lines.Count > 0){
			CommercialProperty prop = new CommercialProperty();
			string line = lines.Pop();
			string[] bits = line.Split(',');
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
			properties[data.key].desc = data.desc;
        }
        float initvalue = properties[data.key].val;
        float finalvalue = initvalue + data.val;
		UINew.Instance.PopupCounter(data.desc, initvalue, finalvalue, this);
        properties[data.key].val = finalvalue;
    }
	public void WriteReport(){
		string filename = Path.Combine(Application.persistentDataPath, "commercial_history.txt");
		StreamWriter writer = new StreamWriter(filename, true);
		foreach(EventData data in eventData){
			writer.WriteLine(data.whatHappened);
		}
		writer.Close();
		filename = Path.Combine(Application.persistentDataPath, "commercial_events.txt");
		writer = new StreamWriter(filename, true);
		foreach(EventData data in eventData){
			string line = data.noun + " " + data.disturbing.ToString() + " " + data.disgusting.ToString() + " " + data.chaos.ToString() + " " + data.offensive.ToString() + " " + data.positive.ToString();
			writer.WriteLine(line);
		}
		writer.Close();
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