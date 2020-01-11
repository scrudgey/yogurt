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
    void Start() {
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;

        emailButtons = new List<emailEntryButton>();

        emailListObject = transform.Find("main/window/listPanel/list/Viewport/Content").gameObject;
        toText = transform.Find("main/window/emailView/info/to").GetComponent<Text>();
        fromText = transform.Find("main/window/emailView/info/from").GetComponent<Text>();
        subjectText = transform.Find("main/window/emailView/info/subject").GetComponent<Text>();
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

        foreach (Email email in GameManager.Instance.data.emails) {
            GameObject entry = GameObject.Instantiate(Resources.Load("UI/emailEntry")) as GameObject;
            emailEntryButton entryScript = entry.GetComponent<emailEntryButton>();
            Button emailButton = entry.GetComponent<Button>();
            effects.buttons.Add(emailButton);
            entryScript.Initialize(this, email);
            entry.transform.SetParent(emailListObject.transform, false);
            emailButtons.Add(entryScript);
        }
        if (emailButtons.Count > 0) {
            EmailEntryCallback(emailButtons[0].email);
        }
        effects.Configure();
    }
    public void EmailEntryCallback(Email email) {
        // populate the text entries with the information from the email
        emailText.text = name_hook.Replace(email.content, GameManager.Instance.saveGameName);
        toText.text = "To: " + name_hook.Replace(email.toString, GameManager.Instance.saveGameName);
        fromText.text = "From: " + email.fromString;
        subjectText.text = "Subject: " + name_hook.Replace(email.subject, GameManager.Instance.saveGameName);
        email.read = true;

        foreach (emailEntryButton button in emailButtons) {
            button.CheckReadStatus();
        }
        computer.CheckBubble();
    }

    public void OKButtonCallback() {
        // Destroy(gameObject);
        UINew.Instance.CloseActiveMenu();
        computer.CheckBubble();
    }
}
