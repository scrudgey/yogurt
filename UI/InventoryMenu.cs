using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenu : MonoBehaviour {
    public Transform itemDrawer;
    public Button closeButton;
    public UIButtonEffects effects;
    public void Awake() {
        foreach (Transform child in itemDrawer) {
            Destroy(child.gameObject);
        }
    }
    public void Initialize(Inventory inventory) {
        effects = GetComponent<UIButtonEffects>();
        effects.buttons = new List<Button>() { closeButton };
        foreach (GameObject item in inventory.items) {
            GameObject button = Instantiate(Resources.Load("UI/ItemButton")) as GameObject;
            Button itemButton = button.GetComponent<Button>();
            effects.buttons.Add(itemButton);
            button.transform.SetParent(itemDrawer.transform, false);
            button.GetComponent<ItemButtonScript>().SetButtonAttributes(item, inventory);
        }
        effects.Configure();
    }
    public void CloseButtonCallback() {
        UINew.Instance.CloseActiveMenu();
    }
}
