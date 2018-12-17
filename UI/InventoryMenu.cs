using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMenu : MonoBehaviour {
    public void Initialize(Inventory inventory){
        Transform itemDrawer = transform.Find("menu/itemdrawer");
        foreach (GameObject item in inventory.items) {
            GameObject button = Instantiate(Resources.Load("UI/ItemButton")) as GameObject;
            button.transform.SetParent(itemDrawer.transform, false);
            button.GetComponent<ItemButtonScript>().SetButtonAttributes(item, inventory);
        }
    }
    public void CloseButtonCallback(){
        UINew.Instance.CloseActiveMenu();
    }
}
