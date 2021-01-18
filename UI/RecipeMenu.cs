using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class RecipeMenu : MonoBehaviour {
    public Text nameText;
    public Text ingredient1Text;
    public Image ingredient1Image;
    public Text ingredient2Text;
    public Image ingredient2Image;
    public Text buffTitle;
    public Text buffDescription;
    public Button closeButton;
    public UIButtonEffects effects;
    public Transform listContent;
    void Awake() {
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
    }
    private RecipeEntryScript spawnEntry() {
        GameObject newObject = Instantiate(Resources.Load("UI/RecipeEntry")) as GameObject;
        RecipeEntryScript script = newObject.GetComponent<RecipeEntryScript>();
        return script;
    }
    public void PopulateItemList() {
        effects = GetComponent<UIButtonEffects>();
        effects.buttons = new List<Button>() { closeButton };
        foreach (Transform childObject in listContent) {
            Destroy(childObject.gameObject);
        }

        List<Liquid> itemList = GameManager.Instance.data.collectedLiquids;
        itemList = itemList.OrderBy(i => i.name).ToList();
        bool mousedover = false;

        foreach (MutablePotionData data in GameManager.Instance.data.collectedPotions.Values) {
            if (!data.unlockedIngredient1 && !data.unlockedIngredient2) {
                continue;
            }
            RecipeEntryScript script = spawnEntry();
            script.Configure(data, this);
            script.transform.SetParent(listContent, false);
            Button entryButton = script.gameObject.GetComponentInChildren<Button>();
            effects.buttons.Add(entryButton);
            if (!mousedover) {
                mousedover = true;
                ItemClick(script);
            }
        }
        effects.Configure();
        GameManager.Instance.data.newCollectedLiquids = new List<Liquid>();
    }
    public void CloseButtonClick() {
        UINew.Instance.CloseActiveMenu();
    }
    public void ItemClick(RecipeEntryScript itemScript) {
        GameObject playerObject = GameManager.Instance.playerObject;
        MutablePotionData data = itemScript.potionData;

        nameText.text = $"potion of {data.name}";

        PotionData potionData = data.myPotionData();

        if (potionData.makeYogurt) {
            buffTitle.text = "Recipe for Yogurt";
        } else {
            buffTitle.text = Buff.buffNames[potionData.buff.type];

        }
        buffDescription.text = potionData.bookDescription;

        if (data.unlockedIngredient1) {
            ingredient1Image.sprite = potionData.ingredient1.icon;
            ingredient1Image.color = potionData.ingredient1.spriteColor;
            ingredient1Text.text = potionData.ingredient1.name;
        } else {
            ingredient1Image.sprite = potionData.ingredient1.icon;
            ingredient1Image.color = Color.black;
            ingredient1Text.text = "???";
        }
        if (data.unlockedIngredient2) {
            ingredient2Image.sprite = potionData.ingredient2.icon;
            ingredient2Image.color = potionData.ingredient2.spriteColor;
            ingredient2Text.text = potionData.ingredient2.name;
        } else {
            ingredient2Image.sprite = potionData.ingredient2.icon;
            ingredient2Image.color = Color.black;
            ingredient2Text.text = "???";
        }
    }
}
