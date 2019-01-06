using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMenu : MonoBehaviour {
    public Transform itemDrawer;
    public void Awake(){
        foreach(Transform child in itemDrawer){
            Destroy(child.gameObject);
        }
    }
    public void Initialize(Inventory inventory){
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
