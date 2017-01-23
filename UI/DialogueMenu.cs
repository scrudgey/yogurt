using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Monologue{
	public Stack<string> text = new Stack<string>();
	public Speech speaker;
	public int index;
	private string currentString;
	public Monologue(){}
	public Monologue(Speech speaker, string[] texts){
		index = 0;
		this.speaker = speaker;
		foreach(string entry in texts){
			text.Push(entry);
		}
	}
	public string GetString(){
		currentString = text.Peek().Substring(0, index);
		index += 1;
		return Toolbox.Instance.CloneRemover(speaker.name) + ": " + currentString;
	}
	public bool MoreToSay(){
		return currentString != text.Peek();
	}
	public void NextLine(){
		index = 0;
		text.Pop();
	}
}

public class DialogueMenu : MonoBehaviour {
	private AudioSource audioSource;
	public Speech instigator;
	public Speech target;

	public Inventory instigatorInv;
	public Inventory targetInv;
	public Awareness targetAwareness;
	private Controllable instigatorControl;
	private Controllable targetControl;

	public Image portrait;
	public Text speechText;
	public Text choice1Text;
	public Text choice2Text;
	public Text choice3Text;
	public Text promptText;
	public GameObject choicePanel;
	public Button giveButton;
	public Button demandButton;
	public Button insultButton;
	public Button threatenButton;
	public Button suggestButton;
	public Button followButton;
	public Button endButton;
	private List<Button> buttons = new List<Button>();

	public Monologue monologue = new Monologue();
	public Stack<Monologue> dialogue = new Stack<Monologue>();
	public bool waitForKeyPress;
	public float blitInterval = 0.01f;
	public float blitTimer;

	public delegate void MyDelegate();
	public MyDelegate menuClosed;
	void Start () {
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
		portrait = transform.Find("base/main/portrait").GetComponent<Image>();
		speechText = transform.Find("base/main/speechPanel/speechText").GetComponent<Text>();
		promptText = transform.Find("base/main/speechPanel/textPrompt").GetComponent<Text>();
		choice1Text = transform.Find("base/choicePanel/choice1/Text").GetComponent<Text>();
		choice2Text = transform.Find("base/choicePanel/choice2/Text").GetComponent<Text>();
		choice3Text = transform.Find("base/choicePanel/choice3/Text").GetComponent<Text>();
		choicePanel = transform.Find("base/choicePanel").gameObject;
		
		giveButton = transform.Find("base/buttons/Give").GetComponent<Button>();
		demandButton = transform.Find("base/buttons/Demand").GetComponent<Button>();
		insultButton = transform.Find("base/buttons/Insult").GetComponent<Button>();
		threatenButton = transform.Find("base/buttons/Threaten").GetComponent<Button>();
		suggestButton = transform.Find("base/buttons/Suggest").GetComponent<Button>();
		followButton = transform.Find("base/buttons/Follow").GetComponent<Button>();
		endButton = transform.Find("base/buttons/End").GetComponent<Button>();
		buttons.AddRange(new Button[]{giveButton, demandButton, insultButton, threatenButton, suggestButton, followButton, endButton});


		// Controller.Instance.suspendInput = true;
		promptText.text = "";
		choicePanel.SetActive(false);
	}

	public void Configure(Speech instigator, Speech target){
		Start();
		// GameObject giveButton = transform.Find("base/buttons/Give").gameObject;
		// GameObject demandButton = transform.Find("base/buttons/Demand").gameObject;
		this.instigator = instigator;
		this.target = target;
		instigatorInv = instigator.GetComponent<Inventory>();
		targetInv = target.GetComponent<Inventory>();
		targetAwareness = target.GetComponent<Awareness>();
		if (instigatorInv == null || targetInv == null){
			giveButton.gameObject.SetActive(false);
			demandButton.gameObject.SetActive(false);
		}
		speechText.text = instigator.name + " " + target.name;
		targetControl = target.GetComponent<Controllable>();
		instigatorControl = instigator.GetComponent<Controllable>();
		if (targetControl)
			targetControl.disabled = true;
		if (instigatorControl)
			instigatorControl.disabled = true;
	}

	public void ChoiceCallback(int choiceNumber){
		Say(instigator, "choice number "+choiceNumber.ToString());
	}
	public void ActionCallback(string callType){
		switch (callType){
			case "end":
			Destroy(gameObject);
			// Controller.Instance.suspendInput = false;
			if (targetControl)
				targetControl.disabled = false;
			if (instigatorControl)
				instigatorControl.disabled = false;
			break;
			case "insult":
			Say(instigator.Insult(target.gameObject));
			MessageInsult insult = new MessageInsult();
			Toolbox.Instance.SendMessage(target.gameObject, instigator, insult);
			Say(target.Riposte());
			break;
			case "threat":
			Say(instigator.Threaten(target.gameObject));
			MessageThreaten threat = new MessageThreaten();
			Toolbox.Instance.SendMessage(target.gameObject, instigator, threat);
			Say(target.RespondToThreat());
			break;
			default:
			break;
		}
	}
	void OnDestroy(){
		if (targetControl)
			targetControl.disabled = false;
		if (instigatorControl)
			instigatorControl.disabled = false;
		menuClosed();
	}

	public void Say(Speech speaker, string text){
		Monologue newLogue = new Monologue(speaker, new string[]{text});
		Say(newLogue);
	}
	public void Say(Monologue text){
		if (monologue.text.Count == 0){
			monologue = text;
		} else {
			dialogue.Push(text);
		}
	}
	public void EnableButtons(){
		foreach(Button button in buttons)
			button.interactable = true;
	}
	public void DisableButtons(){
		foreach(Button button in buttons)
			button.interactable = false;
	}

	public void Update(){
		blitTimer += Time.deltaTime;
		if (Input.GetKeyDown("a")){
			if (waitForKeyPress){
				waitForKeyPress = false;
				if (monologue.text.Count > 0){
					monologue.NextLine();
				} else if (dialogue.Count > 0){
					monologue = dialogue.Pop();
				}
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
		if (monologue.MoreToSay()){
			DisableButtons();
			speechText.text = monologue.GetString();
			if (monologue.speaker != null){
				if (monologue.speaker.speakSound)
					audioSource.PlayOneShot(monologue.speaker.speakSound);
			}
		} else {
			if (monologue.text.Count > 1){
				waitForKeyPress = true;
				promptText.text = "[MORE...]";
				// EnableButtons();
			} else {
				monologue.text.Pop();
				if (dialogue.Count == 0)
					EnableButtons();
			}
		}
	}
}
