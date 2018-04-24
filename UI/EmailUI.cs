using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EmailUI : MonoBehaviour {
    public GameObject emailListObject;
    public Text emailText;
    public Text toText;
    public Text fromText;
    public Text subjectText;
    public List<emailEntryButton> emailButtons;
    public Computer computer;
    void Start() {
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;

        emailButtons = new List<emailEntryButton>();

        emailListObject = transform.Find("main/window/listPanel/list/Viewport/Content").gameObject;
        toText = transform.Find("main/window/emailView/info/to").GetComponent<Text>();
        fromText = transform.Find("main/window/emailView/info/from").GetComponent<Text>();
        subjectText = transform.Find("main/window/emailView/info/subject").GetComponent<Text>();
        emailText = transform.Find("main/window/emailView/text/scrollview/Viewport/content").GetComponent<Text>();

        InitializeEmailList();
    }
    public void InitializeEmailList() {
        // remove any existing entries
        // retrieve an email list, populate the list of buttons
        Destroy(emailListObject.transform.GetChild(0).gameObject);

        foreach (Email email in GameManager.Instance.data.emails) {
            GameObject entry = GameObject.Instantiate(Resources.Load("UI/emailEntry")) as GameObject;
            emailEntryButton entryScript = entry.GetComponent<emailEntryButton>();
            entryScript.Initialize(this, email);
            entry.transform.SetParent(emailListObject.transform, false);
            emailButtons.Add(entryScript);
        }
        if (emailButtons.Count > 0) {
            EmailEntryCallback(emailButtons[0].email);
        }
    }
    public void EmailEntryCallback(Email email) {
        // populate the text entries with the information from the email
        emailText.text = email.content;
        toText.text = "To: " + email.toString;
        fromText.text = "From: " + email.fromString;
        subjectText.text = "Subject: " + email.subject;
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
