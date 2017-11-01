using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FocusGroupMenu : MonoBehaviour {
	[System.Serializable]
	public class FocusGroupPersonality{
		public Sprite head_norm;
		public Sprite head_talking;
		public Sprite body;
	}
	public Commercial commercial;
	public Text reviewText;
	private int _index;
	public int index{
		get {return _index;}
		set {
			_index = value;
			if (_index > 2)
				_index = 0;
			IndexUpdate();
			}
	}
	public List<GameObject> bubbles = new List<GameObject>();
	public List<Image> heads = new List<Image>();
	public List<Image> bodies = new List<Image>();
	public List<bool> mouths = new List<bool>();
	public List<FocusGroupPersonality> personalities = new List<FocusGroupPersonality>();
	public float timer;
	public void Start(){
		mouths = new List<bool>(){false, false, false};

		Canvas canvas = GetComponent<Canvas>();
		canvas.worldCamera = GameManager.Instance.cam;
		reviewText = transform.Find("panel/textbox/Text").GetComponent<Text>();
		bubbles.Add(transform.Find("panel/graphic/person1/bubble").gameObject);
		bubbles.Add(transform.Find("panel/graphic/person2/bubble").gameObject);
		bubbles.Add(transform.Find("panel/graphic/person3/bubble").gameObject);

		heads.Add(transform.Find("panel/graphic/person1/head").GetComponent<Image>());
		heads.Add(transform.Find("panel/graphic/person2/head").GetComponent<Image>());
		heads.Add(transform.Find("panel/graphic/person3/head").GetComponent<Image>());

		bodies.Add(transform.Find("panel/graphic/person1").GetComponent<Image>());
		bodies.Add(transform.Find("panel/graphic/person2").GetComponent<Image>());
		bodies.Add(transform.Find("panel/graphic/person3").GetComponent<Image>());
		index = 0;
	}
	public void Update(){
		timer += Time.deltaTime;
		if (timer > 0.1f){
			mouths[index] = !mouths[index];
			if (mouths[index]){
				heads[index].sprite = personalities[index].head_talking;
			} else {
				heads[index].sprite = personalities[index].head_norm;
			}
			timer = 0f;
		}
	}
	public void IndexUpdate(){
		foreach(GameObject bubble in bubbles)
			bubble.SetActive(false);
		for (int i = 0; i < 3; i++){
			heads[i].sprite = personalities[i].head_norm;
		}
		// foreach(Image head in heads)
		// 	head.sprite = personalities[index].head_norm;
		bubbles[index].SetActive(true);
		mouths[index] = true;
		timer = 1f;
		reviewText.text = commercial.DescribeEvent(index);
	}
	public void DoneButtonCallback(){
		Destroy(gameObject);
	}
	public void NextButtonCallback(){
		index += 1;
		
	}
}
