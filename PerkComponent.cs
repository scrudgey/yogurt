using UnityEngine;

public class PerkComponent : MonoBehaviour {
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
}