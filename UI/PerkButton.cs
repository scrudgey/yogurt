using UnityEngine;

public class PerkButton : MonoBehaviour {
	public PerkMenu menu;
	public Perk perk;
	// public string perkTitle;
	// public string perkDesc;
	// public string perkName;
	// public bool enabled;
	public void Clicked(){
		menu.PerkButtonClicked(this);
	}
	// public void MouseOver(){
	// 	menu.PerkButtonMouseOver(this);
	// }
}
