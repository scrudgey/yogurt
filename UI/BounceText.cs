using UnityEngine;
using UnityEngine.UI;
// using System.Collections;

public class BounceText : MonoBehaviour {

	public GameObject target;
	private Text textObject;
	public string text = "Success!";
	private FollowGameObjectInCamera followScript;
	public float tickTime = 0.1f;
	public float lifeTime = 1f;
	public float timer;
	private int frame = 0;
	private RectTransform trans;
	private Vector3 tempScale;
	
	void Start () {
		followScript = GetComponent<FollowGameObjectInCamera>();
		if (target)
			followScript.target = target;
		trans = transform.FindChild("Text").GetComponent<RectTransform>();
		textObject = transform.FindChild("Text").GetComponent<Text>();
		textObject.text = text;
		
		tempScale = Vector3.one;
	}
	void Update () {
		timer += Time.deltaTime;
		if (timer > tickTime){
			timer = 0f;
			NextFrame();
		}
		if (frame * tickTime > lifeTime){
			Destroy(gameObject);
		}
	}
	
	void NextFrame(){
		frame += 1;
		tempScale = Vector3.one;
		switch(frame){
			case 0:
			tempScale.x = 0.5f;
			tempScale.y = 0.5f;
			break;
			case 1:
			tempScale.x = 1.5f;
			tempScale.y = 0.2f;
			break;
			case 2:
			tempScale.x = 0.2f;
			tempScale.y = 1.5f;
			break;
			case 3:
			tempScale.x = 1.2f;
			tempScale.y = 1.2f;
			break;
			default:
			tempScale = Vector3.one;
			break;
		}
		if (trans.localScale != tempScale)
			trans.localScale = tempScale;
	}
}
