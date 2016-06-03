using UnityEngine;
using UnityEngine.SceneManagement;
// using System.Collections;

public class PauseMenu : MonoBehaviour {

	public void SaveClick(){
		MySaver.Save();
		// GameManager.Instance.SaveGameData();
		SceneManager.LoadScene("title");
	}
	
	public void QuitClick(){
		SceneManager.LoadScene("title");
	}
}
