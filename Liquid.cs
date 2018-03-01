using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Liquid {
    public string name;
    public string filename;
    public float colorR;
    public float colorG;
    public float colorB;
    public float colorA;
    public Color color;
    public float nutrition;
    public bool vegetable;
    public bool meat;
    public bool immoral;
    public bool offal;
    public bool flammable;
    public bool vomit;
    public List<Buff> buffs = new List<Buff>();
    public Liquid() {
        // needed for serialization
    }
    public Liquid(float r, float g, float b) {
        color = new Color(r / 255.0F, g / 255.0F, b / 255.0F);
        vegetable = true;
        nutrition = 10;
    }
    public static Liquid MixLiquids(Liquid l1, Liquid l2) {
        Liquid returnLiquid = l1;
        returnLiquid.filename = l1.filename;
        returnLiquid.vegetable = l1.vegetable | l2.vegetable;
        returnLiquid.meat = l1.meat | l2.meat;
        returnLiquid.immoral = l1.immoral | l2.immoral;
        returnLiquid.offal = l1.offal | l2.offal;
        returnLiquid.colorR = (l1.colorR + l2.colorR) / 2.0f;
        returnLiquid.colorG = (l1.colorG + l2.colorG) / 2.0f;
        returnLiquid.colorB = (l1.colorB + l2.colorB) / 2.0f;
        returnLiquid.color = new Color(returnLiquid.colorR / 255.0F, returnLiquid.colorG / 255.0F, returnLiquid.colorB / 255.0F);
        if (!l1.name.Contains("mix")) {
            l1.name = l1.name + "-" + l2.name + " mix";
        }
        return returnLiquid;
    }
    public static Liquid LoadLiquid(string name) {
        Liquid l = new Liquid();
        TextAsset dataFile = Resources.Load("data/liquids/" + name) as TextAsset;
        string[] dataLines = dataFile.text.Split('\n');
        Dictionary<string, string> data = new Dictionary<string, string>();
        foreach (string line in dataLines) {
            string[] bits = line.Split('=');
            data[bits[0]] = bits[1];
        }
        l.name = data["name"];
        l.filename = name;
        l.color = new Color(float.Parse(data["r"]) / 255.0F,
            float.Parse(data["g"]) / 255.0F,
            float.Parse(data["b"]) / 255.0F,
            float.Parse(data["a"]) / 255.0f);
        l.nutrition = float.Parse(data["nutrition"]);
        l.vegetable = bool.Parse(data["vegetable"]);
        l.meat = bool.Parse(data["meat"]);
        l.immoral = bool.Parse(data["immoral"]);
        l.offal = bool.Parse(data["offal"]);
        l.flammable = bool.Parse(data["flammable"]);
        if (data.ContainsKey("strength")) {
            Buff buff = new Buff();
            buff.type = BuffType.strength;
            buff.boolValue = true;
            buff.lifetime = 25f;
            l.buffs.Add(buff);
        }
        if (data.ContainsKey("poison")) {
            Buff buff = new Buff();
            buff.type = BuffType.poison;
            buff.boolValue = true;
            buff.lifetime = 5f;
            l.buffs.Add(buff);
        }
        return l;
    }
    public static void MonoLiquidify(GameObject target, Liquid liquid, bool timeout = false) {
        // TODO: create intrinsics on gameobject and add liquid intrinsic
        MonoLiquid monoLiquid = Toolbox.Instance.GetOrCreateComponent<MonoLiquid>(target);
        monoLiquid.liquid = liquid;
        monoLiquid.edible.nutrition = liquid.nutrition;
        monoLiquid.edible.vegetable = liquid.vegetable;
        monoLiquid.edible.meat = liquid.meat;
        monoLiquid.edible.immoral = liquid.immoral;
        monoLiquid.edible.offal = liquid.offal;
        monoLiquid.edible.pureeColor = liquid.color;
        monoLiquid.edible.vomit = liquid.vomit;
        if (liquid.flammable) {
            Flammable flam = target.AddComponent<Flammable>();
            flam.flashpoint = 0.1f;
        }
        if (liquid.buffs.Count > 0) {
            Intrinsics intrinsics = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(target);
            intrinsics.buffs.AddRange(liquid.buffs);
        }
    }
}