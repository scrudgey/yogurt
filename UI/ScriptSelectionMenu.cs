using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class ScriptSelectionMenu : MonoBehaviour {
    Text descriptionText;
    GameObject scrollContent;
    ScriptListEntry lastClicked;
    public List<Button> builtInButtons;
    public UIButtonEffects effects;
    void Start() {
        effects = GetComponent<UIButtonEffects>();
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;

        descriptionText = transform.Find("Panel/Body/sidebar/DescriptionPanel/DescriptionBox/Description").GetComponent<Text>();
        scrollContent = transform.Find("Panel/Body/Left/ScriptList/Viewport/Content").gameObject;
        GameObject firstEntry = null;
        effects.buttons = new List<Button>(builtInButtons);
        foreach (Commercial script in GameManager.Instance.data.unlockedCommercials) {
            bool complete = false;
            foreach (Commercial completed in GameManager.Instance.data.completeCommercials) {
                if (completed.name == script.name)
                    complete = true;
            }
            if (complete)
                continue;
            GameObject newEntry = CreateScriptButton(script);
            if (firstEntry == null) {
                firstEntry = newEntry;
            }
        }
        foreach (Commercial script in GameManager.Instance.data.unlockedCommercials) {
            bool complete = false;
            foreach (Commercial completed in GameManager.Instance.data.completeCommercials) {
                if (completed.name == script.name)
                    complete = true;
            }
            if (!complete)
                continue;
            GameObject newEntry = CreateScriptButton(script);
            if (firstEntry == null) {
                firstEntry = newEntry;
            }
        }
        EventSystem.current.SetSelectedGameObject(firstEntry);
        ClickedScript(firstEntry.GetComponent<ScriptListEntry>());
        effects.Configure();
    }
    GameObject CreateScriptButton(Commercial script) {
        GameObject newEntry = Instantiate(Resources.Load("UI/ScriptListEntry")) as GameObject;
        effects.buttons.Add(newEntry.GetComponent<Button>());
        newEntry.transform.SetParent(scrollContent.transform, false);
        ScriptListEntry scriptEntry = newEntry.GetComponent<ScriptListEntry>();
        scriptEntry.Configure(script, this);
        return newEntry;
    }
    public void ClickedOkay() {
        if (lastClicked) {
            GameManager.Instance.StartCommercial(new Commercial(lastClicked.commercial));
        }
        UINew.Instance.CloseActiveMenu();
    }
    public void ClickedCancel() {
        UINew.Instance.CloseActiveMenu();
    }
    public void ClickedScript(ScriptListEntry entry) {
        if (lastClicked) {
            lastClicked.highlight = false;
            lastClicked.ResetColors();
        }
        descriptionText.text = entry.commercial.description;
        lastClicked = entry;
        lastClicked.highlight = true;
    }
}
