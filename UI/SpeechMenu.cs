using UnityEngine;

public class SpeechMenu : MonoBehaviour {
    public void Start() {
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
    }
    public void LineButton() {
        MessageSpeech message = new MessageSpeech();
        message.sayLine = true;
        Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
        // Destroy(gameObject);
        UINew.Instance.CloseActiveMenu();
    }
    public void SwearButton() {
        MessageSpeech message = new MessageSpeech();
        message.randomSwear = true;
        Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
        // Destroy(gameObject);
        UINew.Instance.CloseActiveMenu();
    }
    public void SwearAtButton() {
        // Controller.Instance.currentSelect = Controller.SelectType.swearAt;
        Controller.Instance.state = Controller.ControlState.swearSelect;
        // Destroy(gameObject);
        UINew.Instance.CloseActiveMenu();
    }
    public void InsultButton() {
        // Controller.Instance.currentSelect = Controller.SelectType.insultAt;
        Controller.Instance.state = Controller.ControlState.insultSelect;
        // Destroy(gameObject);
        UINew.Instance.CloseActiveMenu();
    }
    public void RandomButton() {
        MessageSpeech message = new MessageSpeech();
        message.randomSpeech = true;
        Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
        // Destroy(gameObject);
        UINew.Instance.CloseActiveMenu();
    }
    public void CancelButton() {
        // Destroy(gameObject);
        UINew.Instance.CloseActiveMenu();
    }
}
