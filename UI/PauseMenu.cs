using UnityEngine;
public class PauseMenu : MonoBehaviour {

	public void SaveClick(){
		MySaver.Save();
		GameManager.Instance.TitleScreen();
	}
	public void QuitClick(){
		GameManager.Instance.TitleScreen();
	}
	public void ContinueClick(){
		Destroy(gameObject);
	}
}
