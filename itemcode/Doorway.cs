// using UnityEngine;
// using System.Collections;

public class Doorway : Interactive {

	public string destination;
	public int destinationEntry;
	public int entryID;

	// Use this for initialization
	void Start () {
		Interaction leaveaction = new Interaction(this, "Exit", "Leave");
		interactions.Add(leaveaction);
	}

	public void Leave(){
		GameManager.Instance.LeaveScene(destination,destinationEntry);
	}

}
