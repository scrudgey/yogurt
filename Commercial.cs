using System.Collections.Generic;
using System.Xml.Serialization;

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
    public OccurrenceData data;
	public List<string> outfitsWorn;
	public Commercial(){
        unlockUponCompletion = new List<string>();
        data = new OccurrenceData();
	}
	public List<string> transcript = new List<string>();
	public void SetValue(string valname, float val){
		CommercialProperty property = null;
        properties.TryGetValue(valname, out property);
		float incrementval = val;
        if (property != null){
            incrementval = val - property.val;
        }
		IncrementValue(valname, incrementval);
	}
	public void IncrementValue(string valname, float increment){
		if (increment == 0)
			return;
        CommercialProperty property = null;
        properties.TryGetValue(valname, out property);
        if (property == null){
            properties[valname] = new CommercialProperty();
        }
        float initvalue = properties[valname].val;
        float finalvalue = initvalue + increment;
        string poptext = "default";
        Occurrence.KeyDescriptions.TryGetValue(valname, out poptext);
        if (poptext != "default"){
            UINew.Instance.PopupCounter(poptext, initvalue, finalvalue, this);
        } else {
            // UI check if commercial is complete
            UINew.Instance.UpdateRecordButtons(this);
        }
        properties[valname].val = finalvalue;
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