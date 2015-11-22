using UnityEngine;
// using System.Collections;

public class InventoryButtonScript : MonoBehaviour {
	public void Clicked(){
		UINew.Instance.inventoryVisible = !UINew.Instance.inventoryVisible;
		if (UINew.Instance.inventoryVisible){
			UINew.Instance.ShowInventoryMenu();
		} else {
			UINew.Instance.CloseInventoryMenu();
		}
	}
}
