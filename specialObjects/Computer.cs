using UnityEngine;
public class Computer : Item {
	AnimateUIBubble newBubble;
	void Awake () {
		interactions.Add(new Interaction(this, "Email", "OpenEmail"));
		newBubble = GetComponent<AnimateUIBubble>();
		newBubble.DisableFrames();
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
		if (activeBubble){
			newBubble.EnableFrames();
		} else {
			newBubble.DisableFrames();
		}
	}
	public void OpenEmail(){
		GameObject menu = UINew.Instance.ShowMenu(UINew.MenuType.email);
		menu.GetComponent<EmailUI>().computer = this;
	}
}
