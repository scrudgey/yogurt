using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ScriptSelectionMenu : MonoBehaviour
{

    Text descriptionText;
    GameObject scrollContent;
    Dictionary<string, Commercial> commercials = new Dictionary<string, Commercial>();
    List<GameObject> scriptEntries = new List<GameObject>();    
    ScriptListEntry lastClicked;

    void Start(){
        // This line is a temporary hack: we should change it once there is a 
        // solid idea of the "new day" cycle.
        // GameManager.Instance.InitValues();
        Controller.Instance.suspendInput = true;

        descriptionText = transform.Find("Panel/Body/sidebar/DescriptionPanel/DescriptionBox/Description").GetComponent<Text>();
        scrollContent = transform.Find("Panel/Body/Left/ScriptList/Viewport/Content").gameObject;
        foreach (string script in GameManager.Instance.unlockedCommercials)
        {
            GameObject newEntry = Instantiate(Resources.Load("UI/ScriptListEntry")) as GameObject;
            RectTransform rectTransform = newEntry.GetComponent<RectTransform>();

            newEntry.transform.SetParent(scrollContent.transform);
            rectTransform.localScale = new Vector3(1f, 1f, 1f);

            Commercial newCommercial = Toolbox.Instance.LoadCommercialByName(script);
            commercials[script] = newCommercial;

            Text entryText = newEntry.transform.Find("ScriptName").GetComponent<Text>();
            entryText.text = newCommercial.name;

            ScriptListEntry scriptEntry = newEntry.GetComponent<ScriptListEntry>();
            scriptEntry.commercial = newCommercial;
            scriptEntry.menu = this;
            
            scriptEntries.Add(newEntry);
        }
        
        CreateFreestyleEntry(); 
        EventSystem.current.SetSelectedGameObject(scriptEntries[0]);
        ClickedScript(scriptEntries[0].GetComponent<ScriptListEntry>());
        
    }

    public void CreateFreestyleEntry(){
        Commercial freestyle = new Commercial();
        freestyle.name = "freestyle";
        freestyle.description = "Be free, little one!!!";
        commercials["freestyle"] = freestyle;

        GameObject freeEntry = Instantiate(Resources.Load("UI/ScriptListEntry")) as GameObject;
        RectTransform freeTransform = freeEntry.GetComponent<RectTransform>();
        
        freeEntry.transform.SetParent(scrollContent.transform);
        freeTransform.localScale = new Vector3(1f, 1f, 1f);
        
        Text entryText = freeEntry.transform.Find("ScriptName").GetComponent<Text>();
        entryText.text = "freestyle";
        
        ScriptListEntry freeScript = freeEntry.GetComponent<ScriptListEntry>();
        freeScript.commercial = freestyle;
        freeScript.menu = this;
        
        scriptEntries.Add(freeEntry);
    }

    public void ClickedOkay()
    {
        Controller.Instance.suspendInput = false;
        if (lastClicked){
            Debug.Log(lastClicked);
        }
        Destroy(gameObject);
    }

    public void ClickedScript(ScriptListEntry entry)
    {
        if (lastClicked){
            lastClicked.highlight = false;
            lastClicked.ResetColors();
        }
        
        descriptionText.text = entry.commercial.description;
        lastClicked = entry;
        lastClicked.highlight = true;
    }
}
