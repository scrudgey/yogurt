using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        return boolValue || (time < lifetime);
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
    enraged
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
}
