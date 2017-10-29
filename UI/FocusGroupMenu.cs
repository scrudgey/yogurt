using UnityEngine;
using UnityEngine.UI;

public class FocusGroupMenu : MonoBehaviour {
	public Commercial commercial;
	public Text reviewText;
	public void Start(){
		Canvas canvas = GetComponent<Canvas>();
		canvas.worldCamera = GameManager.Instance.cam;
		reviewText = transform.Find("panel/textbox/Text").GetComponent<Text>();
		reviewText.text = commercial.DescribeEvent();
	}
	public void DoneButtonCallback(){
		Destroy(gameObject);
	}
}
