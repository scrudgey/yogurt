using UnityEngine;
public class PauseMenu : MonoBehaviour {
	public void Start(){
		Canvas menuCanvas = GetComponent<Canvas>();
		menuCanvas.worldCamera = GameManager.Instance.cam;
	}
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
