using UnityEngine;
public class SpeakButton : MonoBehaviour {
    
    public void SpeakButtonPressed(){
        GameObject test = GameObject.Find("DialogueMenu(Clone)");
        if (!test){
            GameObject obj = Instantiate(Resources.Load("UI/DialogueMenu")) as GameObject;
            obj.GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
        }
    }
}
