using UnityEngine;
public class Computer : Item {
    public AnimateUIBubble newBubble;
    void Start() {
        Interaction emailInteraction = new Interaction(this, "Email", "OpenEmail");
        emailInteraction.descString = "Check email";
        interactions.Add(emailInteraction);
        if (newBubble == null)
            newBubble = GetComponent<AnimateUIBubble>();
        newBubble.DisableFrames();
        CheckBubble();
    }
    public void CheckBubble() {
        bool activeBubble = false;
        if (GameManager.Instance.data == null)
            return;
        foreach (Email email in GameManager.Instance.data.emails) {
            if (email.read == false)
                activeBubble = true;
        }
        if (activeBubble) {
            newBubble.EnableFrames();
        } else {
            newBubble.DisableFrames();
        }
    }
    public void OpenEmail() {
        if (InputController.Instance.state != InputController.ControlState.cutscene &&
            InputController.Instance.state != InputController.ControlState.inMenu &&
            InputController.Instance.state != InputController.ControlState.waitForMenu
            ) {
            GameObject menu = UINew.Instance.ShowMenu(UINew.MenuType.email);
            menu.GetComponent<EmailUI>().computer = this;
        }
    }
}
