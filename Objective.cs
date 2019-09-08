using System.Collections.Generic;
using UnityEngine;

public abstract class Objective {
    public abstract bool RequirementsMet(Commercial required, Commercial commercial);
}

public class ObjectiveProperty : Objective {
    override public bool RequirementsMet(Commercial required) {

        foreach (KeyValuePair<string, CommercialProperty> kvp in required.properties) {
            CommercialProperty myProperty = null;
            CommercialProperty otherProperty = kvp.Value;
            properties.TryGetValue(kvp.Key, out myProperty);
            if (myProperty == null) {
                requirementsMet = false;
                break;
            }
            requirementsMet = myProperty.RequirementMet(otherProperty);
            if (!requirementsMet)
                break;
        }

        return true;
    }
}

public class ObjectiveLocation : Objective {
    override public bool RequirementsMet(Commercial required) {

    }
}