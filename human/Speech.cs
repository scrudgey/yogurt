using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Speech : Interactive {

	private string words;
	public bool speaking;
	public string[] randomPhrases;
	private List<string> queue = new List<string>();
	private float speakTime;
	private GameObject bubbleParent;
	private RectTransform bubble;
	private RectTransform textRect;
	private Text bubbleText;

	void Start () {
		Interaction speak = new Interaction(this, "Look", "Describe", true, false);
		speak.limitless = true;
		speak.dontWipeInterface = true;
		interactions.Add(speak);
		bubbleParent = gameObject.transform.FindChild("Speechbubble").gameObject;
		bubble = bubbleParent.GetComponent<RectTransform>();
		bubbleText = bubbleParent.transform.FindChild("Text").gameObject.GetComponent<Text>();
		textRect = bubbleText.GetComponent<RectTransform>();
	}

	public void Describe(Item obj){
		Say (obj.description);
	}

	void Update () {
		if (speakTime > 0){
			speakTime -= Time.deltaTime;
			speaking = true;
			bubbleParent.SetActive(true);
			bubbleText.text = words;
			textRect.sizeDelta = new Vector2(bubbleText.preferredWidth, 20);
			bubble.sizeDelta = new Vector2(bubbleText.preferredWidth / 100 + 0.1f, .20f);
			if (bubbleParent.transform.parent.transform.localScale.x < 0){
				Vector3 tempscale = bubbleParent.transform.localScale;
				tempscale.x = -1;
				bubbleParent.transform.localScale = tempscale;
			} else {
				bubbleParent.transform.localScale = Vector3.one;
			}
		}
		if (speakTime < 0){

			speaking = false;
			bubbleParent.SetActive(false);
			speakTime = 0;

			if (queue.Count > 0){
				words = queue[0];
				speakTime =  words.Length / 5;
				queue.RemoveAt(0);
				speaking = true;
				bubbleText.text = words;
				bubbleParent.SetActive(true);
			}

		}
	}

	public void SayRandom(){
		if(randomPhrases.Length > 0 ){
			string toSay = randomPhrases[Random.Range(0,randomPhrases.Length)];
			Say (toSay);
		}
	}

	public void Say(string phrase){

		if(speaking && phrase == words ){
			return;
		}

		words = phrase;
		speakTime = words.Length / 5;
	}


}
