using System.Collections.Generic;
using UnityEngine;

public abstract class Objective {
    public string desc;
    public abstract bool RequirementsMet(Commercial commercial);
}

public class ObjectiveProperty : Objective {
    public string key;
    public CommercialComparison comparison;
    public float val;
    public ObjectiveProperty(string[] bits) {
        // yogurt,≥,1,eat yogurt on camera
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

public class ObjectiveLocation : Objective {
    public string location;
    override public bool RequirementsMet(Commercial commercial) {
        return commercial.visitedLocations.Contains(location);
    }
    public ObjectiveLocation(string[] bits) {
        location = bits[1];
        desc = "shoot a scene in " + location;
    }
}