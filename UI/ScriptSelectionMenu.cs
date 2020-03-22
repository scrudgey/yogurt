using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

        effects.buttons = new List<Button>(builtInButtons);
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "gravy_studio") {
            LoadCommercials(gravy: true);
        } else LoadCommercials();

        effects.Configure();
    }
    public void LoadCommercials(bool gravy = false) {
        GameObject firstEntry = null;
        HashSet<Commercial> unlockedCommercials = new HashSet<Commercial>();

        foreach (Commercial commercial in GameManager.Instance.data.unlockedCommercials) {
            if (gravy == commercial.gravy) {
                unlockedCommercials.Add(commercial);
            }
        }

        HashSet<Commercial> completeCommercials = new HashSet<Commercial>(unlockedCommercials);
        completeCommercials.IntersectWith(GameManager.Instance.data.completeCommercials);

        HashSet<Commercial> incompleteCommercials = new HashSet<Commercial>(unlockedCommercials);
        incompleteCommercials.ExceptWith(GameManager.Instance.data.completeCommercials);


        foreach (Commercial script in incompleteCommercials) {
            GameObject newEntry = CreateScriptButton(script);
            if (firstEntry == null) {
                firstEntry = newEntry;
            }
        }
        foreach (Commercial script in completeCommercials) {
            GameObject newEntry = CreateScriptButton(script);
            if (firstEntry == null) {
                firstEntry = newEntry;
            }
        }

        EventSystem.current.SetSelectedGameObject(firstEntry);
        ClickedScript(firstEntry.GetComponent<ScriptListEntry>());
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
