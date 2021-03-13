using UnityEngine;
using UnityEngine.UI;

public class emailEntryButton : MonoBehaviour {
    private EmailUI emailUI;
    public Email email;
    public Text newText;
    public Text nameText;
    public Text dateText;
    public Button button;
    public Image focusIndicator;



    [Header("incomplete")]
    public ColorBlock incompleteColors;
    [Header("complete")]
    public ColorBlock completeColors;
    public void Initialize(EmailUI ui, Email initEmail) {
        focusIndicator.enabled = false;
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
            button.colors = completeColors;
        } else {
            newText.text = "!";
            button.colors = incompleteColors;
        }


    }
    public void Clicked() {
        emailUI.EmailEntryCallback(email);
        email.read = true;
        focusIndicator.enabled = true;
        // button.Select();
        // ColorBlock cb = button.colors;
        // cb.normalColor = highlightColor;
        // button.colors = cb;
    }
    public void ClearFocusIndicator() {
        focusIndicator.enabled = false;
    }
}
