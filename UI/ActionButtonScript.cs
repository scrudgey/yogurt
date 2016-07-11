using UnityEngine;

public class ActionButtonScript: MonoBehaviour {

	public enum buttonType {Drop, Throw, Stash, Inventory, Action, Punch}

	public buttonType bType = buttonType.Action;

	public Interaction action;
	public bool manualAction = false;
	public string itemName;
	public string buttonText;
	private bool mouseHeld;

	public void clicked(){
		if (bType == buttonType.Action){
			if (Controller.Instance.InteractionIsWithinRange(action) || manualAction){
				action.DoAction();
				if (!action.dontWipeInterface){
					UINew.Instance.ClearWorldButtons();
				}
				UINew.Instance.InventoryButtonsCheck();
			}
			UINew.Instance.SetActionText("");
		} else {
			UINew.Instance.HandActionCallback(bType);
			UINew.Instance.SetActionText("");
		}
		GUI.FocusControl("none");
	}

	void Update(){
		if (mouseHeld && bType == buttonType.Action){
			if (action.continuous && (Controller.Instance.InteractionIsWithinRange(action) || manualAction )){
				action.DoAction();
			}
		}
	}
	
	public void MouseUp(){
		mouseHeld = false;
	}
	
	public void MouseDown(){
		mouseHeld = true;
	}

	public void MouseOver(){
		if (bType == buttonType.Action){
			// Debug.Log(action.Description());
			UINew.Instance.SetActionText(action.Description());
		} else {
			UINew.Instance.SetActionText(UINew.Instance.HandActionDescription(bType));
		}
	}

	public void MouseExit(){
		UINew.Instance.SetActionText("");
	}
}