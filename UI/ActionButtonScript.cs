using UnityEngine;
using UnityEngine.UI;

public class ActionButtonScript : MonoBehaviour {
    public enum buttonType { none, Drop, Throw, Stash, Inventory, Action, Punch }
    public buttonType bType = buttonType.Action;
    public Interaction action;
    public bool manualAction = false;
    public string itemName;
    public string buttonText;
    private bool mouseHeld;
    public Inventory inventory;
    public Button button;
    public void clicked() {
        if (bType == buttonType.Action) {
            if (Controller.Instance.InteractionIsWithinRange(action) || manualAction) {
                // TODO: don't do action yet if current select is command
                if (Controller.Instance.state == Controller.ControlState.commandSelect) {
                    // callback a new dialogue menu
                    Controller.Instance.commandAct = action;
                    Controller.Instance.doCommand = true;
                    UINew.Instance.RefreshUI(active:true);
                    Controller.Instance.ResetLastLeftClicked();
                    GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.dialogue);
                    DialogueMenu dialogue = menuObject.GetComponent<DialogueMenu>();
                    dialogue.CommandCallback(action);
                } else {
                    action.DoAction();
                    if (!action.dontWipeInterface) {
                        UINew.Instance.RefreshUI(active:true);
                        Controller.Instance.ResetLastLeftClicked();
                    }
                }
            }
        } else {
            if (Controller.Instance.state == Controller.ControlState.commandSelect) {
                Controller.Instance.commandButtonType = bType;
                Controller.Instance.doCommand = true;
                UINew.Instance.RefreshUI(active:true);
                Controller.Instance.ResetLastLeftClicked();
                GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.dialogue);
                DialogueMenu dialogue = menuObject.GetComponent<DialogueMenu>();
                dialogue.HandCommandCallback(bType);
            } else {
                HandAction();
            }
            Controller.Instance.ResetLastLeftClicked();
        }
        UINew.Instance.SetActionText("");
        GUI.FocusControl("none");
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
                UINew.Instance.SetActionText("Command " + targetName + " to " + action.Description());
            } else {
                UINew.Instance.SetActionText(action.Description());
            }
        } else {
            if (Controller.Instance.commandTarget != null) {
                string targetName = Toolbox.Instance.GetName(Controller.Instance.commandTarget);
                UINew.Instance.SetActionText("Command " + targetName + " to " + HandActionDescription());
            } else {
                UINew.Instance.SetActionText(HandActionDescription());
            }
        }
    }
    public void MouseExit() {
        UINew.Instance.SetActionText("");
    }
    public void HandAction() {
        switch (bType) {
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
                UINew.Instance.UpdateInventoryButton(inventory);
                break;

            case ActionButtonScript.buttonType.Punch:
                Controller.Instance.focus.shootPressedFlag = true;
                break;

            default:
                break;
        }
        if (Controller.Instance.state == Controller.ControlState.commandSelect){
            UINew.Instance.RefreshUI(active:true);
            Controller.Instance.ResetLastLeftClicked();
            GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.dialogue);
            DialogueMenu dialogue = menuObject.GetComponent<DialogueMenu>();
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