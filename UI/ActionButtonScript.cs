using UnityEngine;
using System.Collections;

public class ActionButtonScript: MonoBehaviour {

	public enum buttonType {Drop,Throw,Stash,Inventory,Action}

	public buttonType bType = buttonType.Action;

	public Interaction action;
	public bool manualAction = false;
	public string itemName;
	public string buttonText;
	private bool mouseHeld;

	public void clicked(){

		switch (bType){
		case buttonType.Action:
			if (Controller.Instance.InteractionIsWithinRange(action) || manualAction){
				action.DoAction();
				if (!action.dontWipeInterface){
					UISystem.Instance.WipeWorldActions();
					UISystem.Instance.doUpdate = true;
				}
			}
			break;
		case buttonType.Drop:
			UISystem.Instance.DropCallback();
			break;
		case buttonType.Inventory:
			UISystem.Instance.InventoryButtonCallback(this);
			UISystem.Instance.WipeHandActions();
			UISystem.Instance.doUpdate = true;
			break;
		case buttonType.Stash:
			UISystem.Instance.StashCallback();
			break;
		case buttonType.Throw:
			UISystem.Instance.ThrowCallback();
			break;
		}

	}

	void Update(){
		if (mouseHeld && bType == buttonType.Action){
			if (action.continuous && (Controller.Instance.InteractionIsWithinRange(action) || manualAction )){
				action.DoAction();
			}
		}
	}

	public void MouseEnter(){
		UISystem.Instance.MouseOverAction(this);
	}

	public void MouseExit(){
		UISystem.Instance.MouseExitAction(this);
	}
	
	public void MouseUp(){
		mouseHeld = false;
	}
	
	public void MouseDown(){
		mouseHeld = true;
	}
}