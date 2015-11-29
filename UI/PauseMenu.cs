using UnityEngine;
// using System.Collections;

public class PauseMenu : MonoBehaviour {

	public void SaveClick(){
		MySaver.Save();
		// GameManager.Instance.SaveGameData();
		Application.LoadLevel("title");
	}
	
	public void QuitClick(){
		Application.LoadLevel("title");		
	}
}
