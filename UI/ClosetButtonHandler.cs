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
    // public Text titleText;
    void Start() {
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
        // icon = transform.Find("menu/body/InfoPanel/ImagePanel/Icon").GetComponent<Image>();
        // descriptionText = transform.Find("menu/body/InfoPanel/TextPanel/Description").GetComponent<Text>();
        // nameText = transform.Find("menu/body/InfoPanel/TextPanel/Title").GetComponent<Text>();
        icon.sprite = null;
        icon.color = new Color(1f, 1f, 1f, 0f);
    }
    private ItemEntryScript spawnEntry() {
        GameObject newObject = Instantiate(Resources.Load("UI/ItemEntry")) as GameObject;
        return newObject.GetComponent<ItemEntryScript>();
    }
    public void PopulateItemList(HomeCloset.ClosetType type) {
        foreach (Transform childObject in listContent) {
            Destroy(childObject.gameObject);
        }
        // GameObject listObject = transform.Find("menu/body/ItemList").gameObject;
        List<string> itemList = GameManager.Instance.data.collectedObjects;

        // titleText = transform.Find("menu/menubar/titlebar").GetComponent<Text>();
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
            // names.Add(tempItem.itemName);
            names[name] = tempItem.itemName;
            Destroy(tempObject);
        }
        // docs = docs.OrderBy(d => docsIds.IndexOf(d.Id)).ToList();
        // itemList.Sort();
        itemList = itemList.OrderBy(i => names[i]).ToList();
        foreach (string name in itemList) {
            ItemEntryScript script = spawnEntry();
            script.Configure(name, type);
            script.transform.SetParent(listContent, false);
        }
    }
    public void CloseButtonClick() {
        UINew.Instance.CloseActiveMenu();
    }
    public void ItemClick(ItemEntryScript itemScript) {
        GameManager.Instance.RetrieveCollectedItem(itemScript.prefabName);
        UINew.Instance.CloseActiveMenu();
    }
    public void ItemMouseover(ItemEntryScript itemScript) {
        nameText.text = itemScript.itemName;
        descriptionText.text = itemScript.description;
        icon.sprite = itemScript.sprite;
        icon.color = new Color(1f, 1f, 1f, 1f);
    }
}
