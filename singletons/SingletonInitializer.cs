using UnityEngine;
public class SingletonInitializer : MonoBehaviour {
	void Start () {
		string tempstring;
		tempstring = Toolbox.Instance.message;
		tempstring = GameManager.Instance.message;
		tempstring = ClaimsManager.Instance.MOTD;
		tempstring = UINew.Instance.MOTD;
		// tempstring = CutsceneManager.Instance.MOTD;
        tempstring += " my droogs";
	}
}
