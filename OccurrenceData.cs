using Nimrod;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public abstract class OccurrenceData {
    public System.Guid id = System.Guid.NewGuid();
    public List<EventData> events = new List<EventData>();
    public abstract HashSet<GameObject> involvedParties();
    public void CalculateDescriptions() {
        events = new List<EventData>();
        Descriptions();
        foreach (EventData eDat in events) {
            eDat.id = this.id.ToString();
        }
    }
    public abstract void Descriptions();
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
        events.Add(data);
        if (objectName == "table") {
            if (extinguished == false) {
                events.Add(EventData.TableFire());
            }
        }
    }
}
public class OccurrenceEat : OccurrenceData {
    public Liquid liquid;
    public Edible edible;
    public GameObject eater;
    public string eaterOutfitName;
    public bool yogurt;
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
        data.noun = "eating";
        if (edible.offal) {
            data.ratings[Rating.disgusting] = 2;
            data.ratings[Rating.chaos] = 2;
        }
        if (edible.immoral) {
            data.ratings[Rating.disturbing] = 3;
            data.ratings[Rating.offensive] = 3;
            data.ratings[Rating.chaos] = 3;
        }
        events.Add(data);
        if (edible.vomit) {
            events.Add(EventData.VomitEat(eater));
        }
        string edibleName = Toolbox.Instance.GetName(edible.gameObject);
        if (
            (liquid != null && (liquid.name == "yogurt" || liquid.ingredients.Contains("yogurt")))
             || edibleName == "yogurt"
             ) {
            yogurt = true;
            // adjust
            events.Add(EventData.Yogurt(eater));
            if (liquid.vomit) {
                events.Add(EventData.YogurtVomitEat(eater));
            }
            if (edible.gameObject.name == "Puddle(Clone)") {
                events.Add(EventData.YogurtFloor(eater));
            }
        }
        if (edibleName == "eggplant") {
            events.Add(EventData.Eggplant(eater));
        }
        if (edible.human) {
            events.Add(EventData.Cannibalism(eater));
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
        events.Add(EventData.Death(dead, lastAttacker, lastDamage, monster, suicide, assailant));
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
        events.Add(data);
        if (vomit != null) {
            MonoLiquid mliquid = vomit.GetComponent<MonoLiquid>();
            if (mliquid == null)
                return;
            if (mliquid.liquid != null) {
                if (mliquid.liquid.name == "yogurt")
                    events.Add(EventData.VomitYogurt(vomiter));
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
        events.Add(data);
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
        EventData data = null;
        if (events.Count > 0) {
            data = events[0];
        } else {
            data = new EventData();
        }
        data.whatHappened = speakerName + " said " + line;
        data.noun = "dialogue";
        data.transcriptLine = speakerName + ": " + line;
        // insert bits here for script desc, transcript line
        if (threat) {
            data.whatHappened = speakerName + " threatened " + targetName;
            data.noun = "threats";
            data.ratings[Rating.chaos] = 1;
            data.ratings[Rating.offensive] = Random.Range(2, 3);
            data.ratings[Rating.disturbing] = 2;
        }
        if (insult) {
            data.whatHappened = speakerName + " insulted " + targetName;
            data.noun = "insults";
            data.ratings[Rating.chaos] = 1;
            data.ratings[Rating.offensive] = Random.Range(2, 3);
            data.ratings[Rating.disturbing] = 2;
        }
        if (profanity > 0 && data.noun == "dialogue")
            data.noun = "profanity";
        if (events.Count == 0) {
            events.Add(data);
        }
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
        events.Add(data);
    }
}