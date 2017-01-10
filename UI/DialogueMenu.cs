using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Monologue{
	public Stack<string> text = new Stack<string>();
	public GameObject speaker;

	public Monologue(){}
	public Monologue(GameObject speaker, string[] texts){
		this.speaker = speaker;
		foreach(string entry in texts){
			text.Push(entry);
		}
	}
}

public class DialogueMenu : MonoBehaviour {
	private AudioSource audioSource;
	public GameObject instigator;
	public GameObject target;
	public GameObject speaker;

	public Inventory instigatorInv;
	public Inventory targetInv;
	public Awareness targetAwareness;

	public Image portrait;
	public Text speechText;
	public Text choice1Text;
	public Text choice2Text;
	public Text choice3Text;
	public Text promptText;
	public Monologue monologue = new Monologue();
	public Stack<Monologue> dialogue = new Stack<Monologue>();
	public string currentString;
	public bool waitForKeyPress;
	public float blitInterval = 0.1f;
	public float blitTimer;
	public int index;
	public AudioClip blitSound;
	void Start () {
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
		portrait = transform.Find("base/main/feedback/portraitPanel/portrait").GetComponent<Image>();
		speechText = transform.Find("base/main/feedback/portraitPanel/speechPanel/speechText").GetComponent<Text>();
		promptText = transform.Find("base/main/feedback/portraitPanel/speechPanel/textPrompt").GetComponent<Text>();
		choice1Text = transform.Find("base/choicePanel/choice1/Text").GetComponent<Text>();
		choice2Text = transform.Find("base/choicePanel/choice2/Text").GetComponent<Text>();
		choice3Text = transform.Find("base/choicePanel/choice3/Text").GetComponent<Text>();
		Controller.Instance.suspendInput = true;
	}

	public void Configure(GameObject instigator, GameObject target){
		Start();
		GameObject giveButton = transform.Find("base/main/buttons/Give").gameObject;
		GameObject demandButton = transform.Find("base/main/buttons/Demand").gameObject;
		// GameObject insultButton = transform.Find("base/main/buttons/Insult").gameObject;
		// GameObject suggestButton = transform.Find("base/main/buttons/Suggest").gameObject;
		// GameObject followButton = transform.Find("base/main/buttons/Follow").gameObject;
		this.instigator = instigator;
		this.target = target;
		instigatorInv = instigator.GetComponent<Inventory>();
		targetInv = target.GetComponent<Inventory>();
		targetAwareness = target.GetComponent<Awareness>();
		if (instigatorInv == null || targetInv == null){
			giveButton.SetActive(false);
			demandButton.SetActive(false);
		}
		speechText.text = instigator.name + " " + target.name;
	}

	public void ChoiceCallback(int choiceNumber){
		Say(gameObject, "choice number "+choiceNumber.ToString());
	}
	public void ActionCallback(string callType){
		switch (callType){
			case "end":
			Destroy(gameObject);
			Controller.Instance.suspendInput = false;
			break;
			case "insult":
			Monologue newLogue = new Monologue(instigator, new string[]{"thou insolent rogue!", "thou churlish knave!"});
			Say(newLogue);
			targetAwareness.Insulted(instigator, this);
			break;
			default:
			break;
		}
	}

	public void Say(GameObject speaker, string text){
		Monologue newLogue = new Monologue(speaker, new string[]{text});
		Say(newLogue);
	}
	public void Say(Monologue text){
		if (monologue.text.Count == 0){
			monologue = text;
			index = 0;
			speaker = text.speaker;
			Debug.Log(speaker);
		} else {
			dialogue.Push(text);
		}
	}

	public void Update(){
		blitTimer += Time.deltaTime;
		if (Input.GetKeyDown("a")){
			if (waitForKeyPress){
				waitForKeyPress = false;
				if (monologue.text.Count > 0){
					monologue.text.Pop();
				} else if (dialogue.Count > 0){
					monologue = dialogue.Pop();
				}
				index = 0;
				speaker = monologue.speaker;
				promptText.text = "";
			}
		}
		if (Input.GetKey("a")){
			blitTimer = blitInterval;
		}
		if (blitTimer < blitInterval){
			return;
		}
		if (monologue.text.Count == 0){
			if (dialogue.Count > 0 && !waitForKeyPress){
				waitForKeyPress = true;
				promptText.text = "[MORE...]";
			}
			return;
		}
		blitTimer = 0;
		if (currentString != monologue.text.Peek()){
			currentString = monologue.text.Peek().Substring(0, index);
			index += 1;
			speechText.text = speaker.name + ": " + currentString;
			if (blitSound != null){
				audioSource.PlayOneShot(blitSound);
			}
		} else {
			if (monologue.text.Count > 1){
				waitForKeyPress = true;
				promptText.text = "[MORE...]";
			} else {
				monologue.text.Pop();
			}
		}
	}


}
