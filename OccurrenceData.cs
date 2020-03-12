using Nimrod;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using analysis;


[System.Serializable]
public class DescribableOccurrenceData : Describable {
    public List<EventData> children;
    public DescribableOccurrenceData() { } // needed for serialization
    public DescribableOccurrenceData(List<Describable> desc) {
        whatHappened = "";
        foreach (Describable child in desc) {
            AddChild(child);
            whatHappened += child.whatHappened + "\n";
        }
    }
    new public List<EventData> GetChildren() {
        return children;
    }
}

[System.Serializable]
public abstract class OccurrenceData : Describable {
    public abstract HashSet<GameObject> involvedParties();
    public void CalculateDescriptions() {
        // children = new List<Describable>();
        ResetChildren();
        Descriptions();
    }
    public abstract void Descriptions();
    public DescribableOccurrenceData ToDescribable() {
        return new DescribableOccurrenceData(GetChildren());
    }
}
public class OccurrenceGeneric : OccurrenceData {
    public override HashSet<GameObject> involvedParties() {
        return new HashSet<GameObject> { };
    }
    override public void Descriptions() { }
}
public class OccurrenceFire : OccurrenceData {
    public GameObject flamingObject;
    public bool extinguished;
    public override HashSet<GameObject> involvedParties() {
        return new HashSet<GameObject> { flamingObject };
    }
    public override void Descriptions() {
        EventData data = new EventData(chaos: 2);
        string objectName = Toolbox.Instance.GetName(flamingObject);
        data.noun = "fire";
        data.whatHappened = "the " + objectName + " burned";
        AddChild(data);
        if (objectName == "table") {
            if (extinguished == false) {
                AddChild(EventData.TableFire());
                // children.Add(EventData.TableFire());
            }
        }
    }
}
public class OccurrenceEat : OccurrenceData {
    public Liquid liquid;
    public Edible edible;
    public GameObject eater;
    public string eaterOutfitName;
    public string eaterName;
    public bool yogurt;
    public bool gravy;
    public override HashSet<GameObject> involvedParties() {
        return new HashSet<GameObject> { eater, edible.gameObject };
    }
    public override void Descriptions() {
        EventData data = new EventData();
        MonoLiquid monoLiquid = edible.GetComponent<MonoLiquid>();
        if (monoLiquid) {
            data.whatHappened = Toolbox.Instance.GetName(eater) + " drank " + monoLiquid.liquid.name;
        } else {
            data.whatHappened = Toolbox.Instance.GetName(eater) + " ate " + edible.name;
        }
        Outfit otherOutfit = eater.GetComponent<Outfit>();
        if (otherOutfit != null) {
            eaterOutfitName = otherOutfit.wornUniformName;
        }
        eaterName = eater.name;
        data.noun = "eating";
        if (edible.offal) {
            data.quality[Rating.disgusting] = 2;
            data.quality[Rating.chaos] = 2;
        }
        if (edible.immoral) {
            data.quality[Rating.disturbing] = 3;
            data.quality[Rating.offensive] = 3;
            data.quality[Rating.chaos] = 3;
        }
        AddChild(data);
        // children.Add(data);
        if (edible.vomit) {
            AddChild(EventData.VomitEat(eater));
        }
        string edibleName = Toolbox.Instance.GetName(edible.gameObject);
        if ((liquid != null && (liquid.name == "yogurt" || liquid.ingredients.Contains("yogurt"))) || edibleName == "yogurt") {
            yogurt = true;
            AddChild(EventData.Yogurt(eater));
            if (liquid.vomit) {
                AddChild(EventData.YogurtVomitEat(eater));
            }
            if (edible.gameObject.name == "Puddle(Clone)") {
                AddChild(EventData.YogurtFloor(eater));
            }
        }
        if ((liquid != null && (liquid.name == "gravy" || liquid.ingredients.Contains("gravy"))) || edibleName == "gravy") {
            gravy = true;
            AddChild(EventData.Gravy(eater));
        }
        if (edibleName == "eggplant") {
            AddChild(EventData.Eggplant(eater));
        }
        if (edible.human) {
            AddChild(EventData.Cannibalism(eater));
        }
    }
}
public class OccurrenceDeath : OccurrenceData {
    public GameObject dead;
    public bool suicide;
    public bool damageZone;
    public bool assailant;
    public GameObject lastAttacker;
    public damageType lastDamage;
    public bool monster;
    public OccurrenceDeath(bool monster) {
        this.monster = monster;
    }
    public override HashSet<GameObject> involvedParties() {
        return new HashSet<GameObject> { dead, lastAttacker };
    }
    public override void Descriptions() {
        if (lastAttacker == null)
            return;
        if (lastAttacker == dead) {
            suicide = true;
        } else {
            if (lastAttacker.GetComponent<DamageZone>() != null)
                damageZone = true;
            if (lastAttacker.GetComponent<Inventory>() != null)
                assailant = true;
        }
        AddChild(EventData.Death(dead, lastAttacker, lastDamage, monster, suicide, assailant));
    }
}
public class OccurrenceVomit : OccurrenceData {
    public GameObject vomiter;
    public GameObject vomit;
    public override HashSet<GameObject> involvedParties() {
        return new HashSet<GameObject> { vomit, vomiter };
    }
    public override void Descriptions() {
        if (vomit == null & vomiter == null)
            return;
        EventData data = EventData.Vomit(vomiter, vomit);
        AddChild(data);
        if (vomit != null) {
            MonoLiquid mliquid = vomit.GetComponent<MonoLiquid>();
            if (mliquid == null)
                return;
            if (mliquid.liquid != null) {
                if (mliquid.liquid.name == "yogurt")
                    AddChild(EventData.VomitYogurt(vomiter));
            }
        }
    }
}
public class OccurrenceNecronomicon : OccurrenceData {
    public override HashSet<GameObject> involvedParties() {
        return new HashSet<GameObject> { };
    }
    public override void Descriptions() {
        EventData data = EventData.EldritchHorror();
        AddChild(data);
    }
}
public class OccurrenceSpeech : OccurrenceData {
    // TODO: include disturbingness for swearing;
    public GameObject speaker;
    public GameObject target;
    public string line;
    public bool threat;
    public bool insult;
    public int profanity;
    public override HashSet<GameObject> involvedParties() {
        return new HashSet<GameObject> { speaker, target };
    }
    public override void Descriptions() {
        string speakerName = Toolbox.Instance.GetName(speaker);
        string targetName = "";
        if (target != null)
            targetName = Toolbox.Instance.GetName(target);
        // EventData data = null;
        // if (GetChildren().Count > 0) {
        //     data = (EventData)GetChildren()[0];
        // } else {
        EventData data = new EventData();
        // }
        data.whatHappened = speakerName + " said " + line;
        data.noun = "dialogue";
        data.transcriptLine = speakerName + ": " + line;
        // insert bits here for script desc, transcript line
        if (threat) {
            data.whatHappened = speakerName + " threatened " + targetName;
            data.noun = "threats";
            data.quality[Rating.chaos] = 1;
            data.quality[Rating.offensive] = Random.Range(2, 3);
            data.quality[Rating.disturbing] = 2;
        }
        if (insult) {
            data.whatHappened = speakerName + " insulted " + targetName;
            data.noun = "insults";
            data.quality[Rating.chaos] = 1;
            data.quality[Rating.offensive] = Random.Range(2, 3);
            data.quality[Rating.disturbing] = 2;
        }
        if (profanity > 0 && data.noun == "dialogue")
            data.noun = "profanity";
        // if (children.Count == 0) {
        //     children.Add(data);
        // }
        AddChild(data);
    }
}
public class OccurrenceViolence : OccurrenceData {
    public GameObject attacker;
    public GameObject victim;
    public float amount;
    public damageType type;
    public override HashSet<GameObject> involvedParties() {
        return new HashSet<GameObject> { attacker, victim };
    }
    public override void Descriptions() {
        string attackerName = Toolbox.Instance.GetName(attacker);
        string victimName = Toolbox.Instance.GetName(victim);
        EventData data = new EventData(disturbing: 2, chaos: 2);
        data.noun = "violence";
        data.whatHappened = attackerName + " attacked " + victimName;
        if (type == damageType.cutting) {
            data.whatHappened = attackerName + " stabbed " + victimName;
        } else if (type == damageType.fire) {
            data.whatHappened = attackerName + " burned " + victimName;
        } else if (type == damageType.piercing) {
            data.whatHappened = attackerName + " stabbed " + victimName;
        }
        AddChild(data);
    }
}