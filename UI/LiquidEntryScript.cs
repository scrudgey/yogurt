using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiquidEntryScript : MonoBehaviour {
    public Text entryText;
    public Text newText;
    public Liquid liquid;
    public BeverageMenu menu;
    public void Configure(Liquid liquid) {
        this.liquid = liquid;
        entryText.text = liquid.name;
        if (GameManager.Instance.data.newCollectedLiquids.Contains(liquid)) {
            newText.text = "new!";
            newText.enabled = true;
        } else {
            newText.enabled = false;
        }
    }
    public void Clicked() {
        menu.ItemClick(this);
    }
    public void MouseOver() {
        menu.MouseOver(this);
    }
}
