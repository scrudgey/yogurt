using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class ScriptSelectionMenu : MonoBehaviour{
    Text descriptionText;
    GameObject scrollContent;
    ScriptListEntry lastClicked;
    void Start(){
        Controller.Instance.suspendInput = true;
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;

        descriptionText = transform.Find("Panel/Body/sidebar/DescriptionPanel/DescriptionBox/Description").GetComponent<Text>();
        scrollContent = transform.Find("Panel/Body/Left/ScriptList/Viewport/Content").gameObject;
        GameObject firstEntry = null;
        foreach (Commercial script in GameManager.Instance.data.unlockedCommercials)
        {
            bool complete = false;
            foreach(Commercial completed in GameManager.Instance.data.completeCommercials){
                if (completed.name == script.name)
                    complete = true;
            }
            if (complete)
                continue;
            GameObject newEntry = CreateScriptButton(script);
            if (firstEntry == null){
                firstEntry = newEntry;
            }
        }
        foreach(Commercial script in GameManager.Instance.data.unlockedCommercials){
            bool complete = false;
            foreach(Commercial completed in GameManager.Instance.data.completeCommercials){
                if (completed.name == script.name)
                    complete = true;
            }
            if (!complete)
                continue;
            GameObject newEntry = CreateScriptButton(script);
            if (firstEntry == null){
                firstEntry = newEntry;
            }
        }
        EventSystem.current.SetSelectedGameObject(firstEntry);
        ClickedScript(firstEntry.GetComponent<ScriptListEntry>());
    }
    GameObject CreateScriptButton(Commercial script){
        GameObject newEntry = Instantiate(Resources.Load("UI/ScriptListEntry")) as GameObject;
        newEntry.transform.SetParent(scrollContent.transform, false);
        ScriptListEntry scriptEntry = newEntry.GetComponent<ScriptListEntry>();
        scriptEntry.Configure(script, this);
        return newEntry;
    }
    public void ClickedOkay(){
        Controller.Instance.suspendInput = false;
        if (lastClicked){
            GameManager.Instance.activeCommercial = lastClicked.commercial;
            VideoCamera director = GameObject.FindObjectOfType<VideoCamera>();
            director.Enable();
        }
        Destroy(gameObject);
    }
    public void ClickedCancel(){
        Controller.Instance.suspendInput = false;
        Destroy(gameObject);
    }
    public void ClickedScript(ScriptListEntry entry){
        if (lastClicked){
            lastClicked.highlight = false;
            lastClicked.ResetColors();
        }
        descriptionText.text = entry.commercial.description;
        lastClicked = entry;
        lastClicked.highlight = true;
    }
}
