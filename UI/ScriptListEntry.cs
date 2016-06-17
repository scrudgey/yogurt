using UnityEngine;
using UnityEngine.UI;
// using System.Collections;

public class ScriptListEntry : MonoBehaviour {

    public Commercial commercial;
    public ScriptSelectionMenu menu;
    
    Button button;
    Color highColor;
    Color normColor;
    
    public bool highlight = false;
    
    
    void Start(){
        button = GetComponent<Button>();
        highColor = button.colors.highlightedColor;
        normColor = button.colors.normalColor;
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
        block.normalColor = normColor;
        button.colors = block;
    }
}
