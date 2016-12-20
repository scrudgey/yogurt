using UnityEngine;
using UnityEngine.UI;

public class ScriptListEntry : MonoBehaviour {
    public Commercial commercial;
    public ScriptSelectionMenu menu;
    public bool complete;
    Button button;
    Color highColor;
    Color normColor;
    Color completeColor = Color.black;
    public bool highlight = false;
    void Start(){
        button = GetComponent<Button>();
        highColor = button.colors.highlightedColor;
        normColor = button.colors.normalColor;

        ColorBlock block = button.colors;
        if (complete){
            block.normalColor = completeColor;
        } else {
            block.normalColor = normColor;
        }
        button.colors = block;
    }
    public void Configure(Commercial c, ScriptSelectionMenu scriptMenu, bool completeVal){
        Text entryText = transform.Find("ScriptName").GetComponent<Text>();
        commercial = c;
        entryText.text = c.name;
        menu = scriptMenu;
        complete = completeVal;
    }
    public void Clicked(){
        if (menu){
            menu.ClickedScript(this);
        }
    }
    void Update(){
        if (highlight){
            ColorBlock block = button.colors;
            block.normalColor = highColor;
            button.colors = block;
        } 
    }
    public void ResetColors(){
        ColorBlock block = button.colors;
        if (complete){
            block.normalColor = completeColor;
        } else {
            block.normalColor = normColor;
        }
        button.colors = block;
    }
}
