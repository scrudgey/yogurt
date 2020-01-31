using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[System.Serializable]
[XmlInclude(typeof(ObjectiveProperty))]
[XmlInclude(typeof(ObjectiveEat))]
[XmlInclude(typeof(ObjectiveLocation))]
[XmlInclude(typeof(ObjectiveEatName))]
[XmlInclude(typeof(ObjectiveScorpion))]
public abstract class Objective {
    public string desc;
    public abstract bool RequirementsMet(Commercial commercial);
    public Objective() {

    }
}

[System.Serializable]
public class ObjectiveProperty : Objective {
    // yogurt,≥,1,eat yogurt on camera
    public string key;

    public CommercialComparison comparison;
    public float val;
    public ObjectiveProperty(string[] bits) {
        key = bits[0];
        val = float.Parse(bits[2]);
        desc = bits[3];
        switch (bits[1]) {
            case "=":
                comparison = CommercialComparison.equal;
                break;
            case ">":
                comparison = CommercialComparison.greater;
                break;
            case "<":
                comparison = CommercialComparison.less;
                break;
            case "≥":
                comparison = CommercialComparison.greaterEqual;
                break;
            case "≤":
                comparison = CommercialComparison.lessEqual;
                break;
            default:
                break;
        }
    }
    public ObjectiveProperty() { } // required for serialization 
    override public bool RequirementsMet(Commercial commercial) {
        CommercialProperty property = null;
        commercial.properties.TryGetValue(key, out property);
        if (property == null) {
            return false;
        }
        return Compare(property);
    }
    public bool Compare(CommercialProperty otherProperty) {
        switch (comparison) {
            case CommercialComparison.equal:
                return otherProperty.val == this.val;
            case CommercialComparison.notequal:
                return otherProperty.val != this.val;
            case CommercialComparison.greater:
                return otherProperty.val > this.val;
            case CommercialComparison.less:
                return otherProperty.val < this.val;
            case CommercialComparison.greaterEqual:
                return otherProperty.val >= this.val;
            case CommercialComparison.lessEqual:
                return otherProperty.val <= this.val;
            default:
                return true;
        }
    }
}

[System.Serializable]
public class ObjectiveLocation : Objective {
    // location,krazy1,shoot a scene outside
    public string location;
    override public bool RequirementsMet(Commercial commercial) {
        return commercial.visitedLocations.Contains(location);
    }
    public ObjectiveLocation(string[] bits) {
        location = bits[1];
        // desc = "shoot a scene in " + location;
        desc = bits[2];
    }
    public ObjectiveLocation() { } // required for serialization 
}

[System.Serializable]
public class ObjectiveEat : Objective {
    public string eaterOutfit;
    public ObjectiveEat(string[] bits) {
        eaterOutfit = bits[1];
        desc = bits[2];
    }
    override public bool RequirementsMet(Commercial commercial) {
        return commercial.yogurtEaterOutfits.Contains(eaterOutfit);
    }
    public ObjectiveEat() { } // required for serialization 
}

[System.Serializable]
public class ObjectiveEatName : Objective {
    public string eaterName;
    public ObjectiveEatName(string[] bits) {
        eaterName = bits[1];
        desc = bits[2];
    }
    override public bool RequirementsMet(Commercial commercial) {
        foreach (string namo in commercial.yogurtEaterNames) {
            if (namo.StartsWith(eaterName))
                return true;
        }
        return false;
    }
    public ObjectiveEatName() { } // required for serialization 
}

[System.Serializable]
public class ObjectiveScorpion : Objective {
    public string eaterOutfit;
    public ObjectiveScorpion() {
        // eaterOutfit = bits[1];
        // desc = bits[2];
        this.desc = "defeat the Scorpion Gang";
    }
    override public bool RequirementsMet(Commercial commercial) {
        // return commercial.yogurtEaterOutfits.Contains(eaterOutfit);
        return GameManager.Instance.data.gangMembersDefeated >= 5;
    }
}