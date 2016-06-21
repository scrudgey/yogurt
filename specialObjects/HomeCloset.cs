// using UnityEngine;
// using System.Collections;

public class HomeCloset : Interactive {

	// Use this for initialization
	void Start () {
		interactions.Add(new Interaction(this, "Open", "OpenCloset"));
	}
	
	public void OpenCloset(){
		UINew.Instance.ShowClosetMenu();
	}
	public string OpenCloset_desc(){
		return "Browse closet";
	}
}
