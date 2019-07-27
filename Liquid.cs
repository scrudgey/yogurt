using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public int vegetable;
    public int meat;
    public int immoral;
    public int offal;
    public bool flammable;
    public bool vomit;
    public List<string> ingredients = new List<string>();
    public List<Buff> buffs = new List<Buff>();
    public Liquid() {
        // needed for serialization
    }
    public Liquid(float r, float g, float b) {
        color = new Color(r / 255.0F, g / 255.0F, b / 255.0F);
        vegetable = 1;
        nutrition = 10;
    }
    public static Liquid MixLiquids(Liquid l1, Liquid l2) {
        Liquid returnLiquid = l1;
        returnLiquid.filename = l1.filename;
        returnLiquid.vegetable = l1.vegetable + l2.vegetable;
        returnLiquid.meat = l1.meat + l2.meat;
        returnLiquid.immoral = l1.immoral + l2.immoral;
        returnLiquid.offal = l1.offal + l2.offal;
        returnLiquid.colorR = (l1.colorR + l2.colorR) / 2.0f;
        returnLiquid.colorG = (l1.colorG + l2.colorG) / 2.0f;
        returnLiquid.colorB = (l1.colorB + l2.colorB) / 2.0f;
        returnLiquid.color = new Color(returnLiquid.colorR / 255.0F, returnLiquid.colorG / 255.0F, returnLiquid.colorB / 255.0F);

        if (l2.ingredients.Count > 0) {
            l1.ingredients.AddRange(l2.ingredients);
            l1.ingredients = l1.ingredients.Distinct().ToList();
        }
        l1.ingredients.Add(l1.name);
        l1.ingredients.Add(l2.name);

        // TODO: smarter add here
        l1.buffs.AddRange(l2.buffs);

        l1.name = GetName(l1);
        return returnLiquid;
    }
    public static Buff MixPotion(Liquid liq) {
        List<PotionData> potions = PotionComponent.LoadAllPotions();
        foreach (PotionData potion in potions) {
            if (potion.Satisfied(liq.ingredients)) {
                liq.ingredients.Remove(potion.ingredient1.prefabName);
                liq.ingredients.Remove(potion.ingredient2.prefabName);
                return potion.buff;
            }
        }
        return null;
    }
    public static string GetName(Liquid liq) {
        if (liq.buffs.Count > 0) {
            return NameOfBuffs(liq);
        } else if (liq.ingredients.Count == 0) {
            return liq.name;
        } else if (liq.ingredients.Count == 1) {
            return liq.ingredients.ToList()[0] + " juice";
        } else if (liq.ingredients.Count == 2) {
            List<string> ingredients = liq.ingredients.ToList();
            return ingredients[0] + "-" + ingredients[1] + " juice";
        } else if (liq.ingredients.Count > 2) {
            return NameOfQualities(liq);
        }
        return "";
    }
    public static string NameOfQualities(Liquid liq) {
        Dictionary<string, float> qualities = new Dictionary<string, float>(){
            {"vegetable smoothie", liq.vegetable},
            {"meat smoothie", liq.meat},
            {"liquid remains", liq.immoral},
            {"sludge", liq.offal},
        };
        return qualities.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
    }
    public static string NameOfBuffs(Liquid liq) {
        Dictionary<BuffType, PotionData> buffMap = PotionComponent.BuffToPotion();
        if (liq.buffs.Count == 0) {
            return "water";
        } else if (liq.buffs.Count == 1) {
            return "potion of " + buffMap[liq.buffs[0].type].name;
        } else if (liq.buffs.Count == 2) {
            string buff1 = buffMap[liq.buffs[0].type].name;
            string buff2 = buffMap[liq.buffs[1].type].name;
            return buff1 + " " + buff2 + " potion";
        } else if (liq.buffs.Count == 3) {
            return "magic potion";
        } else if (liq.buffs.Count == 4) {
            return "supermagic potion";
        } else if (liq.buffs.Count == 5) {
            return "ultramagic potion";
        } else if (liq.buffs.Count == 6) {
            return "giga potion";
        } else {
            return "hyper potion";
        }
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
        l.colorA = float.Parse(data["a"]);
        l.colorR = float.Parse(data["r"]);
        l.colorG = float.Parse(data["g"]);
        l.colorB = float.Parse(data["b"]);
        l.nutrition = float.Parse(data["nutrition"]);
        l.vegetable = int.Parse(data["vegetable"]);
        l.meat = int.Parse(data["meat"]);
        l.immoral = int.Parse(data["immoral"]);
        l.offal = int.Parse(data["offal"]);
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
        if (data.ContainsKey("invulnerability")) {
            Buff buff = new Buff();
            buff.type = BuffType.invulnerable;
            buff.boolValue = true;
            buff.lifetime = 20f;
            l.buffs.Add(buff);
        }
        if (data.ContainsKey("ethereal")) {
            Buff buff = new Buff();
            buff.type = BuffType.ethereal;
            buff.boolValue = true;
            buff.lifetime = 20f;
            l.buffs.Add(buff);
        }
        return l;
    }
    public static void MonoLiquidify(GameObject target, Liquid liquid, bool timeout = false) {
        // TODO: create intrinsics on gameobject and add liquid intrinsic
        MonoLiquid monoLiquid = Toolbox.GetOrCreateComponent<MonoLiquid>(target);
        monoLiquid.liquid = liquid;
        monoLiquid.edible.nutrition = liquid.nutrition;
        monoLiquid.edible.vegetable = liquid.vegetable > 0;
        monoLiquid.edible.meat = liquid.meat > 0;
        monoLiquid.edible.immoral = liquid.immoral > 0;
        monoLiquid.edible.offal = liquid.offal > 0;
        monoLiquid.edible.pureeColor = liquid.color;
        monoLiquid.edible.vomit = liquid.vomit;
        if (liquid.flammable) {
            Flammable existingFlammable = target.transform.root.GetComponentInChildren<Flammable>();
            if (existingFlammable == null) {
                Flammable flam = target.AddComponent<Flammable>();
                flam.flashpoint = 0.1f;
            }
        }
        if (liquid.buffs.Count > 0) {
            Intrinsics intrinsics = Toolbox.GetOrCreateComponent<Intrinsics>(target);
            intrinsics.buffs.AddRange(liquid.buffs);
        }
    }
}