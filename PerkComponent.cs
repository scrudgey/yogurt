﻿using UnityEngine;

public class PerkComponent : MonoBehaviour {
	public Perk perk;
}

[System.Serializable]
public class Perk {
	public string name;
	public string title;
	public string desc;
	public Sprite perkImage;
	public Perk(){

	}
	public Perk(Perk otherPerk){
		this.name = otherPerk.name;
		this.title = otherPerk.title;
		this.desc = otherPerk.desc;
		this.perkImage = otherPerk.perkImage;
	}
}