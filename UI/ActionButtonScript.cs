using UnityEngine;
// using System.Collections;

public class ActionButtonScript: MonoBehaviour {

	public enum buttonType {Drop, Throw, Stash, Inventory, Action}

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
		} else {
			UINew.Instance.HandActionCallback(bType);
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
}