using UnityEngine;

public class SpeechMenu : MonoBehaviour {
    public GameObject detectiveButton;
    public void Start() {
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
        // enable / disable detective button
        detectiveButton.SetActive(GameManager.Instance.data.yogurtDetective);
    }
    public void SwearButton() {
        MessageSpeech message = new MessageSpeech("");
        message.randomSwear = true;
        Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
        UINew.Instance.CloseActiveMenu();
    }
    public void SwearAtButton() {
        UINew.Instance.CloseActiveMenu();
        InputController.Instance.state = InputController.ControlState.swearSelect;
    }
    public void InsultButton() {
        UINew.Instance.CloseActiveMenu();
        InputController.Instance.state = InputController.ControlState.insultSelect;
    }
    public void DetectButton() {
        UINew.Instance.CloseActiveMenu();
        InputController.Instance.state = InputController.ControlState.detectSelect;
    }
    // public void RandomButton() {
    //     MessageSpeech message = new MessageSpeech();
    //     // message.randomSpeech = true;
    //     // Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
    //     Awareness awareness = Controller.Instance.focus.GetComponent<Awareness>();
    //     if (awareness) {
    //         message.phrase = awareness.RecallMemory();
    //     } else {
    //         message.randomSpeech = true;
    //     }
    //     Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
    //     UINew.Instance.CloseActiveMenu();
    // }
    public void CancelButton() {
        UINew.Instance.CloseActiveMenu();
    }
}
