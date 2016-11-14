using UnityEngine;
// using System.Collections;

public class SingletonInitializer : MonoBehaviour {
	void Start () {
		string tempstring;
		tempstring = Toolbox.Instance.message;
		tempstring = GameManager.Instance.message;
		tempstring = Messenger.Instance.MOTD;
		tempstring = UINew.Instance.MOTD;
        tempstring += " my droogs";
	}
}
