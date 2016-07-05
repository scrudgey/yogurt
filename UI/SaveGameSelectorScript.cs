using UnityEngine;
// using System.Collections;
using UnityEngine.UI;

public class SaveGameSelectorScript : MonoBehaviour {
	public Text nameText;
	public Text dateText;
	public Text timeText;
	public Image icon;
	public string saveName;
	
	// Use this for initialization
	void Start () {
		ConfigValues();
	}

	public void ConfigValues(){
		GameObject temp = transform.Find("HeadShot/Icon").gameObject;
		icon = temp.GetComponent<Image>();
		temp = transform.Find("TextPanel/Name").gameObject;
		nameText = temp.GetComponent<Text>();
		temp = transform.Find("TextPanel/Last").gameObject;
		dateText = temp.GetComponent<Text>();
		temp = transform.Find("TextPanel/Time").gameObject;
		timeText = temp.GetComponent<Text>();
	}

	public void Clicked(){
	 	GetComponentInParent<StartMenu>().LoadGameSelect(this);
	}
}
