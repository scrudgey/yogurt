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
    static private Regex name_hook = new Regex(@"\$name");
    static private Regex cosmic_name_hook = new Regex(@"\$cosmicName");
    public static string replaceHooks(string inString) {
        string line = name_hook.Replace(inString, GameManager.Instance.saveGameName);
        line = cosmic_name_hook.Replace(line, GameManager.Instance.data.cosmicName);
        return line;
    }
    public Monologue(Speech speaker, string[] texts) {
        index = 0;
        this.speaker = speaker;
        for (int i = texts.Length - 1; i >= 0; i--) {
            // string line = name_hook.Replace(texts[i], GameManager.Instance.saveGameName);
            // line = cosmic_name_hook.Replace(texts[i], GameManager.Instance.data.cosmicName);
            string line = replaceHooks(texts[i]);
            text.Push(line);
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
    private AnchoriteDance targetAnchoriteDance;
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
    public delegate void MyDelegate();
    public MyDelegate menuClosed;
    public bool configured;
    public int blitCounter;
    public bool cutsceneDialogue;
    public void Start() {
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

        speechText.text = "";
        promptText.text = "";
        promptText.text = "[A]";
        Canvas.ForceUpdateCanvases();
    }

    public void Configure(Speech instigator, Speech target, bool interruptDefault = false) {
        Start();
        this.instigator = instigator;
        this.target = target;
        target.gibberizer.StopPlay();
        instigator.gibberizer.StopPlay();
        instigator.inDialogue = true;
        target.inDialogue = true;
        giveButton.gameObject.SetActive(false);
        followButton.gameObject.SetActive(false);

        instigatorInv = instigator.GetComponent<Inventory>();
        targetInv = target.GetComponent<Inventory>();
        targetAwareness = target.GetComponent<Awareness>();
        targetTrade = target.GetComponent<Trader>();
        targetAnchoriteDance = target.GetComponent<AnchoriteDance>();
        if (targetAnchoriteDance)
            targetAnchoriteDance.StopDance();
        if (instigatorInv == null || targetInv == null) {
            giveButton.interactable = false;
        }
        portrait2.sprite = target.portrait[0];
        portrait1.sprite = instigator.portrait[0];
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
        if (target.hitState <= Controllable.HitState.stun) {
            if (target.defaultMonologue != "") {
                LoadDialogueTree(target.defaultMonologue);
            }
        } else {
            LoadDialogueTree("target_unresponsive");
        }
    }
    void OnDestroy() {
        if (monologue != null && monologue.speaker != null)
            monologue.speaker.gibberizer.StopPlay();
        instigator.inDialogue = false;
        target.inDialogue = false;
        if (targetControl)
            targetControl.disabled = false;
        if (instigatorControl)
            instigatorControl.disabled = false;
        if (targetAnchoriteDance)
            targetAnchoriteDance.StartDance();
        if (menuClosed != null)
            menuClosed();
    }
    public void LoadDialogueTree(string filename) {
        if (filename == "polestar_first") {
            cutsceneDialogue = true;
            EnableButtons();
        }

        Regex node_hook = new Regex(@"^(\d+)>(.+)", RegexOptions.Multiline);
        Regex response_hook = new Regex(@"^(\d+)\)(.+)");
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
                string response = Monologue.replaceHooks(match.Groups[2].Value);
                newNode.responses.Add(response);
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
                Say(instigator.InsultMonologue(target.gameObject));
                MessageInsult insult = new MessageInsult();
                Toolbox.Instance.SendMessage(target.gameObject, instigator, insult);
                Say(target.Riposte(say: false));
                DisableResponses();
                if (CutsceneManager.Instance.cutscene is CutsceneMayor) {
                    // GameManager.Instance.data.achievementStats.mayorsSassed += 1;
                    GameManager.Instance.IncrementStat(StatType.mayorsSassed, 1);
                }
                break;
            case "threat":
                Say(instigator.ThreatMonologue(target.gameObject));
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
        if (desireToAct == desire.decline) {
            DeclineCommand();
            Controller.Instance.ResetCommandState();
        } else {
            AcceptCommand();
        }
    }
    void PromptCommand(string actionDescription) {
        List<string> request = new List<string>();
        request.Add("Say, could you " + actionDescription + " for me?");
        Say(new Monologue(instigator, request.ToArray()));

        string[] ender = new string[] { "END" };
        Say(new Monologue(target, ender));
    }
    void DeclineCommand() {
        List<string> response = new List<string>();
        response.Add("I'd rather not.");
        Say(new Monologue(target, response.ToArray()));
    }
    void AcceptCommand() {
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
        if (monologue.text.Peek() == "END")
            UINew.Instance.CloseActiveMenu();
    }
    public void EnableButtons() {
        if (cutsceneDialogue) {
            foreach (Button button in buttons) {
                button.interactable = false;
                button.gameObject.SetActive(false);
            }
        } else {
            foreach (Button button in buttons)
                button.interactable = true;

            if (instigatorInv == null || targetInv == null) {
                giveButton.gameObject.SetActive(false);
                // demandButton.gameObject.SetActive(false);
            }
            if (CutsceneManager.Instance.cutscene != null) {
                if (CutsceneManager.Instance.cutscene.GetType() == typeof(CutsceneMayor)) {
                    suggestButton.GetComponent<Button>().interactable = false;
                }
            }
        }
        if (choice1Text.gameObject.activeSelf) {
            choicePanel.SetActive(true);
        }
        Canvas.ForceUpdateCanvases();
        // Debug.Break();
    }
    public void DisableButtons() {
        foreach (Button button in buttons)
            button.interactable = false;
        choicePanel.SetActive(false);
    }
    public void Update() {
        if (Input.GetKeyDown("a")) {
            if (waitForKeyPress) {
                waitForKeyPress = false;
                advancedKeyPressed = true;
                if (monologue.text.Count > 0) {
                    monologue.NextLine();
                    CheckForCommands(monologue.text.Peek());
                } else if (dialogue.Count > 0) {
                    SetMonologue(dialogue.Pop());
                    CheckForCommands(monologue.text.Peek());
                }
            }
        }

        advancedKeyPressed = false;
        if (monologue.text.Count == 0) {
            if (dialogue.Count > 0 && !waitForKeyPress) {
                waitForKeyPress = true;
                if (dialogue.Peek().text.Peek() == "END") {
                    promptText.text = "[END]";
                } else {
                    promptText.text = "[PRESS A]";
                }
            }
            return;
        }
        if (monologue.MoreToSay()) {
            DisableButtons();
            speechText.text = monologue.GetString();
            blitCounter += 1;
            monologue.speaker.gibberizer.StartPlay();
            promptText.text = "";
        } else {
            monologue.speaker.gibberizer.StopPlay();
            if (monologue.text.Count > 1) {
                waitForKeyPress = true;
                promptText.text = "[PRESS A]";
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

    public void CheckForCommands(string text) {
        if (text == "END")
            UINew.Instance.CloseActiveMenu();
        if (text == "POLESTARCALLBACK") {
            PoleStarCallback();
            monologue.NextLine();
            speechText.text = monologue.GetString();
        }
    }
    public void PoleStarCallback() {
        target.defaultMonologue = "polestar";
        GameManager.Instance.data.teleporterUnlocked = true;
        GameManager.Instance.data.cosmicName = GameManager.Instance.CosmicName();
    }
}