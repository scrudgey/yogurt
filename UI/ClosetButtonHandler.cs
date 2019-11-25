using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class ClosetButtonHandler : MonoBehaviour {
    public Image icon;
    public Text titleText;
    public Text descriptionText;
    public Text nameText;
    public Transform listContent;
    public UIButtonEffects effects;
    public Button closeButton;
    public HomeCloset.ClosetType closetType;
    void Start() {
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
        icon.sprite = null;
        icon.color = new Color(1f, 1f, 1f, 0f);
    }
    private ItemEntryScript spawnEntry() {
        GameObject newObject = Instantiate(Resources.Load("UI/ItemEntry")) as GameObject;
        return newObject.GetComponent<ItemEntryScript>();
    }
    public void PopulateItemList(HomeCloset.ClosetType type) {
        closetType = type;
        effects = GetComponent<UIButtonEffects>();
        effects.buttons = new List<Button>() { closeButton };
        foreach (Transform childObject in listContent) {
            Destroy(childObject.gameObject);
        }
        List<string> itemList = GameManager.Instance.data.collectedObjects;

        if (type == HomeCloset.ClosetType.clothing) {
            itemList = GameManager.Instance.data.collectedClothes;
            titleText.text = "Collected Clothing";
        } else if (type == HomeCloset.ClosetType.food) {
            itemList = GameManager.Instance.data.collectedFood;
            titleText.text = "Collected Food";
        } else if (type == HomeCloset.ClosetType.items) {
            itemList = GameManager.Instance.data.collectedItems;
            titleText.text = "Collected Items";
        }
        Dictionary<string, string> names = new Dictionary<string, string>();
        foreach (string name in itemList) {
            GameObject tempObject = Instantiate(Resources.Load("prefabs/" + name)) as GameObject;
            Item tempItem = tempObject.GetComponent<Item>();
            if (tempItem) {
                names[name] = tempItem.itemName;
            } else names[name] = name;
            Destroy(tempObject);
        }
        itemList = itemList.OrderBy(i => names[i]).ToList();
        foreach (string name in itemList) {
            ItemEntryScript script = spawnEntry();
            // effects.buttons.Add(script.GetComponentInChildren<Button>());
            script.Configure(name, type);
            script.transform.SetParent(listContent, false);

            Button entryButton = script.gameObject.GetComponentInChildren<Button>();
            effects.buttons.Add(entryButton);
        }
        effects.Configure();
    }
    public void CloseButtonClick() {
        UINew.Instance.CloseActiveMenu();
    }
    public void ItemClick(ItemEntryScript itemScript) {
        GameManager.Instance.RetrieveCollectedItem(itemScript.prefabName, closetType);
        UINew.Instance.CloseActiveMenu();
    }
    public void ItemMouseover(ItemEntryScript itemScript) {
        nameText.text = itemScript.itemName;
        descriptionText.text = itemScript.description;
        icon.sprite = itemScript.sprite;
        icon.color = new Color(1f, 1f, 1f, 1f);
    }
}
