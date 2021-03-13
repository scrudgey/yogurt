using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Linq;

[System.Serializable]
[XmlInclude(typeof(ObjectiveProperty))]
[XmlInclude(typeof(ObjectiveEat))]
[XmlInclude(typeof(ObjectiveLocation))]
[XmlInclude(typeof(ObjectiveEatName))]
[XmlInclude(typeof(ObjectiveScorpion))]
[XmlInclude(typeof(ObjectiveMayorHead))]
public abstract class Objective {
    public string desc;
    public abstract bool RequirementsMet(Commercial commercial);
    public Objective() {

    }
}

[System.Serializable]
public class ObjectiveProperty : Objective {
    Dictionary<string, Rating> ratings = new Dictionary<string, Rating>{
        {"disgusting", Rating.disgusting},
        {"disturbing", Rating.disturbing},
        {"offensive", Rating.offensive},
        {"chaos", Rating.chaos},
        {"positive", Rating.positive},
    };
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

        // if metric, check metric. otherwise, check property
        if (ratings.ContainsKey(key)) {
            float metric = commercial.quality[ratings[key]];
            return Compare(metric);
        }
        CommercialProperty property = null;
        commercial.properties.TryGetValue(key, out property);
        if (property == null) {
            return false;
        }
        return Compare(property.val);
    }
    public bool Compare(float otherProperty) {
        switch (comparison) {
            case CommercialComparison.equal:
                return otherProperty == this.val;
            case CommercialComparison.notequal:
                return otherProperty != this.val;
            case CommercialComparison.greater:
                return otherProperty > this.val;
            case CommercialComparison.less:
                return otherProperty < this.val;
            case CommercialComparison.greaterEqual:
                return otherProperty >= this.val;
            case CommercialComparison.lessEqual:
                return otherProperty <= this.val;
            default:
                return true;
        }
    }
}

[System.Serializable]
public class ObjectiveLocation : Objective {
    // location,krazy1,shoot a scene outside
    public List<string> locations;
    override public bool RequirementsMet(Commercial commercial) {
        foreach (string location in locations) {
            if (commercial.visitedLocations.Contains(location))
                return true;
        }
        return false;
    }
    public ObjectiveLocation(string[] bits) {
        desc = bits[1];
        locations = new List<string>(bits.Skip(1).Take(bits.Length - 1));
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
    public int limit;
    public ObjectiveScorpion() { } // required for serialization
    public ObjectiveScorpion(string[] bits) {
        this.limit = int.Parse(bits[1]);
        this.desc = bits[2];
    }
    override public bool RequirementsMet(Commercial commercial) {
        return GameManager.Instance.data.baddiesDefeated >= limit;
    }
}

[System.Serializable]
public class ObjectiveMayorHead : Objective {
    public ObjectiveMayorHead() {
        this.desc = "Bring the mayor's head to the gravy studio";
    } // required for serialization
    override public bool RequirementsMet(Commercial commercial) {
        foreach (DescribableOccurrenceData data in commercial.GetChildren()) {
            if (data.whatHappened == "I saw the mayor's head") {
                return true;
            }
        }
        return false;
    }
}