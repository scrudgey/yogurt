using UnityEngine;

public class UIButtonCallbacks : MonoBehaviour {
	public void FightButtonClick(){
		Controller.Instance.focus.ToggleFightMode();
	}
	public void SpeakButtonClick(){
        GameObject test = GameObject.Find("DialogueMenu(Clone)");
        if (!test){
            GameObject obj = Instantiate(Resources.Load("UI/DialogueMenu")) as GameObject;
            obj.GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
        }
    }
	public void InventoryButtonClick(){
		if (UINew.Instance.inventoryVisible){
			UINew.Instance.CloseInventoryMenu();
		} else {
			Inventory inventory = GameManager.Instance.playerObject.GetComponent<Inventory>();
			UINew.Instance.ShowInventoryMenu(inventory);
		}
	}

	public void FinishButtonClick(){
		VideoCamera video = GameObject.FindObjectOfType<VideoCamera>();
		video.live = false;
		if (video){
			GameManager.Instance.EvaluateCommercial(video.commercial);
		}
		// GameManager.Instance.EvaluateCommercial();
	}

	public void StopRecButtonClick(){
		VideoCamera video = GameObject.FindObjectOfType<VideoCamera>();
		video.live = false;
		GameManager.Instance.activeCommercial = null;
		UINew.Instance.EnableRecordButtons(false);
	}
}
