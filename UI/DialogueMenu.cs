using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DialogueNode{
	public List<string> text = new List<string>();
	public List<string> responses = new List<string>();
	public List<int> responseLinks = new List<int>();
}

public class Monologue{
	public Stack<string> text = new Stack<string>();
	public Speech speaker;
	public int index;
	private string currentString;
	public Monologue(){}
	public Monologue(Speech speaker, string[] texts){
		index = 0;
		this.speaker = speaker;
		for (int i = texts.Length - 1; i >= 0; i--)
		{
			text.Push(texts[i]);
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
	public void PlaySpeakSound(AudioSource audioSource){
		if (audioSource == null)
			return;
		if (speaker == null)
			return;
		if (speaker.speakSound)
			audioSource.PlayOneShot(speaker.speakSound);
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
	public GameObject choice1Object;
	public GameObject choice2Object;
	public GameObject choice3Object;
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
	public List<DialogueNode> dialogueTree = new List<DialogueNode>();
	public DialogueNode node;
	public int nextNode = -1;
	public bool waitForKeyPress;
	public bool advancedKeyPressed;
	public float blitInterval = 0.01f;
	public float blitTimer;

	public delegate void MyDelegate();
	public MyDelegate menuClosed;
	public bool configured;
	void Start () {
		if (configured)
			return;
		configured = true;
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
		audioSource.spatialBlend = 0;
		portrait = transform.Find("base/main/portrait").GetComponent<Image>();
		speechText = transform.Find("base/main/speechPanel/speechText").GetComponent<Text>();
		promptText = transform.Find("base/main/speechPanel/textPrompt").GetComponent<Text>();
		choice1Text = transform.Find("base/choicePanel/choice1/Text").GetComponent<Text>();
		choice2Text = transform.Find("base/choicePanel/choice2/Text").GetComponent<Text>();
		choice3Text = transform.Find("base/choicePanel/choice3/Text").GetComponent<Text>();
		choice1Object = transform.Find("base/choicePanel/choice1/").gameObject;
		choice2Object = transform.Find("base/choicePanel/choice2/").gameObject;
		choice3Object = transform.Find("base/choicePanel/choice3/").gameObject;
		choicePanel = transform.Find("base/choicePanel").gameObject;
		
		giveButton = transform.Find("base/buttons/Give").GetComponent<Button>();
		demandButton = transform.Find("base/buttons/Demand").GetComponent<Button>();
		insultButton = transform.Find("base/buttons/Insult").GetComponent<Button>();
		threatenButton = transform.Find("base/buttons/Threaten").GetComponent<Button>();
		suggestButton = transform.Find("base/buttons/Suggest").GetComponent<Button>();
		followButton = transform.Find("base/buttons/Follow").GetComponent<Button>();
		endButton = transform.Find("base/buttons/End").GetComponent<Button>();
		buttons.AddRange(new Button[]{giveButton, demandButton, insultButton, threatenButton, suggestButton, followButton, endButton});

		promptText.text = "";
		choicePanel.SetActive(false);
		choice1Object.SetActive(false);
		choice2Object.SetActive(false);
		choice3Object.SetActive(false);
	}

	public void Configure(Speech instigator, Speech target){
		Start();
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
		instigatorControl.SetDirection(target.transform.position - instigator.transform.position);
		targetControl.SetDirection(instigator.transform.position - target.transform.position);
		if (targetControl)
			targetControl.disabled = true;
		if (instigatorControl)
			instigatorControl.disabled = true;
	}
	public void LoadDialogueTree(string filename){
		Regex node_hook = new Regex(@"^(\d)>(.+)", RegexOptions.Multiline);
		Regex response_hook = new Regex(@"^(\d)\)(.+)");
		TextAsset textData = Resources.Load("data/dialogue/"+filename) as TextAsset;
		DialogueNode newNode = null;
		foreach (string line in textData.text.Split('\n')){
			if (node_hook.IsMatch(line)){
				newNode = new DialogueNode();
				dialogueTree.Add(newNode);
				Match match = node_hook.Match(line);
				newNode.text.Add(match.Groups[2].Value);
				continue;
			}
			if (response_hook.IsMatch(line)){
				Match match = response_hook.Match(line);
				newNode.responses.Add(match.Groups[2].Value);
				newNode.responseLinks.Add(int.Parse(match.Groups[1].Value));
				continue;
			}
			if (line != ""){
				newNode.text.Add(line);
			}
		}
		ParseNode(dialogueTree[0]);
	}
	public void ParseNode(DialogueNode node){
		this.node = node;
		choicePanel.SetActive(false);
		choice1Object.SetActive(false);
		choice2Object.SetActive(false);
		choice3Object.SetActive(false);
		if (node.responses.Count > 0){
			// choicePanel.SetActive(true);
			choice1Object.SetActive(true);
			choice1Text.text = node.responses[0];
		}
		if (node.responses.Count > 1){
			choice2Object.SetActive(true);
			choice2Text.text = node.responses[1];
		}
		if (node.responses.Count > 2){
			choice3Object.SetActive(true);
			choice3Text.text = node.responses[2];
		}
		Say(new Monologue(target, node.text.ToArray()));
	}

	public void ChoiceCallback(int choiceNumber){
		Say(instigator, node.responses[choiceNumber - 1]);
		ParseNode(dialogueTree[node.responseLinks[choiceNumber - 1]]);
	}
	public void DisableResponses(){
		choice1Object.SetActive(false);
		choice2Object.SetActive(false);
		choice3Object.SetActive(false);
	}
	public void ActionCallback(string callType){
		switch (callType){
			case "end":
			Destroy(gameObject);
			break;
			case "insult":
			Say(instigator.Insult(target.gameObject));
			MessageInsult insult = new MessageInsult();
			Toolbox.Instance.SendMessage(target.gameObject, instigator, insult);
			Say(target.Riposte());
			DisableResponses();
			break;
			case "threat":
			Say(instigator.Threaten(target.gameObject));
			MessageThreaten threat = new MessageThreaten();
			Toolbox.Instance.SendMessage(target.gameObject, instigator, threat);
			Say(target.RespondToThreat());
			DisableResponses();
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
		if (menuClosed != null)
			menuClosed();
	}

	public void Say(Speech speaker, string text){
		Monologue newLogue = new Monologue(speaker, new string[]{text});
		Say(newLogue);
	}
	public void Say(Monologue text){
		if (monologue.text.Count == 0){
			SetMonologue(text);
		} else {
			dialogue.Push(text);
		}
	}
	public void SetMonologue(Monologue newMonologue){
		monologue = newMonologue;
		portrait.sprite = monologue.speaker.portrait;
		if (monologue.text.Peek() == "END")
			Destroy(gameObject);
	}
	public void EnableButtons(){
		foreach(Button button in buttons)
			button.interactable = true;
		if (choice1Text.gameObject.activeSelf){
			choicePanel.SetActive(true);
		}
	}
	public void DisableButtons(){
		foreach(Button button in buttons)
			button.interactable = false;
		choicePanel.SetActive(false);
	}

	public void Update(){
		blitTimer += Time.deltaTime;
		if (Input.GetKeyDown("a")){
			if (waitForKeyPress){
				waitForKeyPress = false;
				advancedKeyPressed = true;
				if (monologue.text.Count > 0){
					monologue.NextLine();
				} else if (dialogue.Count > 0){
					SetMonologue(dialogue.Pop());
				}
				promptText.text = "";
			}
		}
		if (Input.GetKey("a")){
			if (!advancedKeyPressed)
				blitTimer = blitInterval;
		} else {
			advancedKeyPressed = false;
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
			monologue.PlaySpeakSound(audioSource);
		} else {
			if (monologue.text.Count > 1){
				waitForKeyPress = true;
				promptText.text = "[MORE...]";
			} else {
				monologue.text.Pop();
				if (dialogue.Count == 0)
					EnableButtons();
			}
		}
	}
}
