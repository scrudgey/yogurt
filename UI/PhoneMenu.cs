// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhoneMenu : MonoBehaviour {
	AudioSource source;
	public AudioClip dialTone;
	public AudioClip phoneDown;
	public Transform numbersList;
	public Telephone telephone;
	public Color defaultButtonColor;
	public Color highlightButtonColor;
	public Color disabledButtonColor;
	private PhoneNumberButton selectedButton;
	private Image callButtonImage;
	void Start(){
		numbersList = transform.Find("main/Numbers");
		callButtonImage = transform.Find("main/ButtonBar/Call").GetComponent<Image>();
		callButtonImage.color = disabledButtonColor;
		source = Toolbox.Instance.SetUpAudioSource(gameObject);
		source.spatialBlend = 0;
		selectedButton = null;
	}
	public void CloseButton(){
		Destroy(gameObject);
		if (phoneDown != null)
			Toolbox.Instance.AudioSpeaker(phoneDown, telephone.transform.position);
	}
	public void PopulateList(){
		Start();
		GameObject newEntry = Instantiate(Resources.Load("UI/PhoneNumberButton")) as GameObject;
		newEntry.transform.SetParent(numbersList, false);
		PhoneNumberButton buttonScript = newEntry.GetComponent<PhoneNumberButton>();
		buttonScript.menu = this;
		buttonScript.SetPhoneType(PhoneNumberButton.phoneNumber.fire);
	}
	public void PhoneButtonCallback(PhoneNumberButton button){
		if (selectedButton != null){
			Image selectedImage = selectedButton.GetComponent<Image>();
			selectedImage.color = defaultButtonColor;
		}
		selectedButton = button;
		callButtonImage.color = highlightButtonColor;
		Image newImage = selectedButton.GetComponent<Image>();
		newImage.color = highlightButtonColor;
	}
	public void CallButtonCallback(){
		if (selectedButton == null){
			Debug.Log("no selected");
			return;
		} 
		Debug.Log(selectedButton.number);
	}
}
