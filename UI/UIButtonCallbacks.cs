using UnityEngine;

public class UIButtonCallbacks : MonoBehaviour {
	public void FightButtonClick(){
		GameManager.Instance.ToggleFightMode();
	}
	public void SpeakButtonClick(){
        GameObject test = GameObject.Find("DialogueMenu(Clone)");
        if (!test){
            GameObject obj = Instantiate(Resources.Load("UI/DialogueMenu")) as GameObject;
            obj.GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
        }
    }
	public void InventoryButtonClick(){
		UINew.Instance.inventoryVisible = !UINew.Instance.inventoryVisible;
		if (UINew.Instance.inventoryVisible){
			UINew.Instance.ShowInventoryMenu();
		} else {
			UINew.Instance.CloseInventoryMenu();
		}
	}
}
