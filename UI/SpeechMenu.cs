using UnityEngine;

public class SpeechMenu : MonoBehaviour {
    public void Start() {
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
    }
    public void SwearButton() {
        MessageSpeech message = new MessageSpeech();
        message.randomSwear = true;
        Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
        UINew.Instance.CloseActiveMenu();
    }
    public void SwearAtButton() {
        UINew.Instance.CloseActiveMenu();
        Controller.Instance.state = Controller.ControlState.swearSelect;
    }
    public void InsultButton() {
        UINew.Instance.CloseActiveMenu();
        Controller.Instance.state = Controller.ControlState.insultSelect;
    }
    public void RandomButton() {
        MessageSpeech message = new MessageSpeech();
        // message.randomSpeech = true;
        // Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
        Awareness awareness = Controller.Instance.focus.GetComponent<Awareness>();
        if (awareness){
            message.phrase = awareness.RecallMemory();
        } else {
            message.randomSpeech = true;
        }
        Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
        UINew.Instance.CloseActiveMenu();
    }
    public void CancelButton() {
        UINew.Instance.CloseActiveMenu();
    }
}
