using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeBook : Interactive {
    void Start() {
        bool showBook = false;
        if (GameManager.Instance.data != null) {
            foreach (MutablePotionData data in GameManager.Instance.data.collectedPotions.Values) {
                showBook = data.unlockedIngredient1 || data.unlockedIngredient2;
                if (showBook)
                    break;
            }
        }
        if (!showBook)
            gameObject.SetActive(false);

        Interaction power = new Interaction(this, "Browse recipes...", "Power");
        power.descString = "Browse recipes";
        interactions.Add(power);
    }
    public void Power() {
        GameObject menu = UINew.Instance.ShowMenu(UINew.MenuType.recipeMenu);
        RecipeMenu bevMenu = menu.GetComponent<RecipeMenu>();
        bevMenu.PopulateItemList();
    }
}
