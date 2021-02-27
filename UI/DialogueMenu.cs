using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

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
    static private Regex either_name_hook = new Regex(@"\$eitherName");
    public static string replaceHooks(string inString) {
        string line = name_hook.Replace(inString, GameManager.Instance.saveGameName);
        line = cosmic_name_hook.Replace(line, GameManager.Instance.data.cosmicName);

        string bestName = GameManager.Instance.saveGameName;
        if (GameManager.Instance.data.cosmicName != "") {
            bestName = GameManager.Instance.data.cosmicName;
        }
        line = either_name_hook.Replace(line, bestName);

        List<bool> swearList = new List<bool>();
        line = Speech.ProcessDialogue(line, ref swearList).phrase;
        return line;
    }
    public Monologue(Speech speaker, string[] texts) {
        index = 0;
        this.speaker = speaker;
        for (int i = texts.Length - 1; i >= 0; i--) {
            // string line = name_hook.Replace(texts[i], GameManager.Instance.saveGameName);
            // line = cosmic_name_hook.Replace(texts[i], GameManager.Instance.data.cosmicName);
            string line = replaceHooks(texts[i]);
            line = speaker.grammar.Parse(line);
            text.Push(line);
        }
    }
    public string GetString() {
        currentString = text.Peek().Substring(0, index);
        if (index > 0)
            if (currentString.Substring(index - 1, 1) == "<") {
                //<sprite=0>
                index += 9;
                currentString = text.Peek().Substring(0, index);
            }

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

public partial class DialogueMenu : MonoBehaviour {
    public enum TextSize { normal, large };
    private AudioSource audioSource;
    public Speech instigator;
    public Speech target;
    public Inventory instigatorInv;
    public Inventory targetInv;
    public Awareness targetAwareness;
    public Trader targetTrade;
    private Controllable instigatorControl;
    private Controllable targetControl;
    // private AnchoriteDance targetAnchoriteDance;
    public Image portrait1;
    public Image portrait2;
    public GameObject portraitContainer1;
    public GameObject portraitContainer2;
    public TextMeshProUGUI speechText;
    public Text largeText;
    public TextSize textSize {
        get { return _activeText; }
        set {
            _activeText = value;
            UpdateActiveText();
        }
    }
    private TextSize _activeText;
    public TextMeshProUGUI choice1Text;
    public TextMeshProUGUI choice2Text;
    public TextMeshProUGUI choice3Text;
    public GameObject choice1Object;
    public GameObject choice2Object;
    public GameObject choice3Object;
    public TextMeshProUGUI promptText;
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
    public bool disableCommand;
    private bool doTrapDoor;
    private bool doCEOSequence;
    private bool doVampireAttack;
    public bool keypressedThisFrame;
    static List<string> cutsceneDialogues = new List<string>{
        "polestar_first", "vampire", "dancing_god", "dancing_god_bless", "dancing_god_destroy", "magician", "magician2", "magician3"
    };
    void Awake() {
        keypressedThisFrame = false;
        InputController.Instance.PrimaryAction.action.performed += ctx => {
            keypressedThisFrame = ctx.ReadValueAsButton();
        };
        InputController.Instance.PrimaryAction.action.Enable();
    }
    // void OnEnable() {
    //     InputController.Instance.PrimaryAction.action.Enable();
    // }

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

        speechText = transform.Find("base/dialogueElements/main/speechPanel/speechText").GetComponent<TextMeshProUGUI>();
        largeText = transform.Find("base/dialogueElements/main/speechPanel/largeSpeechText").GetComponent<Text>();

        promptText = transform.Find("base/dialogueElements/main/speechPanel/textPrompt").GetComponent<TextMeshProUGUI>();
        choice1Text = transform.Find("base/dialogueElements/choicePanel/choice1/Text").GetComponent<TextMeshProUGUI>();
        choice2Text = transform.Find("base/dialogueElements/choicePanel/choice2/Text").GetComponent<TextMeshProUGUI>();
        choice3Text = transform.Find("base/dialogueElements/choicePanel/choice3/Text").GetComponent<TextMeshProUGUI>();
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

        largeText.gameObject.SetActive(false);
        SetText(newText: "");
        promptText.text = "";
        promptText.text = "[A]";
        Canvas.ForceUpdateCanvases();
    }
    public void SetText(string newText = "DEFAULT") {
        if (newText == "DEFAULT") {
            List<bool> swearList = new List<bool>();
            MessagePhrase text = Speech.ProcessDialogue(monologue.GetString(), ref swearList);
            speechText.text = text.phrase;
            largeText.text = monologue.text.Peek();
        } else {
            speechText.text = newText;
            largeText.text = newText;
        }
    }

    public void Configure(Speech instigator, Speech target, bool interruptDefault = false, string dialogue = null, bool recuit = false) {
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
        // targetAnchoriteDance = target.GetComponent<AnchoriteDance>();
        // if (targetAnchoriteDance)
        //     targetAnchoriteDance.StopDance();
        if (instigatorInv == null || targetInv == null) {
            giveButton.interactable = false;
        }
        portrait2.sprite = target.portrait[0];
        portrait1.sprite = instigator.portrait[0];
        targetControl = target.GetComponent<Controllable>();
        instigatorControl = instigator.GetComponent<Controllable>();
        using (Controller controller = new Controller(targetControl)) {
            controller.SetDirection(instigator.transform.position - target.transform.position);
        }
        using (Controller controller = new Controller(instigatorControl)) {
            controller.SetDirection(target.transform.position - instigator.transform.position);
        }

        if (targetControl) {
            targetControl.disabled = true;
        }
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
            if (dialogue != null) {
                LoadDialogueTree(dialogue);
            } else if (target.onCamera && target.cameraMonologue != "") {
                LoadDialogueTree(target.cameraMonologue);
            } else if (target.defaultMonologue != "") {
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
        if (doVampireAttack)
            VampireAttack();
        if (doTrapDoor)
            VampireTrap();
        if (doCEOSequence) {
            GameManager.Instance.StartCEOSequence();
        }
        if (targetControl)
            targetControl.disabled = false;
        if (instigatorControl)
            instigatorControl.disabled = false;
        // if (targetAnchoriteDance)
        //     targetAnchoriteDance.StartDance();
        if (menuClosed != null)
            menuClosed();
        menuClosed = null;
    }
    public void UpdateActiveText() {
        switch (textSize) {
            case TextSize.normal:
                speechText.enabled = true;
                largeText.gameObject.SetActive(false);
                break;
            case TextSize.large:
                speechText.enabled = false;
                largeText.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
    public void LoadDialogueTree(string filename) {
        // Debug.Log("load " + filename);
        // CUTSCENE-STYLE DIALOGUE (NO INTERACTION)
        if (cutsceneDialogues.Contains(filename)) {
            cutsceneDialogue = true;
            EnableButtons();
        }
        if (filename == "imp2") {
            disableCommand = true;
            // cutsceneDialogue = true;
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
        textSize = TextSize.normal;
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
        InputController.Instance.commandTarget = target.gameObject;
        InputController.Instance.state = InputController.ControlState.commandSelect;
        UINew.Instance.CloseActiveMenu();
    }
    public void CommandCallback(InteractionParam ip) {
        Speech playerSpeech = GameManager.Instance.playerObject.GetComponent<Speech>();
        Speech targetSpeech = InputController.Instance.commandTarget.GetComponent<Speech>();
        Configure(playerSpeech, targetSpeech, interruptDefault: true);
        desire desireToAct = ip.GetDesire(InputController.Instance.commandTarget, GameManager.Instance.playerObject);

        PromptCommand(ip.Description());
        if (desireToAct == desire.decline) {
            DeclineCommand();
            InputController.Instance.ResetCommandState();
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
    public void RecruitAsk(Speech targetSpeech) {
        Speech playerSpeech = GameManager.Instance.playerObject.GetComponent<Speech>();
        Configure(playerSpeech, targetSpeech, interruptDefault: true);

        List<string> request = new List<string>();
        request.Add("Say, how would you like to be the new yogurt commercial actor?");
        Say(new Monologue(instigator, request.ToArray()));

        string[] ender = new string[] { "END" };
        Say(new Monologue(target, ender));

        List<string> response = new List<string>();
        response.Add("It has always been my dream to be a yogurt commercial actor!");
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
        Speech targetSpeech = InputController.Instance.commandTarget.GetComponent<Speech>();
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
    public void Inquire() {
        Say(instigator, "Are you the previous yogurt commercial actor?");
        Say(target, "I am not the previous yogurt commercial actor.");
        // TODO: set up a node
    }
    public void InquireSuccess() {
        Say(instigator, "Are you the previous yogurt commercial actor?");
        Say(target, "Yes, I used to be the yogurt commercial actor.");
        LoadDialogueTree("detective_success");
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
        CheckForCommands(monologue.text.Peek());
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
        if (disableCommand) {
            suggestButton.interactable = false;
        }
        Canvas.ForceUpdateCanvases();
    }
    public void DisableButtons() {
        foreach (Button button in buttons)
            button.interactable = false;
        choicePanel.SetActive(false);
    }
    public void Update() {
        if (keypressedThisFrame) {
            if (waitForKeyPress) {
                waitForKeyPress = false;
                advancedKeyPressed = true;
                NextLine();
            }
        }
        keypressedThisFrame = false;

        advancedKeyPressed = false;
        if (monologue.text.Count == 0) {
            if (dialogue.Count > 0 && !waitForKeyPress) {
                waitForKeyPress = true;
                if (dialogue.Peek().text.Peek() == "END") {
                    promptText.text = "[PRESS ANY KEY TO END]";
                } else {
                    promptText.text = "[PRESS ANY KEY]";
                }
            }
            return;
        }
        if (monologue.MoreToSay()) {
            DisableButtons();
            SetText();
            blitCounter += 1;
            monologue.speaker.gibberizer.StartPlay();
            promptText.text = "";
        } else {
            monologue.speaker.gibberizer.StopPlay();
            if (monologue.text.Count > 1) {
                waitForKeyPress = true;
                promptText.text = "[PRESS ANY KEY]";
            } else {
                monologue.text.Pop();
                if (dialogue.Count == 0)
                    EnableButtons();
                promptText.text = "";
            }
        }
        if (blitCounter > 5) {
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
    public void NextLine() {
        if (monologue.text.Count > 0) {
            monologue.NextLine();
        } else if (dialogue.Count > 0) {
            SetMonologue(dialogue.Pop());
        }
        CheckForCommands(monologue.text.Peek());
    }
}