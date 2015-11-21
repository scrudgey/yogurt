using UnityEngine;
using System.Collections;

public class CloseButtonScript : MonoBehaviour {

	public void Clicked(){
		UINew.Instance.CloseInventoryMenu();
	}
}
