using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoadoutEntryScript : MonoBehaviour {
    public string itemName;
    public string prefabName;
    private bool _enableItem;
    public bool enableItem {
        get {
            return _enableItem;
        }
        set {
            _enableItem = value;
            CheckGrey();
        }
    }
    public LoadoutEditor loadoutEditor;
    public ItemEntryScript itemEntryScript;
    public void DeleteButtonCallback() {
        // call back to loadout editor
        loadoutEditor.EvictItem(this);
    }
    private void CheckGrey() {
        Text myText = transform.Find("item").GetComponent<Text>();
        if (enableItem) {
            myText.color = new Color(1f, 1f, 1f, 1f);
        } else {
            myText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }
    public void Configure(ItemEntryScript script) {
        this.itemEntryScript = script;
        Text entryText = transform.Find("item").GetComponent<Text>();
        prefabName = script.prefabName;
        entryText.text = itemName;
    }
}
