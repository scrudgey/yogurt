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

        GameObject liquidIcon = icon.transform.Find("liquid").gameObject;
        LiquidContainer liquidContainer = item.GetComponent<LiquidContainer>();
        if (liquidContainer != null) {
            Image liquidImage = liquidIcon.GetComponent<Image>();
            if (liquidContainer.liquidDisplaySprite != null) {
                liquidImage.sprite = liquidContainer.liquidDisplaySprite;
                liquidImage.color = liquidContainer.liquid.color;
            } else {
                liquidIcon.SetActive(false);
            }
        } else {
            liquidIcon.SetActive(false);
        }
        iconImage.sprite = itemRenderer.sprite;
    }
    public void Clicked() {
        inventory.RetrieveItem(itemName);
        UINew.Instance.CloseActiveMenu();
    }
}
