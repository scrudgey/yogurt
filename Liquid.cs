using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[System.Serializable]
public class Liquid { //: IEquatable<Liquid> {
    public string name;
    public string filename;
    public float colorR;
    public float colorG;
    public float colorB;
    public float colorA;
    public Color color = Color.white;
    public float nutrition;
    public int vegetable;
    public int meat;
    public int immoral;
    public int offal;
    public bool flammable;
    public bool vomit;
    public List<string> ingredients = new List<string>();
    public List<Buff> buffs = new List<Buff>();
    public HashSet<Liquid> atomicLiquids = new HashSet<Liquid>();
    public bool newLiquid;
    public Liquid() {
        // needed for serialization
    }
    public Liquid(Liquid that) { // deep copy
        this.name = that.name;
        this.filename = that.filename;
        this.colorR = that.colorR;
        this.colorG = that.colorG;
        this.colorB = that.colorB;
        this.colorA = that.colorA;
        this.color = that.color;
        this.nutrition = that.nutrition;
        this.vegetable = that.vegetable;
        this.meat = that.meat;
        this.immoral = that.immoral;
        this.offal = that.offal;
        this.flammable = that.flammable;
        this.vomit = that.vomit;
        this.ingredients = that.ingredients;
        this.newLiquid = that.newLiquid;

        this.ingredients = new List<string>();
        foreach (string ingredient in that.ingredients) {
            this.ingredients.Add(ingredient);
        }

        this.atomicLiquids = new HashSet<Liquid>();
        foreach (Liquid atomicLiquid in that.atomicLiquids) {
            this.atomicLiquids.Add(new Liquid(atomicLiquid));
        }

        this.buffs = new List<Buff>();
        foreach (Buff buff in that.buffs) {
            this.buffs.Add(new Buff(buff));
        }
    }

    public bool Equals(Liquid that) {
        return this.name == that.name &&
        this.filename == that.filename &&
        // this.colorR == that.colorR &&
        // this.colorG == that.colorG &&
        // this.colorB == that.colorB &&
        // this.colorA == that.colorA &&
        // this.color == that.color &&
        this.nutrition == that.nutrition &&
        this.vegetable == that.vegetable &&
        this.meat == that.meat &&
        this.immoral == that.immoral &&
        this.offal == that.offal &&
        this.flammable == that.flammable &&
        this.vomit == that.vomit;
        // this.ingredients == that.ingredients &&
        // this.buffs == that.buffs;
    }
    public static Liquid MixLiquids(Liquid l1, Liquid l2) {
        Liquid returnLiquid = new Liquid(l1);
        returnLiquid.filename = l1.filename;
        returnLiquid.vegetable = l1.vegetable + l2.vegetable;
        returnLiquid.meat = l1.meat + l2.meat;
        returnLiquid.immoral = l1.immoral + l2.immoral;
        returnLiquid.offal = l1.offal + l2.offal;
        returnLiquid.colorR = (l1.colorR + l2.colorR) / 2.0f;
        returnLiquid.colorG = (l1.colorG + l2.colorG) / 2.0f;
        returnLiquid.colorB = (l1.colorB + l2.colorB) / 2.0f;

        returnLiquid.color = AddColors(l1.color, l2.color);

        // returnLiquid.color = new Color(returnLiquid.colorR / 255.0F, returnLiquid.colorG / 255.0F, returnLiquid.colorB / 255.0F);

        if (l2.ingredients.Count > 0) {
            returnLiquid.ingredients.AddRange(l2.ingredients);
            returnLiquid.ingredients = returnLiquid.ingredients.Distinct().ToList();
        }
        returnLiquid.ingredients.Add(returnLiquid.name);
        returnLiquid.ingredients.Add(l2.name);

        // TODO: smarter add here
        returnLiquid.buffs.AddRange(l2.buffs);

        returnLiquid.name = GetName(returnLiquid);

        // I'm doing this instead of redefining equality / hash because I don't feel like it
        foreach (Liquid atomicLiquid in l2.atomicLiquids) {
            bool collision = false;
            foreach (Liquid myLiquid in returnLiquid.atomicLiquids) {
                if (myLiquid.Equals(atomicLiquid)) {
                    collision = true;
                    break;
                }
            }
            if (!collision) {
                // Debug.Log("adding atomic liquid " + atomicLiquid.name);
                returnLiquid.atomicLiquids.Add(atomicLiquid);
            }
        }
        return returnLiquid;
    }
    public static Color AddColors(Color x, Color y) {
        List<Color32> c = Colors.colors.Values.ToList();
        return c[Mathf.Abs(x.GetHashCode() + y.GetHashCode()) % c.Count];
    }
    public static List<Buff> MixPotion(Liquid liq) {
        List<PotionData> potions = PotionComponent.LoadAllPotions();
        List<Buff> newlyMixedBuffs = new List<Buff>();
        foreach (PotionData potion in potions) {
            if (potion.Satisfied(liq.ingredients)) {
                if (potion.makeYogurt) {
                    liq = LoadLiquid("yogurt");

                } else if (!liq.buffs.Contains(potion.buff)) {
                    Buff newBuff = potion.buff;
                    if (GameManager.Instance.data.perks["potion"]) {
                        newBuff.lifetime = 0;
                    }
                    newlyMixedBuffs.Add(newBuff);
                }

                // unlock potion
                MutablePotionData mutableData = GameManager.Instance.data.collectedPotions[potion.name];
                mutableData.unlockedIngredient1 = true;
                mutableData.unlockedIngredient2 = true;
                GameManager.Instance.data.collectedPotions[potion.name] = mutableData;
            }
        }
        return newlyMixedBuffs;
    }
    public static bool MixYogurt(Liquid liquid) {
        GameObject potionPrefab = Resources.Load("data/potions/yogurt") as GameObject;
        PotionComponent component = potionPrefab.GetComponent<PotionComponent>();
        PotionData potionData = new PotionData(component.potionData);
        return potionData.Satisfied(liquid.ingredients);
    }
    public static string GetName(Liquid liq) {

        string returnString = "";

        if (liq.buffs.Count > 0) {
            returnString = NameOfBuffs(liq);
        } else if (liq.ingredients.Count == 0) {
            returnString = liq.name;
        } else if (liq.ingredients.Count == 1) {
            returnString = liq.ingredients.ToList()[0] + " juice";
        } else if (liq.ingredients.Count == 2) {
            List<string> ingredients = liq.ingredients.ToList();
            returnString = ingredients[0] + "-" + ingredients[1] + " juice";
        } else if (liq.ingredients.Count > 2) {
            returnString = NameOfQualities(liq);
        }
        if (liq.vomit) {
            returnString = "vomited-up " + returnString;
        }
        return returnString;
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
            // permanent / not permanent
            if (liq.buffs[0].lifetime == 0) {
                return "potion of permanent " + buffMap[liq.buffs[0].type].name;
            } else {
                return "potion of " + buffMap[liq.buffs[0].type].name;
            }
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
            buff.lifetime = 5f;
            l.buffs.Add(buff);
        }
        if (data.ContainsKey("death")) {
            Buff buff = new Buff();
            buff.type = BuffType.death;
            buff.boolValue = true;
            // buff.lifetime = 20f;
            l.buffs.Add(buff);
        }
        l.atomicLiquids.Add(new Liquid(l));
        return l;
    }
    public static void MonoLiquidify(GameObject target, Liquid liquid) {
        MonoLiquid monoLiquid = Toolbox.GetOrCreateComponent<MonoLiquid>(target);
        monoLiquid.liquid = new Liquid(liquid);
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