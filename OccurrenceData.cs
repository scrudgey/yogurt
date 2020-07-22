using Nimrod;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using analysis;


// Occurrence: monobehavior
// occurrencedata: non-serializable runtime fields for passing detailed information between components on sight
//                      contains eventdata
// eventdata: serializable, standard fields.

// to simplify: turn occurrencedata into eventdata if nothing consumes its nonserializable fields
//          * occurrencefire
//          * occurrencenecronomicon
//
// calcdescriptions is employed to ensure that initializing code can set things nicely in code.
// remove this and use constructor code, and or initialization block.
// allow a toolbox method that just takes an eventdata

// eventdata -> occurrencedata -> occurrence

[System.Serializable]
public class DescribableOccurrenceData : MetaDescribable<EventData> {
    public HashSet<string> nouns = new HashSet<string>();
    public DescribableOccurrenceData() {
        whatHappened = "";
    } // needed for serialization
    override public void AddChild(EventData eventData) {
        if (eventData == null)
            return;
        eventData.id = this.id;
        if (eventData.noun != "")
            nouns.Add(eventData.noun);
        base.AddChild(eventData);
    }
    override public void UpdateChildren() {
        this.whatHappened = NotableChild().whatHappened;
        base.UpdateChildren();
    }
    public DescribableOccurrenceData(List<EventData> desc) {
        whatHappened = "";
        foreach (EventData child in desc) {
            AddChild(child);
        }
    }
}

[System.Serializable]
public abstract class OccurrenceData {
    public DescribableOccurrenceData describable = new DescribableOccurrenceData();
    public abstract HashSet<GameObject> involvedParties();
    public virtual void CalculateDescriptions() {
        describable.ResetChildren();
        Descriptions();
    }
    public abstract void Descriptions();
    protected virtual void AddChild(EventData child) {
        describable.AddChild(child);
    }
}
public class OccurrenceEvent : OccurrenceData {
    protected EventData eventData;
    public HashSet<GameObject> involved = new HashSet<GameObject>();
    public override HashSet<GameObject> involvedParties() {
        return involved;
    }
    public OccurrenceEvent(EventData eventData) {
        this.eventData = eventData;
        describable.AddChild(eventData);
    }
    public override void CalculateDescriptions() {
        Descriptions();
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
        // if (edible != null)
        return new HashSet<GameObject> { eater, edible.gameObject };
        // else
        // return new HashSet<GameObject> { eater };
    }
    public override void Descriptions() {
        MonoLiquid monoLiquid = edible.GetComponent<MonoLiquid>();
        string whatHappened = "";
        if (monoLiquid) {
            whatHappened = Toolbox.Instance.GetName(eater) + " drank " + monoLiquid.liquid.name;
        } else {
            whatHappened = Toolbox.Instance.GetName(eater) + " ate " + edible.name;
        }
        EventData data = new EventData("eating", whatHappened);

        Outfit otherOutfit = eater.GetComponent<Outfit>();
        if (otherOutfit != null) {
            eaterOutfitName = otherOutfit.wornUniformName;
        }
        eaterName = eater.name;
        if (edible.offal) {
            data.quality[Rating.disgusting] = 2;
            data.quality[Rating.chaos] = 1;
        }
        if (edible.immoral) {
            data.quality[Rating.disturbing] = 3;
            data.quality[Rating.offensive] = 3;
            data.quality[Rating.chaos] = 3;
        }
        AddChild(data);
        if (edible.vomit) {
            AddChild(EventData.VomitEat(eater));
        }
        string edibleName = Toolbox.Instance.GetName(edible.gameObject);
        if (edibleName == "yogurt cup") {
            yogurt = true;
            AddChild(EventData.Yogurt(eater));
        }
        if (liquid != null) {
            if (liquid.name == "yogurt" || liquid.ingredients.Contains("yogurt")) {
                yogurt = true;
                if (liquid.vomit) {
                    AddChild(EventData.YogurtVomitEat(eater));
                }
                if (edible.gameObject.name.ToLower().Contains("puddle")) {
                    AddChild(EventData.YogurtFloor(eater));
                    whatHappened = $"{Toolbox.Instance.GetName(eater)} ate yogurt off the floor";
                }
                AddChild(EventData.Yogurt(eater));
            }
            if (liquid.name == "gravy" || liquid.ingredients.Contains("gravy")) {
                gravy = true;
                AddChild(EventData.Gravy(eater));
            }
            if (liquid.ingredients.Contains("eggplant")) {
                AddChild(EventData.Eggplant(eater));
            }
        }
        if (edibleName.Contains("gravy")) {
            gravy = true;
            AddChild(EventData.Gravy(eater));
        }
        if (edibleName.Contains("eggplant")) {
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
    public MessageDamage lastDamage;
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
public class OccurrenceSpeech : OccurrenceEvent {
    public GameObject speaker;
    public GameObject target;
    public string line;
    public bool threat;
    public bool insult;
    public int profanity;
    public List<bool> swearList = new List<bool>();
    public OccurrenceSpeech(EventData eventData) : base(eventData) { }
    public override HashSet<GameObject> involvedParties() {
        return new HashSet<GameObject> { speaker, target };
    }
    public override void CalculateDescriptions() {
        Descriptions();
    }
    public override void Descriptions() {
        describable.ResetChildren();
        string speakerName = Toolbox.Instance.GetName(speaker);
        string targetName = "";
        // if (target != null) {
        //     targetName = Toolbox.Instance.GetName(target);
        //     Debug.Log(target);
        //     Debug.Log(targetName);
        // }
        EventData data = new EventData("dialogue", speakerName + " said " + line);
        data.transcriptLine = speakerName + ": " + line;
        // insert bits here for script desc, transcript line
        if (threat) {
            data.whatHappened = speakerName + " threatened " + targetName;
            data.noun = "threats";
        }
        if (insult) {
            data.whatHappened = speakerName + " insulted " + targetName;
            data.noun = "insults";
        }
        if (eventData != null)
            data.quality = eventData.quality;

        data.quality[Rating.chaos] += (int)(profanity / 10);
        data.quality[Rating.offensive] += (int)(profanity / 2);
        if (profanity > 0 && data.noun == "dialogue")
            data.noun = "profanity";

        AddChild(data);
    }
}
public class OccurrenceViolence : OccurrenceData {
    public GameObject attacker;
    public GameObject victim;
    public float amount;
    public MessageDamage lastMessage;
    public override HashSet<GameObject> involvedParties() {
        return new HashSet<GameObject> { attacker, victim };
    }
    public override void Descriptions() {
        string attackerName = Toolbox.Instance.GetName(attacker);
        string victimName = Toolbox.Instance.GetName(victim);

        EventData data = new EventData(disturbing: 2, chaos: 2);
        Hurtable victimHurtable = victim.GetComponent<Hurtable>();
        if (victimHurtable == null) {
            data = new EventData(disturbing: 1, chaos: 1);
        }

        data.noun = "violence";
        data.whatHappened = attackerName + " attacked " + victimName;

        if (lastMessage.type == damageType.cutting) {
            data.whatHappened = $"{attackerName} stabbed {victimName}";
        } else if (lastMessage.type == damageType.fire) {
            data.whatHappened = attackerName + " burned " + victimName;
        } else if (lastMessage.type == damageType.piercing) {
            data.whatHappened = attackerName + " stabbed " + victimName;
        }

        if (lastMessage.weaponName != null)
            data.whatHappened += $" with {lastMessage.weaponName}";

        AddChild(data);
    }
}