using UnityEngine;
using System.Collections;

public class ItemButtonScript : MonoBehaviour {
	public string itemName;
	public void Clicked(){
		UINew.Instance.ItemButtonCallback(this);
	}
}
