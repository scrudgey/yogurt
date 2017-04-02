// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

public class Telephone : Item {
	AudioSource source;
	public AudioClip phoneUp;
	// public AudioClip dialTone;
	public AudioClip phoneDown;
	void Start () {
		source = Toolbox.Instance.SetUpAudioSource(gameObject);
		Interaction use = new Interaction(this, "Phone", "UsePhone");
		interactions.Add(use);
	}
	public void UsePhone(){
		if (phoneUp != null)
			source.PlayOneShot(phoneUp);
		GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.phone);
		PhoneMenu menu = menuObject.GetComponent<PhoneMenu>();
		menu.PopulateList();
		menu.telephone = this;
	}
	public string UsePhone_desc(){
		return "Use telephone";
	}
}
