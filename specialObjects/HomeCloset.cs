// using UnityEngine;
// using System.Collections;

public class HomeCloset : Interactive {

	public enum ClosetType {all, items, food, clothing}
	public ClosetType type;
	// Use this for initialization
	void Start () {
		interactions.Add(new Interaction(this, "Open", "OpenCloset"));
	}
	
	public void OpenCloset(){
		ClosetButtonHandler menu = UINew.Instance.ShowClosetMenu();
		menu.PopulateItemList(type);
	}
	public string OpenCloset_desc(){
		return "Browse closet";
	}
}
