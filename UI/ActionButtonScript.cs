using UnityEngine;

public class ActionButtonScript: MonoBehaviour {

	public enum buttonType {Drop, Throw, Stash, Inventory, Action, Punch}
	public buttonType bType = buttonType.Action;
	public Interaction action;
	public bool manualAction = false;
	public string itemName;
	public string buttonText;
	private bool mouseHeld;
	public Inventory inventory;

	public void clicked(){
		if (bType == buttonType.Action){
			if (Controller.Instance.InteractionIsWithinRange(action) || manualAction){
				action.DoAction();
				if (!action.dontWipeInterface){
					UINew.Instance.ClearWorldButtons();
				}
				Controller.Instance.focus.DetermineInventoryActions();
			}
			UINew.Instance.SetActionText("");
		} else {
			// UINew.Instance.HandActionCallback(bType);
			HandActionCallback(bType);
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
			UINew.Instance.SetActionText(action.Description());
		} else {
			UINew.Instance.SetActionText(HandActionDescription());
		}
	}
	public void MouseExit(){
		UINew.Instance.SetActionText("");
	}

	public void HandActionCallback(ActionButtonScript.buttonType bType){
		switch (bType){
		case ActionButtonScript.buttonType.Drop:
		inventory.DropItem();
		UINew.Instance.ClearWorldButtons();
		break;

		case ActionButtonScript.buttonType.Throw:
		inventory.ThrowItem();
		UINew.Instance.ClearWorldButtons();
		break;

		case ActionButtonScript.buttonType.Stash:
		inventory.StashItem(inventory.holding.gameObject);
		UINew.Instance.ClearWorldButtons();
		UINew.Instance.HandleInventoryButton(inventory);
		// if (inventoryVisible){
		// 	CloseInventoryMenu();
		// 	ShowInventoryMenu();
		// }
		break;

		case ActionButtonScript.buttonType.Punch:
		if (inventory)
			inventory.StartPunch();
		break;

		default:
		break;
		}
	}

	public string HandActionDescription(){
		// ActionButtonScript.buttonType bType = aScript.bType;
		string itemname = "";
		switch (bType){
		case ActionButtonScript.buttonType.Drop:
			itemname = Toolbox.Instance.GetName(inventory.holding.gameObject);
			return "Drop "+itemname;
		case ActionButtonScript.buttonType.Throw:
			itemname = Toolbox.Instance.GetName(inventory.holding.gameObject);
			return "Throw "+itemname;
		case ActionButtonScript.buttonType.Stash:
			itemname = Toolbox.Instance.GetName(inventory.holding.gameObject);
			return "Put "+itemname+" in pocket";
		default:
			return "";
		}
	}
}