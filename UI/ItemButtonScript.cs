using UnityEngine;
using UnityEngine.UI;

public class ItemButtonScript : MonoBehaviour {
    public string itemName;
    public Inventory inventory;
    public BagOfHolding bagOfHolding;
    public GameObject item;
    public void SetButtonAttributes(GameObject item, Inventory inv, BagOfHolding bag) {
        inventory = inv;
        bagOfHolding = bag;
        this.item = item;
        Text buttonText = transform.Find("Text").GetComponent<Text>();
        GameObject icon = transform.Find("icon").gameObject;
        Item itemComponent = item.GetComponent<Item>();
        Image iconImage = icon.GetComponent<Image>();
        Pickup itemPickup = item.GetComponent<Pickup>();
        SpriteRenderer itemRenderer = item.GetComponent<SpriteRenderer>();
        if (itemComponent != null) {
            itemName = itemComponent.itemName;
        } else itemName = Toolbox.Instance.GetName(item);
        buttonText.text = itemName;

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
        if (itemPickup != null && itemPickup.icon != null) {
            iconImage.sprite = itemPickup.icon;
            // TODO: someday, use c# 6 null-conditional or monads
            Transform balloon = item.transform.Find("balloon");
            if (balloon != null) {
                SpriteRenderer balloonRenderer = balloon.GetComponent<SpriteRenderer>();
                if (balloonRenderer != null) {
                    iconImage.color = balloonRenderer.color;
                }
            }
        } else iconImage.sprite = itemRenderer.sprite;
    }
    public void Clicked() {
        if (inventory != null) {
            inventory.RetrieveItem(itemName);
            UINew.Instance.CloseActiveMenu();
        }
        if (bagOfHolding != null) {
            bagOfHolding.Remove(item);
            UINew.Instance.CloseActiveMenu();
        }
    }
}
