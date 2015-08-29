using UnityEngine;
using System.Collections;

public class SingletonInitializer : MonoBehaviour {

	// Use this for initialization
	void Start () {
		string tempstring;
		tempstring = GameManager.Instance.message;
		tempstring = Toolbox.Instance.message;
		tempstring = Messenger.Instance.MOTD;
		tempstring = UISystem.Instance.MOTD;
		Debug.Log(tempstring);
	}

}
