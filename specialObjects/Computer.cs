using UnityEngine;
public class Computer : Item {
	GameObject newBubble;
	void Start () {
		interactions.Add(new Interaction(this, "Email", "OpenEmail"));
		newBubble = transform.Find("newBubble").gameObject;
		CheckBubble();
	}
	public void CheckBubble(){
		bool activeBubble = false;
		if (GameManager.Instance.data == null)
			return;
		foreach (Email email in GameManager.Instance.data.emails){
			if (email.read == false)
				activeBubble = true;
		}
		newBubble.SetActive(activeBubble);
	}
	public void OpenEmail(){
		GameObject menu = GameObject.Instantiate(Resources.Load("UI/EmailUI")) as GameObject;
		menu.GetComponent<EmailUI>().computer = this;
	}
}
