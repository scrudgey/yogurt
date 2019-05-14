using UnityEngine;
using UnityEngine.UI;

public class ActionButtonScript : MonoBehaviour {
    public enum buttonType {
        none,
        Drop,
        Throw,
        Stash,
        Inventory,
        Action,
        Punch
    }
    public buttonType bType = buttonType.Action;
    public Interaction action;
    public bool manualAction = false;
    public string itemName;
    public string buttonText;
    private bool mouseHeld;
    public Inventory inventory;
    public Button button;
    public void clicked() {
        Controller.Instance.ButtonClicked(this);
    }
    void Update() {
        if (mouseHeld && bType == buttonType.Action) {
            if (action.continuous && (Controller.Instance.InteractionIsWithinRange(action) || manualAction)) {
                action.DoAction();
            }
        } else {
            if (button == null)
                return;
            if (!manualAction && action != null) {
                if (Controller.Instance.InteractionIsWithinRange(action)) {
                    button.interactable = true;
                }
                if (!Controller.Instance.InteractionIsWithinRange(action)) {
                    button.interactable = false;
                }
            }
        }
    }
    public void MouseUp() {
        mouseHeld = false;
    }
    public void MouseDown() {
        mouseHeld = true;
    }
    public void MouseOver() {
        if (bType == buttonType.Action) {
            if (Controller.Instance.commandTarget != null) {
                string targetName = Toolbox.Instance.GetName(Controller.Instance.commandTarget);
                UINew.Instance.actionButtonText = "Command " + targetName + " to " + action.Description();
            } else {
                UINew.Instance.actionButtonText = action.Description();
            }
        } else {
            if (Controller.Instance.commandTarget != null) {
                string targetName = Toolbox.Instance.GetName(Controller.Instance.commandTarget);
                UINew.Instance.actionButtonText = "Command " + targetName + " to " + HandActionDescription();
            } else {
                UINew.Instance.actionButtonText = HandActionDescription();
            }
        }
    }
    public void HandAction() {
        switch (bType) {
            case buttonType.Drop:
                inventory.DropItem();
                UINew.Instance.ClearWorldButtons();
                break;

            case buttonType.Throw:
                inventory.ThrowItem();
                UINew.Instance.ClearWorldButtons();
                break;

            case buttonType.Stash:
                inventory.StashItem(inventory.holding.gameObject);
                UINew.Instance.ClearWorldButtons();
                UINew.Instance.UpdateInventoryButton(inventory);
                break;

            case buttonType.Punch:
                Controller.Instance.focus.ShootPressed();
                break;

            default:
                break;
        }
    }
    public string HandActionDescription() {
        if (bType == buttonType.Punch)
            return "Punch";
        string itemname = Toolbox.Instance.GetName(inventory.holding.gameObject);
        switch (bType) {
            case ActionButtonScript.buttonType.Drop:
                return "Drop " + itemname;
            case ActionButtonScript.buttonType.Throw:
                return "Throw " + itemname;
            case ActionButtonScript.buttonType.Stash:
                return "Put " + itemname + " in pocket";
            default:
                return "";
        }
    }
}