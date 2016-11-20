using UnityEngine;

public class Diary : MonoBehaviour {
	public void Start(){
		GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
		Time.timeScale = 0;
	}
	public void OKButtonCallback(){
		Time.timeScale = 1;
		Destroy(gameObject);
	}
}
