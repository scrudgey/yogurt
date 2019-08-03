using UnityEngine;
using UnityEngine.UI;

public class ItemButtonScript : MonoBehaviour {
    public string itemName;
    public Inventory inventory;
    public void SetButtonAttributes(GameObject item, Inventory inv) {
        inventory = inv;
        Text buttonText = transform.Find("Text").GetComponent<Text>();
        GameObject icon = transform.Find("icon").gameObject;
        Item itemComponent = item.GetComponent<Item>();
        Image iconImage = icon.GetComponent<Image>();
        SpriteRenderer itemRenderer = item.GetComponent<SpriteRenderer>();
        buttonText.text = itemComponent.itemName;
        itemName = itemComponent.itemName;
        iconImage.sprite = itemRenderer.sprite;
    }
    public void Clicked() {
        inventory.RetrieveItem(itemName);
        UINew.Instance.CloseActiveMenu();
    }
}
