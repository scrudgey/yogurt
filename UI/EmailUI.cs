using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class EmailUI : MonoBehaviour {
    public GameObject emailListObject;
    public Text emailText;
    public Text toText;
    public Text fromText;
    public Text subjectText;
    public List<emailEntryButton> emailButtons;
    public Computer computer;
    public Button doneButton;
    public UIButtonEffects effects;
    private Regex name_hook = new Regex(@"\$name");
    public Image torsoImage;
    public Image headImage;
    public List<Sprite> headSprites = new List<Sprite>();
    float timer = 0;
    float pauseCountDown = 0f;
    int headIndex;
    void Start() {
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;

        emailButtons = new List<emailEntryButton>();

        emailListObject = transform.Find("main/window/listPanel/list/Viewport/Content").gameObject;
        toText = transform.Find("main/window/emailView/infobar/info/to").GetComponent<Text>();
        fromText = transform.Find("main/window/emailView/infobar/info/from").GetComponent<Text>();
        subjectText = transform.Find("main/window/emailView/infobar/info/subject").GetComponent<Text>();
        emailText = transform.Find("main/window/emailView/text/sv/Viewport/Content").GetComponent<Text>();

        toText.text = "To:";
        fromText.text = "From:";
        subjectText.text = "Subject:";
        emailText.text = "";

        InitializeEmailList();
    }
    public void InitializeEmailList() {
        effects = GetComponent<UIButtonEffects>();
        effects.buttons = new List<Button>() { doneButton };
        // remove any existing entries
        // retrieve an email list, populate the list of buttons
        // Destroy(emailListObject.transform.GetChild(0).gameObject);
        foreach (Transform child in emailListObject.transform) {
            Destroy(child.gameObject);
        }
        emailEntryButton selectedEmail = null;
        foreach (Email email in GameManager.Instance.data.emails) {
            GameObject entry = GameObject.Instantiate(Resources.Load("UI/emailEntry")) as GameObject;
            emailEntryButton entryScript = entry.GetComponent<emailEntryButton>();
            Button emailButton = entry.GetComponent<Button>();
            effects.buttons.Add(emailButton);
            entryScript.Initialize(this, email);
            entry.transform.SetParent(emailListObject.transform, false);
            entry.transform.SetAsFirstSibling();
            emailButtons.Add(entryScript);
            if (selectedEmail == null) {
                selectedEmail = entryScript;
            } else if (!email.read) {
                selectedEmail = entryScript;
            }
        }
        if (selectedEmail != null) {
            EmailEntryCallback(selectedEmail.email);
            selectedEmail.button.Select();
        }
        effects.Configure();
    }
    public void EmailEntryCallback(Email email) {
        // populate the text entries with the information from the email
        emailText.text = name_hook.Replace(email.content, GameManager.Instance.saveGameName);
        emailText.text = Speech.ParseGender(emailText.text);
        toText.text = "To: " + name_hook.Replace(email.toString, GameManager.Instance.saveGameName);
        fromText.text = "From: " + email.fromString;
        subjectText.text = "Subject: " + name_hook.Replace(email.subject, GameManager.Instance.saveGameName);
        email.read = true;
        pauseCountDown = 0f;

        // load sprites
        torsoImage.sprite = Toolbox.MemoizedSkinTone($"{email.torsoSprite}_spritesheet", (SkinColor)email.skinColor)[7];
        Sprite[] tempHeadSprites = Toolbox.MemoizedSkinTone($"{email.headSprite}_head", (SkinColor)email.skinColor);
        headSprites = new List<Sprite>();
        headSprites.Add(tempHeadSprites[2]);
        headSprites.Add(tempHeadSprites[3]);
        headImage.sprite = headSprites[0];

        foreach (emailEntryButton button in emailButtons) {
            button.CheckReadStatus();
        }
        computer.CheckBubble();
    }
    void Update() {
        if (headSprites.Count <= 0) return;
        if (pauseCountDown > 0f) {
            pauseCountDown -= Time.unscaledDeltaTime;
            return;
        }
        timer += Time.unscaledDeltaTime;
        if (timer > 0.1f) {
            timer = 0f;
            headIndex += 1;
            if (headIndex > 1) headIndex = 0;
            headImage.sprite = headSprites[headIndex];
            if (UnityEngine.Random.Range(0, 100f) < 3f) {
                pauseCountDown = Random.Range(1f, 2f);
                headImage.sprite = headSprites[0];
            }
        }
    }

    public void OKButtonCallback() {
        // Destroy(gameObject);
        UINew.Instance.CloseActiveMenu();
        computer.CheckBubble();
    }
}
