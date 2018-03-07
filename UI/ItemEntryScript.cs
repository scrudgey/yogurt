using UnityEngine;
using UnityEngine.UI;
// using System.Collections;

public class ItemEntryScript : MonoBehaviour {

    public string itemName;
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

    public void Clicked() {
        ClosetButtonHandler handler = GetComponentInParent<ClosetButtonHandler>();
        handler.ItemClick(this);
    }

    public void MouseOver() {
        ClosetButtonHandler handler = GetComponentInParent<ClosetButtonHandler>();
        handler.ItemMouseover(this);
    }

    private void CheckGrey() {
        Text myText = GetComponent<Text>();
        if (enableItem) {
            myText.color = new Color(1f, 1f, 1f, 1f);
        } else {
            myText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }
}
