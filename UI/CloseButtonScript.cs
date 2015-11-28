using UnityEngine;
// using System.Collections;

public class CloseButtonScript : MonoBehaviour {
	public enum closeTarget {Inventory, Closet}
	public closeTarget target;
	public void Clicked(){
		switch (target)
		{
			case closeTarget.Inventory:
			UINew.Instance.CloseInventoryMenu();
			break;
			case closeTarget.Closet:
			break;
			default:
			break;
		}
		
		
	}
}
