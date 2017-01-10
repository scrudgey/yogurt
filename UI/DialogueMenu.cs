using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
	
	public Stack<string> monologue = new Stack<string>();
	public Stack<Stack<string>> dialogue = new Stack<Stack<string>>();
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
		choice1Text = transform.Find("base/main/feedback/choicePanel/choice1/Text").GetComponent<Text>();
		choice2Text = transform.Find("base/main/feedback/choicePanel/choice2/Text").GetComponent<Text>();
		choice3Text = transform.Find("base/main/feedback/choicePanel/choice3/Text").GetComponent<Text>();
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
			break;
			case "insult":
			Stack<string> newStack = new Stack<string>();
			newStack.Push("thou insolent rogue!");
			newStack.Push("thou churlish knave!");
			Say(instigator, newStack);
			PersonalAssessment assessment = targetAwareness.FormPersonalAssessment(instigator);
			assessment.status = PersonalAssessment.friendStatus.enemy;
			break;
			default:
			break;
		}
	}

	public void Say(GameObject speaker, string text){
		Stack<string> newStack = new Stack<string>();
		newStack.Push(text);
		Say(speaker, newStack);
	}
	public void Say(GameObject speaker, Stack<string> text){
		this.speaker = speaker;
		if (monologue.Count == 0){
			monologue = text;
			index = 0;
		} else {
			dialogue.Push(text);
		}
	}

	public void Update(){
		blitTimer += Time.deltaTime;
		if (Input.GetKeyDown("a")){
			if (waitForKeyPress){
				waitForKeyPress = false;
				if (monologue.Count > 0){
					monologue.Pop();
				} else if (dialogue.Count > 0){
					monologue = dialogue.Pop();
				}
				index = 0;
				promptText.text = "";
			}
		}
		if (Input.GetKey("a")){
			blitTimer = blitInterval;
		}
		if (blitTimer < blitInterval){
			return;
		}
		if (monologue.Count == 0){
			if (dialogue.Count > 0){
				waitForKeyPress = true;
				promptText.text = "[MORE...]";
			}
			return;
		}
		blitTimer = 0;
		if (currentString != monologue.Peek()){
			currentString = monologue.Peek().Substring(0, index);
			index += 1;
			speechText.text = speaker.name + ": " + currentString;
			if (blitSound != null){
				audioSource.PlayOneShot(blitSound);
			}
		} else {
			if (monologue.Count > 1){
				waitForKeyPress = true;
				promptText.text = "[MORE...]";
			} else {
				monologue.Pop();
			}
		}
	}


}
