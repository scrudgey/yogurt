using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Speech : Interactive {
	private string words;
	public bool speaking;
	public string[] randomPhrases;
	private List<string> queue = new List<string>();
	private float speakTime;
	private GameObject bubbleParent;
    private GameObject flipper;
	private Text bubbleText;
    private ScriptReader reader;

	void Start () {
		Interaction speak = new Interaction(this, "Look", "Describe", true, false);
		speak.limitless = true;
		speak.dontWipeInterface = false;
		interactions.Add(speak);
        flipper = transform.FindChild("SpeechChild").gameObject;
		bubbleParent = transform.FindChild("SpeechChild/Speechbubble").gameObject;
		bubbleText = bubbleParent.transform.FindChild("Text").gameObject.GetComponent<Text>();
        reader = GetComponent<ScriptReader>();
	}

    // TODO: allow liquids and things to self-describe; add modifiers etc.
	public void Describe(Item obj){
        LiquidContainer container = obj.GetComponent<LiquidContainer>();
        MonoLiquid mono = obj.GetComponent<MonoLiquid>();
        if (container){
            if (container.amount > 0 && container.containerName != ""){
                Say("It's a "+container.containerName+" of "+container.liquid.name+".");
            } else {
                Say (obj.description);
            }
        } else if (mono){
            Say("It's "+mono.liquid.name+".");
        } else {
            Say (obj.description);
        }
	}

	void Update () {
		if (speakTime > 0){
			speakTime -= Time.deltaTime;
			speaking = true;
			bubbleParent.SetActive(true);
			bubbleText.text = words;
            // if the parent scale is flipped, we need to flip the flipper back to keep
            // the text properly oriented.
            if (flipper.transform.localScale != transform.localScale){
                Vector3 tempscale = transform.localScale;
                flipper.transform.localScale = tempscale; 
			}
		}
		if (speakTime < 0){
            if (speaking && reader){
                Debug.Log("speech callback");
                // do scriptreader callback
                reader.SpeechCallback();
            }
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
        Occurrence flag = Toolbox.Instance.OccurenceFlag(gameObject);
        OccurrenceSpeech data = new OccurrenceSpeech();
        data.speaker = gameObject;
        data.line = phrase;
        flag.data.Add(data);
        
		words = phrase;
		speakTime = words.Length / 5;
	}


}
