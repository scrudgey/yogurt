using UnityEngine;

public class Pickup : Item {
    public AudioClip[] pickupSounds;
    public Inventory holder;
    public bool heavyObject;
    public bool largeObject;
    public Sprite icon;
}
