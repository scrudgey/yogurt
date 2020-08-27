using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCollectionButton : MonoBehaviour {
    public ItemCollectionInspector inspector;
    public string itemName;
    public string description;
    public Sprite sprite;
    public void Configure(ItemCollectionInspector inspector, string name) {
        this.inspector = inspector;

        GameObject tempObject = Instantiate(Resources.Load("prefabs/" + name)) as GameObject;
        Item tempItem = tempObject.GetComponent<Item>();
        // set icon
        Pickup itemPickup = tempObject.GetComponent<Pickup>();
        SpriteRenderer itemRenderer = tempObject.GetComponent<SpriteRenderer>();
        if (itemPickup != null && itemPickup.icon != null) {
            sprite = itemPickup.icon;
        } else sprite = itemRenderer.sprite;
        // this.sprite = tempObject.GetComponent<SpriteRenderer>().sprite;
        this.itemName = tempItem.itemName;
        // set description
        if (tempItem.longDescription != "") {
            this.description = tempItem.longDescription;
        } else {
            this.description = tempItem.description;
        }
        Destroy(tempObject);

        // set button text
        Text myText = transform.Find("Text").GetComponent<Text>();
        myText.text = itemName;
    }
    public void ButtonClicked() {
        inspector.EntryClickedCallback(this);
    }
}
