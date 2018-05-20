using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class DialogueNode {
    public List<string> text = new List<string>();
    public List<string> responses = new List<string>();
    public List<int> responseLinks = new List<int>();
}

public class Monologue {
    public Stack<string> text = new Stack<string>();
    public Speech speaker;
    public int index;
    private string currentString;
    public Monologue() { }
    public Monologue(Speech speaker, string[] texts) {
        index = 0;
        this.speaker = speaker;
        for (int i = texts.Length - 1; i >= 0; i--) {
            text.Push(texts[i]);
        }
    }
    public string GetString() {
        currentString = text.Peek().Substring(0, index);
        index += 1;
        return currentString;
        // return Toolbox.Instance.CloneRemover(speaker.name) + ": " + currentString;
    }
    public bool MoreToSay() {
        return currentString != text.Peek();
    }
    public void NextLine() {
        index = 0;
        text.Pop();
    }
    public void PlaySpeakSound(AudioSource audioSource) {
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
    public Trader targetTrade;
    private Controllable instigatorControl;
    private Controllable targetControl;
    public Image portrait1;
    public Image portrait2;
    public GameObject portraitContainer1;
    public GameObject portraitContainer2;
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
    public float blitInterval = 0.005f;
    public float blitTimer;
    public delegate void MyDelegate();
    public MyDelegate menuClosed;
    public bool configured;
    public int blitCounter;
    void Start() {
        if (configured)
            return;
        configured = true;
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        audioSource.spatialBlend = 0;
        portrait1 = transform.Find("base/dialogueElements/main/portrait1/Image").GetComponent<Image>();
        portrait2 = transform.Find("base/dialogueElements/main/portrait2/Image").GetComponent<Image>();
        portraitContainer1 = transform.Find("base/dialogueElements/main/portrait1").gameObject;
        portraitContainer2 = transform.Find("base/dialogueElements/main/portrait2").gameObject;

        speechText = transform.Find("base/dialogueElements/main/speechPanel/speechText").GetComponent<Text>();
        promptText = transform.Find("base/dialogueElements/main/speechPanel/textPrompt").GetComponent<Text>();
        choice1Text = transform.Find("base/dialogueElements/choicePanel/choice1/Text").GetComponent<Text>();
        choice2Text = transform.Find("base/dialogueElements/choicePanel/choice2/Text").GetComponent<Text>();
        choice3Text = transform.Find("base/dialogueElements/choicePanel/choice3/Text").GetComponent<Text>();
        choice1Object = transform.Find("base/dialogueElements/choicePanel/choice1/").gameObject;
        choice2Object = transform.Find("base/dialogueElements/choicePanel/choice2/").gameObject;
        choice3Object = transform.Find("base/dialogueElements/choicePanel/choice3/").gameObject;
        choicePanel = transform.Find("base/dialogueElements/choicePanel").gameObject;

        giveButton = transform.Find("base/buttons/Give").GetComponent<Button>();
        insultButton = transform.Find("base/buttons/Insult").GetComponent<Button>();
        threatenButton = transform.Find("base/buttons/Threaten").GetComponent<Button>();
        suggestButton = transform.Find("base/buttons/Suggest").GetComponent<Button>();
        followButton = transform.Find("base/buttons/Follow").GetComponent<Button>();
        endButton = transform.Find("base/buttons/End").GetComponent<Button>();
        buttons.AddRange(new Button[] { giveButton, insultButton, threatenButton, suggestButton, followButton, endButton });

        promptText.text = "";
        promptText.text = "[A]";
    }

    public void Configure(Speech instigator, Speech target, bool interruptDefault = false) {
        Start();
        this.instigator = instigator;
        this.target = target;
        instigator.inDialogue = true;
        target.inDialogue = true;

        instigatorInv = instigator.GetComponent<Inventory>();
        targetInv = target.GetComponent<Inventory>();
        targetAwareness = target.GetComponent<Awareness>();
        targetTrade = target.GetComponent<Trader>();
        if (instigatorInv == null || targetInv == null) {
            giveButton.interactable = false;
        }
        portrait2.sprite = target.portrait[0];
        portrait1.sprite = instigator.portrait[0];
        speechText.text = instigator.name + " " + target.name;
        targetControl = target.GetComponent<Controllable>();
        instigatorControl = instigator.GetComponent<Controllable>();
        instigatorControl.SetDirection(target.transform.position - instigator.transform.position);
        targetControl.SetDirection(instigator.transform.position - target.transform.position);
        if (targetControl)
            targetControl.disabled = true;
        if (instigatorControl)
            instigatorControl.disabled = true;
        if (interruptDefault) {
            choicePanel.SetActive(false);
            choice1Object.SetActive(false);
            choice2Object.SetActive(false);
            choice3Object.SetActive(false);
            return;
        }
        if (target.defaultMonologue != "") {
            LoadDialogueTree(target.defaultMonologue);
        }
    }
    public void LoadDialogueTree(string filename) {
        Regex node_hook = new Regex(@"^(\d)>(.+)", RegexOptions.Multiline);
        Regex response_hook = new Regex(@"^(\d)\)(.+)");
        TextAsset textData = Resources.Load("data/dialogue/" + filename) as TextAsset;
        DialogueNode newNode = null;
        foreach (string line in textData.text.Split('\n')) {
            if (node_hook.IsMatch(line)) {
                newNode = new DialogueNode();
                dialogueTree.Add(newNode);
                Match match = node_hook.Match(line);
                newNode.text.Add(match.Groups[2].Value);
                continue;
            }
            if (response_hook.IsMatch(line)) {
                Match match = response_hook.Match(line);
                newNode.responses.Add(match.Groups[2].Value);
                newNode.responseLinks.Add(int.Parse(match.Groups[1].Value));
                continue;
            }
            if (line != "") {
                newNode.text.Add(line);
            }
        }
        ParseNode(dialogueTree[0]);
    }
    public void ParseNode(DialogueNode node) {
        this.node = node;
        choicePanel.SetActive(false);
        choice1Object.SetActive(false);
        choice2Object.SetActive(false);
        choice3Object.SetActive(false);
        if (node.responses.Count > 0) {
            choice1Object.SetActive(true);
            choice1Text.text = node.responses[0];
        }
        if (node.responses.Count > 1) {
            choice2Object.SetActive(true);
            choice2Text.text = node.responses[1];
        }
        if (node.responses.Count > 2) {
            choice3Object.SetActive(true);
            choice3Text.text = node.responses[2];
        }
        Say(new Monologue(target, node.text.ToArray()));
    }
    public void ChoiceCallback(int choiceNumber) {
        Say(instigator, node.responses[choiceNumber - 1]);
        ParseNode(dialogueTree[node.responseLinks[choiceNumber - 1]]);
    }
    public void DisableResponses() {
        choice1Object.SetActive(false);
        choice2Object.SetActive(false);
        choice3Object.SetActive(false);
    }
    public void ActionCallback(string callType) {
        switch (callType) {
            case "end":
                UINew.Instance.CloseActiveMenu();
                break;
            case "insult":
                Say(instigator.InsultMonologue(target.gameObject, say: false));
                MessageInsult insult = new MessageInsult();
                Toolbox.Instance.SendMessage(target.gameObject, instigator, insult);
                Say(target.Riposte(say: false));
                DisableResponses();
                if (CutsceneManager.Instance.cutscene is CutsceneMayor) {
                    GameManager.Instance.data.achievementStats.mayorsSassed += 1;
                }
                break;
            case "threat":
                Say(instigator.ThreatMonologue(target.gameObject, say: false));
                MessageThreaten threat = new MessageThreaten();
                Toolbox.Instance.SendMessage(target.gameObject, instigator, threat);
                Say(target.RespondToThreat(say: false));
                DisableResponses();
                break;
            case "buy":
                AttemptTrade();
                break;
            case "command":
                Command();
                break;
            default:
                break;
        }
    }
    public void Command() {
        Controller.Instance.commandTarget = target.gameObject;
        Controller.Instance.state = Controller.ControlState.commandSelect;
        UINew.Instance.CloseActiveMenu();
    }
    public void CommandCallback(Interaction action) {
        Speech playerSpeech = GameManager.Instance.playerObject.GetComponent<Speech>();
        Speech targetSpeech = Controller.Instance.commandTarget.GetComponent<Speech>();
        Configure(playerSpeech, targetSpeech, interruptDefault: true);
        desire desireToAct = action.GetDesire(Controller.Instance.commandTarget, GameManager.Instance.playerObject);

        PromptCommand(action.Description());
        if (desireToAct == desire.decline){
            DeclineCommand();
            Controller.Instance.ResetCommandState();
        } else {
            AcceptCommand();
        }
    }
    void PromptCommand(string actionDescription){
        List<string> request = new List<string>();
        request.Add("Say, could you " + actionDescription + " for me?");
        Say(new Monologue(instigator, request.ToArray()));

        string[] ender = new string[] { "END" };
        Say(new Monologue(target, ender));
    }
    void DeclineCommand(){
        List<string> response = new List<string>();
        response.Add("I'd rather not.");
        Say(new Monologue(target, response.ToArray()));
    }
    void AcceptCommand(){
        List<string> response = new List<string>();
        response.Add("Why certainly my good man!");
        Say(new Monologue(target, response.ToArray()));
    }
    public void HandCommandCallback(ActionButtonScript.buttonType btype) {
        string act = "do this thing";

        //  Drop, Throw, Stash, Inventory, Action, Punch 
        switch (btype) {
            case ActionButtonScript.buttonType.Drop:
                act = "drop what you're holding";
                break;
            case ActionButtonScript.buttonType.Throw:
                act = "throw that";
                break;
            case ActionButtonScript.buttonType.Stash:
                act = "put that away";
                break;
            case ActionButtonScript.buttonType.Inventory:
                act = "open your inventory";
                break;
            case ActionButtonScript.buttonType.Punch:
                act = "throw a punch";
                break;
            default:
                break;
        }
        Speech playerSpeech = GameManager.Instance.playerObject.GetComponent<Speech>();
        Speech targetSpeech = Controller.Instance.commandTarget.GetComponent<Speech>();
        Configure(playerSpeech, targetSpeech, interruptDefault: true);

        List<string> request = new List<string>();
        request.Add("Say, could you " + act + " for me?");
        Say(new Monologue(instigator, request.ToArray()));

        string[] ender = new string[] { "END" };
        Say(new Monologue(target, ender));

        List<string> response = new List<string>();
        response.Add("why certainly my good man!");
        Say(new Monologue(target, response.ToArray()));
    }
    public void AttemptTrade() {
        switch (targetTrade.CheckTradeStatus(instigatorInv)) {
            // TODO: use nimrod for these responses
            case Trader.TradeStatus.noItemForTrade:
                Say(target, "I'm sorry, I'm fresh out.");
                break;
            case Trader.TradeStatus.wrongItemOffered:
                Say(target, "That's not what I want.");
                Say(target, "Give me one " + targetTrade.receive);
                break;
            case Trader.TradeStatus.noItemOffered:

                Say(target, "If you want this, give me one " + targetTrade.receive);
                break;
            case Trader.TradeStatus.pass:
                Say(target, "Pleasure doing business with you.");
                targetTrade.Exchange(instigatorInv);
                break;
            default:
                break;
        }
    }
    void OnDestroy() {
        instigator.inDialogue = false;
        target.inDialogue = false;
        if (targetControl)
            targetControl.disabled = false;
        if (instigatorControl)
            instigatorControl.disabled = false;
        if (menuClosed != null)
            menuClosed();
    }
    public void Say(Speech speaker, string text) {
        Monologue newLogue = new Monologue(speaker, new string[] { text });
        Say(newLogue);
    }
    public void Say(Monologue text) {
        if (monologue.text.Count == 0) {
            SetMonologue(text);
        } else {
            dialogue.Push(text);
        }
    }
    public void SetMonologue(Monologue newMonologue) {
        monologue = newMonologue;
        if (monologue.speaker == target) {
            portraitContainer1.SetActive(false);
            portraitContainer2.SetActive(true);
        } else if (monologue.speaker == instigator) {
            portraitContainer2.SetActive(false);
            portraitContainer1.SetActive(true);
        }
        // portrait1.sprite = monologue.speaker.portrait;
        if (monologue.text.Peek() == "END")
            UINew.Instance.CloseActiveMenu();
        // Destroy(gameObject);
    }
    public void EnableButtons() {
        foreach (Button button in buttons)
            button.interactable = true;
        if (instigatorInv == null || targetInv == null) {
            giveButton.gameObject.SetActive(false);
            // demandButton.gameObject.SetActive(false);
        }
        if (choice1Text.gameObject.activeSelf) {
            choicePanel.SetActive(true);
        }
    }
    public void DisableButtons() {
        foreach (Button button in buttons)
            button.interactable = false;
        choicePanel.SetActive(false);
    }
    public void Update() {
        blitTimer += Time.deltaTime;
        if (Input.GetKeyDown("a")) {
            if (waitForKeyPress) {
                waitForKeyPress = false;
                advancedKeyPressed = true;
                if (monologue.text.Count > 0) {
                    monologue.NextLine();
                } else if (dialogue.Count > 0) {
                    SetMonologue(dialogue.Pop());
                }
            }
        }
        if (Input.GetKey("a")) {
            if (!advancedKeyPressed)
                blitTimer = blitInterval;
        } else {
            advancedKeyPressed = false;
        }
        if (blitTimer < blitInterval) {
            return;
        }
        if (monologue.text.Count == 0) {
            if (dialogue.Count > 0 && !waitForKeyPress) {
                waitForKeyPress = true;
                if (dialogue.Peek().text.Peek() == "END") {
                    promptText.text = "[END]";
                } else {
                    promptText.text = "[MORE...]";
                }
            }
            return;
        }
        blitTimer = 0;
        if (monologue.MoreToSay()) {
            DisableButtons();
            speechText.text = monologue.GetString();
            blitCounter += 1;
            monologue.PlaySpeakSound(audioSource);
            promptText.text = "[A]";
        } else {
            if (monologue.text.Count > 1) {
                waitForKeyPress = true;
                promptText.text = "[MORE...]";
            } else {
                monologue.text.Pop();
                if (dialogue.Count == 0)
                    EnableButtons();
                promptText.text = "";
            }
        }
        if (blitCounter > 2) {
            if (instigator.portrait.Length > 1) {
                List<Sprite> unusedSprites = new List<Sprite>(instigator.portrait);
                unusedSprites.Remove(portrait1.sprite);
                portrait1.sprite = unusedSprites[Random.Range(0, unusedSprites.Count)];
            }
            if (target.portrait.Length > 1) {
                List<Sprite> unusedSprites = new List<Sprite>(target.portrait);
                unusedSprites.Remove(portrait2.sprite);
                portrait2.sprite = unusedSprites[Random.Range(0, unusedSprites.Count)];
            }
            blitCounter = 0;
        }
    }
}