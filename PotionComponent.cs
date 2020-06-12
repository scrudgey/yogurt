﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PotionComponent : MonoBehaviour {
    public PotionData potionData;
    public static List<PotionData> LoadAllPotions() {
        List<PotionData> potions = new List<PotionData>();
        GameObject[] achievementPrefabs = Resources.LoadAll("data/potions/", typeof(GameObject))
            .Cast<GameObject>()
            .ToArray();
        foreach (GameObject prefab in achievementPrefabs) {
            PotionComponent component = prefab.GetComponent<PotionComponent>();
            if (component) {
                potions.Add(component.potionData);
            }
        }
        return potions;
    }
    public static Dictionary<BuffType, PotionData> BuffToPotion() {
        List<PotionData> potions = LoadAllPotions();
        Dictionary<BuffType, PotionData> buffMap = new Dictionary<BuffType, PotionData>();
        foreach (PotionData potion in potions) {
            buffMap.Add(potion.buff.type, potion);
        }
        return buffMap;
    }
}

[System.Serializable]
public struct PotionData {
    public string name;
    public BuffData ingredient1;
    public BuffData ingredient2;
    public Buff buff;
    public string buffDescription;
    public HashSet<string> RequiredItems() {
        return new HashSet<string>() { ingredient1.prefabName, ingredient2.prefabName };
    }
    public bool Satisfied(List<string> ingredients) {
        if (ingredients.Count < 2)
            return false;
        HashSet<string> ingredientSet = new HashSet<string>(ingredients);
        if (ingredientSet.IsSupersetOf(RequiredItems()))
            return true;
        return false;
    }
}
