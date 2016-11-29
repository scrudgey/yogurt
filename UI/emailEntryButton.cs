using UnityEngine;
using UnityEngine.UI;

public class emailEntryButton : MonoBehaviour {
	private EmailUI emailUI;
	private Email email;
	public Text newText;
	public Text nameText;

	public void Initialize(EmailUI ui, Email initEmail){
		emailUI = ui;
		email = initEmail;
		newText = transform.Find("new").GetComponent<Text>();
		nameText = transform.Find("name").GetComponent<Text>();

		nameText.text = email.subject;
		CheckReadStatus();
	}

	public void CheckReadStatus(){
		if (email.read){
			newText.text = "";
		} else {
			newText.text = "!";
		}
	}
	public void Clicked(){
		emailUI.EmailEntryCallback(email);
		email.read = true;
	}
}
