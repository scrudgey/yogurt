using UnityEngine;
// using System.Collections;

public class InventoryButtonScript : MonoBehaviour {

	public void Clicked(){
		UINew.Instance.ShowInventoryMenu();
	}
}
