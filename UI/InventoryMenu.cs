using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenu : MonoBehaviour {
    public Transform itemDrawer;
    public Button closeButton;
    public UIButtonEffects effects;
    public Text titleText;
    public void Awake() {
        foreach (Transform child in itemDrawer) {
            Destroy(child.gameObject);
        }
    }
    public void Initialize(Inventory inventory) {
        Initialize(inventory.items, inventory, null);
    }
    public void Initialize(BagOfHolding bag) {
        titleText.text = "Bag Contents";
        Initialize(bag.items, null, bag);
    }
    public void Initialize(List<GameObject> items, Inventory inv, BagOfHolding bag) {
        effects = GetComponent<UIButtonEffects>();
        effects.buttons = new List<Button>() { closeButton };
        foreach (GameObject item in items) {
            GameObject button = Instantiate(Resources.Load("UI/ItemButton")) as GameObject;
            Button itemButton = button.GetComponent<Button>();
            effects.buttons.Add(itemButton);
            button.transform.SetParent(itemDrawer.transform, false);
            button.GetComponent<ItemButtonScript>().SetButtonAttributes(item, inv, bag);
        }
        effects.Configure();
    }
    public void CloseButtonCallback() {
        UINew.Instance.CloseActiveMenu();
    }
}
