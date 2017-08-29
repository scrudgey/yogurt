using UnityEngine;

public class SpeechMenu : MonoBehaviour {
    public void Start(){
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
    }
    public void LineButton(){
        MessageSpeech message = new MessageSpeech();
        message.sayLine = true;
        Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
        Destroy(gameObject);
    }
    public void SwearButton(){
        MessageSpeech message = new MessageSpeech();
        message.randomSwear = true;
        Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
        Destroy(gameObject);
    }
    public void SwearAtButton(){
        Controller.Instance.currentSelect = Controller.SelectType.swearAt;
        Destroy(gameObject);
    }
    public void InsultButton(){
        Controller.Instance.currentSelect = Controller.SelectType.insultAt;
        Destroy(gameObject);
    }
    public void RandomButton(){
        MessageSpeech message = new MessageSpeech();
        message.randomSpeech = true;
        Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
        Destroy(gameObject);
    }
    public void CancelButton(){
        Destroy(gameObject);
    }
}
