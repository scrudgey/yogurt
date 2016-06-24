using UnityEngine;
// using System.Collections;
using UnityEngine.UI;

public class ClosetButtonHandler : MonoBehaviour {
	private Image icon;
	void Start(){
		GameObject iconObject = transform.Find("menu/body/InfoPanel/ImagePanel/Icon").gameObject;
		icon = iconObject.GetComponent<Image>();
		icon.sprite = null;
		icon.color = new Color(1f, 1f, 1f, 0f);
		PopulateItemList();
	}
	
	private GameObject spawnEntry(){
		GameObject newObject = Instantiate(Resources.Load("UI/ItemEntry")) as GameObject;
		return newObject;
	}
	
	private void PopulateItemList(){
		GameObject listObject = transform.Find("menu/body/ItemList").gameObject;
		foreach (string name in GameManager.Instance.data.collectedItems){
			GameObject newEntry = spawnEntry();
			ItemEntryScript entryScript = newEntry.GetComponent<ItemEntryScript>();
			entryScript.itemName = name;
			entryScript.enableItem = !GameManager.Instance.itemCheckedOut[name];
			Text entryText = newEntry.GetComponent<Text>();
			entryText.text = Toolbox.Instance.ScrubText(name);
			newEntry.transform.SetParent(listObject.transform, false);
		}
	}
	public void CloseButtonClick(){
		UINew.Instance.CloseClosetMenu();
	}
	// public void GetButtonClick(){
		
	// }
	
	public void ItemClick(ItemEntryScript itemScript){
		GameManager.Instance.RetrieveCollectedItem(itemScript.itemName);
		UINew.Instance.CloseClosetMenu();
	}
	
	public void ItemMouseover(ItemEntryScript itemScript){
		GameObject tempObject = Instantiate(Resources.Load("prefabs/"+itemScript.itemName)) as GameObject;
		icon.sprite = tempObject.GetComponent<SpriteRenderer>().sprite;
		icon.color = new Color(1f, 1f, 1f, 1f);
		Destroy(tempObject);
	}
}
