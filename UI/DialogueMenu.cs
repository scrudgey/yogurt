using UnityEngine;
// using System.Collections;

public class DialogueMenu : MonoBehaviour {
    public void LineButton(){
        Controller.Instance.SayLine();
        Destroy(gameObject);
    }
    public void SwearButton(){
        Controller.Instance.Swear();
        Destroy(gameObject);
    }
    
    public void SwearAtButton(){
        Controller.Instance.currentSelect = Controller.SelectType.swearAt;
        Destroy(gameObject);
    }
    
    public void RandomButton(){
        Controller.Instance.SayRandom();
        Destroy(gameObject);
    }
    
    public void CancelButton(){
        Destroy(gameObject);
    }
    
}
