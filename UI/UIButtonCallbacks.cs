using UnityEngine;

public class UIButtonCallbacks : MonoBehaviour {
	public void FightButtonClick(){
		Controller.Instance.focus.ToggleFightMode();
	}
	public void SpeakButtonClick(){
		UINew.Instance.ShowMenu(UINew.MenuType.speech);
    }
	public void InventoryButtonClick(){
		UINew.Instance.ShowInventoryMenu();
	}
	public void FinishButtonClick(){
		VideoCamera video = GameObject.FindObjectOfType<VideoCamera>();
		video.live = false;
		if (video){
			GameManager.Instance.EvaluateCommercial(video.commercial);
		}
	}
	public void StopRecButtonClick(){
		ScriptDirector director = GameObject.FindObjectOfType<ScriptDirector>();
		director.ResetScript();
	}
	public void SaveButtonClick(){
		MySaver.Save();
		Debug.Log("save...");
	}
	public void LoadButtonClick(){
		GameManager.Instance.SetFocus(MySaver.LoadScene()); 
		Debug.Log("load...");
		// Debug.Break();
	}
}
