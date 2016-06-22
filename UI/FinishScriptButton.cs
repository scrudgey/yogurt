using UnityEngine;

public class FinishScriptButton : MonoBehaviour {
	public void Clicked(){
		VideoCamera video = GameObject.FindObjectOfType<VideoCamera>();
		video.live = false;
		if (video){
			GameManager.Instance.EvaluateCommercial(video.commercial);
		}
		// GameManager.Instance.EvaluateCommercial();
	}
}
