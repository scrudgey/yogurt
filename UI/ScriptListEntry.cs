using UnityEngine;
using UnityEngine.UI;

public class ScriptListEntry : MonoBehaviour {
    public Commercial commercial;
    public ScriptSelectionMenu menu;
    public bool complete;
    Button button;
    public bool highlight = false;
    public Text text;
    public Shadow shadow;

    //  TODO: fix image color: set to white


    [Header("incomplete")]
    public ColorBlock incompleteColors;
    [Header("complete")]
    public ColorBlock completeColors;

    void Start() {
        text = GetComponentInChildren<Text>();
        // shadow = GetComponentInChildren<Shadow>();
        button = GetComponent<Button>();
        if (complete) {
            button.colors = completeColors;
        } else {
            button.colors = incompleteColors;
        }
    }
    public void Configure(Commercial c, ScriptSelectionMenu scriptMenu) {
        Text entryText = transform.Find("ScriptName").GetComponent<Text>();
        commercial = c;
        entryText.text = c.name;
        menu = scriptMenu;
        complete = false;
        foreach (Commercial completed in GameManager.Instance.data.completeCommercials) {
            if (completed.name == c.name)
                complete = true;
        }
    }
    public void Clicked() {
        if (menu) {
            menu.ClickedScript(this);
        }
    }
    void Update() {
        if (highlight) {
            text.color = Color.black;
            shadow.enabled = false;
        } else {
            text.color = Color.white;
            shadow.enabled = true;
        }
    }
    public void ResetColors() {
        // TODO: change this
        // ColorBlock block = button.colors;
        // if (complete) {
        //     // block.normalColor = completeColor;
        // } else {
        //     // block.normalColor = normColor;
        // }
        // button.colors = block;
    }
}
