﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LiquidEntryScript : MonoBehaviour, IPointerEnterHandler {
    public Text entryText;
    public Text newText;
    public Liquid liquid;
    public BeverageMenu menu;
    public void Configure(Liquid liquid) {
        this.liquid = liquid;
        // entryText.text = liquid.name;
        entryText.text = Liquid.GetName(liquid);
        if (liquid.newLiquid) {
            newText.text = "new!";
            newText.enabled = true;
            liquid.newLiquid = false;
        } else {
            newText.enabled = false;
        }
    }
    public void Clicked() {
        menu.ItemClick(this);
    }
    // public void MouseOver() {
    //     menu.MouseOver(this);
    // }
    public void OnPointerEnter(PointerEventData eventData) {
        menu.MouseOver(this);
    }
}
