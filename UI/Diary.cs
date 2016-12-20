using UnityEngine;
using UnityEngine.UI;

public class Diary : MonoBehaviour {
	public Text diaryText;
	public string loadDiaryName;
	public void Start(){
		diaryText = transform.Find("diaryPanel/diaryText").GetComponent<Text>();
		GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
		Time.timeScale = 0;
		if (loadDiaryName != null){
			TextAsset asset = Resources.Load("data/diaries/"+loadDiaryName) as TextAsset;
			diaryText.text = asset.text;
		}
	}
	public void OKButtonCallback(){
		Time.timeScale = 1;
		Destroy(gameObject);
	}
}
