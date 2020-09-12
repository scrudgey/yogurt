using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeEntryScript : MonoBehaviour {
    public MutablePotionData potionData;
    public RecipeMenu menu;
    public Text myText;
    public void Configure(MutablePotionData data, RecipeMenu menu) {
        this.potionData = data;
        this.menu = menu;
        myText.text = $"potion of {data.potionData.name}";
    }
    public void Clicked() {
        menu.ItemClick(this);
    }
}
