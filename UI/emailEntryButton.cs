using UnityEngine;
using UnityEngine.UI;

public class emailEntryButton : MonoBehaviour {
    private EmailUI emailUI;
    public Email email;
    public Text newText;
    public Text nameText;
    public Text dateText;
    public Button button;
    public Color highlightColor;
    public Color regularColor;
    public void Initialize(EmailUI ui, Email initEmail) {
        button = GetComponent<Button>();
        emailUI = ui;
        email = initEmail;
        newText = transform.Find("new").GetComponent<Text>();
        nameText = transform.Find("name").GetComponent<Text>();

        nameText.text = email.subject;
        dateText.text = $"    {email.fromString}";
        CheckReadStatus();
    }

    public void CheckReadStatus() {
        if (email.read) {
            newText.text = "";
        } else {
            newText.text = "!";
        }
    }
    public void Clicked() {
        emailUI.EmailEntryCallback(email);
        email.read = true;
        // button.Select();
        // ColorBlock cb = button.colors;
        // cb.normalColor = highlightColor;
        // button.colors = cb;
    }
}
