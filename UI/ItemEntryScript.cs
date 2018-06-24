using UnityEngine;
using UnityEngine.UI;

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
    public Sprite sprite;
    public string description;

    public void Clicked() {
        ClosetButtonHandler handler = GetComponentInParent<ClosetButtonHandler>();
        handler.ItemClick(this);
    }
    public void MouseOver() {
        ClosetButtonHandler handler = GetComponentInParent<ClosetButtonHandler>();
        handler.ItemMouseover(this);
    }
    private void CheckGrey() {
        Text myText = transform.Find("item").GetComponent<Text>();
        if (enableItem) {
            myText.color = new Color(1f, 1f, 1f, 1f);
        } else {
            myText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }
    public void Configure(string name, HomeCloset.ClosetType type){
        Text newText = transform.Find("newIndicator").GetComponent<Text>();
        ItemEntryScript entryScript = transform.Find("item").GetComponent<ItemEntryScript>();
        Text entryText = transform.Find("item").GetComponent<Text>();
        newText.text = "";
        enableItem = !GameManager.Instance.data.itemCheckedOut[name];
        if (type == HomeCloset.ClosetType.all || type == HomeCloset.ClosetType.items) {
            if (GameManager.Instance.data.newCollectedItems.Contains(name)) {
                if (!GameManager.Instance.data.itemCheckedOut[name]) {
                    GameManager.Instance.data.newCollectedItems.Remove(name);
                    newText.text = "new!";
                }
            }
        }
        if (type == HomeCloset.ClosetType.food) {
            if (GameManager.Instance.data.newCollectedFood.Contains(name)) {
                if (!GameManager.Instance.data.itemCheckedOut[name]) {
                    GameManager.Instance.data.newCollectedFood.Remove(name);
                    newText.text = "new!";
                }
            }
        }
        if (type == HomeCloset.ClosetType.clothing) {
            if (GameManager.Instance.data.newCollectedClothes.Contains(name)) {
                if (!GameManager.Instance.data.itemCheckedOut[name]) {
                    GameManager.Instance.data.newCollectedClothes.Remove(name);
                    newText.text = "new!";
                }
            }
        }
        GameObject tempObject = Instantiate(Resources.Load("prefabs/" + name)) as GameObject;
        Item tempItem = tempObject.GetComponent<Item>();
        sprite = tempObject.GetComponent<SpriteRenderer>().sprite;
        itemName = tempItem.itemName;
        if (tempItem.longDescription != "") {
            description = tempItem.longDescription;
        } else {
            description = tempItem.description;
        }
        entryText.text = tempItem.itemName;
        Destroy(tempObject);
    }
}
