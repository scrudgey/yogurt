using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PerkComponent : MonoBehaviour {
    public bool disablePerk = false;
    public Perk perk;
}

[System.Serializable]
public class Perk {
    public int requiredPerks;
    [TextArea(3, 10)]
    public string name;
    [TextArea(3, 10)]
    public string title;
    [TextArea(3, 10)]
    public string desc;
    public Sprite perkImage;
    public Perk() { } // required for serialization 
    public Perk(Perk otherPerk) {
        this.name = otherPerk.name;
        this.title = otherPerk.title;
        this.desc = otherPerk.desc;
        this.perkImage = otherPerk.perkImage;
        this.requiredPerks = otherPerk.requiredPerks;
    }

    // public static Dictionary<GameObject, PerkComponent> LoadAllPerks() {
    //     GameObject[] perkPrefabs = Resources.LoadAll("data/perks/", typeof(GameObject))
    //         .Cast<GameObject>()
    //         .ToArray();

    //     Dictionary<GameObject, PerkComponent> perkComponents = new Dictionary<GameObject, PerkComponent>();
    //     foreach (GameObject prefab in perkPrefabs) {
    //         PerkComponent component = prefab.GetComponent<PerkComponent>();
    //         if (component && !component.disablePerk)
    //             perkComponents[prefab] = component;
    //     }
    //     perkPrefabs = perkPrefabs.OrderBy(p => perkComponents[p].perk.requiredPerks).ToArray();
    //     return perkComponents;
    // }
}