using UnityEngine;

public class PerkButton : MonoBehaviour {
	public PerkMenu menu;
	public Perk perk;
	public void Clicked(){
		menu.PerkButtonClicked(this);
	}
}
