using UnityEngine;
// using Nimrod;
using System.IO;
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
	public void SaveButtonClick(){
		MySaver.Save();
		MySaver.SaveObjectDatabase();
	}
	public void LoadButtonClick(){
		GameManager.Instance.SetFocus(MySaver.LoadScene()); 
	}
	public void TestButtonClick(){
		string path = Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName);
		DirectoryInfo dataDir = new DirectoryInfo(path);
 		dataDir.Delete(true);
		// Debug.Log("test");
		// Grammar g = new Grammar();
		// g.Load("insult");
		// Debug.Log(g.Parse("{main}"));
	}
	public void HypnosisButtonClick(){
		if (Controller.Instance.currentSelect != Controller.SelectType.hypnosis){
			Controller.Instance.currentSelect = Controller.SelectType.hypnosis;
		} else {
			Controller.Instance.currentSelect = Controller.SelectType.none;
		}
	}
}
