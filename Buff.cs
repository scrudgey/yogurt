using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[System.Serializable]
public class Buff {
    public BuffType type;
    public bool boolValue;
    public float floatValue;
    public float lifetime;
    public float time;
    public Buff() { }
    public Buff(BuffType type, bool boolValue, float floatValue, float lifetime) {
        this.type = type;
        this.boolValue = boolValue;
        this.floatValue = floatValue;
        this.lifetime = lifetime;
        this.time = 0;
    }
    public Buff(Buff otherBuff) {
        this.type = otherBuff.type;
        this.boolValue = otherBuff.boolValue;
        this.floatValue = otherBuff.floatValue;
        this.lifetime = otherBuff.lifetime;
        this.time = otherBuff.time;
    }
    public bool active() {
        return boolValue || (time < lifetime) || floatValue > 0;
    }
    public bool Update() {
        bool changed = false;
        time += Time.deltaTime;
        if (time > lifetime && lifetime > 0) {
            boolValue = false;
            floatValue = 0;
            changed = true;
        } else {
            boolValue = true;
        }
        return changed;
    }
    public static Buff operator +(Buff a, Buff b) {
        Buff result = new Buff(a);

        result.boolValue |= b.boolValue;
        result.floatValue += b.floatValue;

        // adjust times.
        if (a.lifetime == 0 && b.lifetime > 0) {
            result.lifetime = b.lifetime;
            result.time = b.time;
        } else if (b.lifetime == 0 && a.lifetime > 0) {
            result.lifetime = a.lifetime;
            result.time = a.time;
        } else if (b.lifetime > 0 && a.lifetime > 0) {
            result.lifetime = Mathf.Max(a.lifetime, b.lifetime);
            result.time = Mathf.Min(a.time, b.time);
        }

        return result;
    }

    public static Dictionary<BuffType, string> buffNames = new Dictionary<BuffType, string>{
        {BuffType.armor, "armor"},
        {BuffType.bonusHealth, "bonus health"},
        {BuffType.clearHeaded, "mindfulness"},
        {BuffType.coughing, "coughing"},
        {BuffType.death, "death"},
        {BuffType.enraged, "rage"},
        {BuffType.ethereal, "ethereal"},
        {BuffType.fireproof, "fireproof"},
        {BuffType.invulnerable, "invulnerable"},
        {BuffType.noPhysicalDamage, "nonphysical"},
        {BuffType.poison, "poison"},
        {BuffType.speed, "speed"},
        {BuffType.strength, "strength"},
        {BuffType.telepathy, "telepathy"},
        {BuffType.undead, "undead"},
        {BuffType.acidDamage, "acid"},
        {BuffType.polymorph, "polymorph"},
        {BuffType.offensive, "offensive"},
        {BuffType.disturbing, "disturbing"},
        {BuffType.disgusting, "disgusting"},
    };


    public static List<Buff> FlattenBuffs(List<Buff> l1) {
        List<Buff> flattenedBuffs = new List<Buff>();
        foreach (BuffType buffType in Enum.GetValues(typeof(BuffType))) {
            IEnumerable<Buff> subBuffs = l1.Where(x => x.type == buffType);
            if (subBuffs.Count() > 0) {
                flattenedBuffs.Add(l1
                .Where(x => x.type == buffType)
                .Aggregate<Buff>((prod, next) => prod + next)
                );
            }
        }
        return flattenedBuffs;
    }
}
public enum BuffType {
    telepathy,
    speed,
    bonusHealth,
    armor,
    fireproof,
    noPhysicalDamage,
    invulnerable,
    strength,
    poison,
    undead,
    ethereal,
    coughing,
    death,
    clearHeaded,
    enraged,
    acidDamage,
    polymorph,
    offensive,
    disturbing,
    disgusting
}
[System.Serializable]
public class BuffData {
    public Sprite icon;
    public string name;
    public string prefabName;
    public Color spriteColor = Color.white;
    public BuffData(Sprite icon, string name, string prefabName, Color color) {
        this.icon = icon;
        this.name = name;
        this.prefabName = prefabName;
        this.spriteColor = color;
    }
    public BuffData() { }
}
