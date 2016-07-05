using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ClosetButtonHandler : MonoBehaviour {
	private Image icon;
	private Text titleText;
	void Start(){
		// GameObject iconObject = transform.Find("menu/body/InfoPanel/ImagePanel/Icon").gameObject;
		// icon = iconObject.GetComponent<Image>();
		icon = transform.Find("menu/body/InfoPanel/ImagePanel/Icon").GetComponent<Image>();
		// titleText = transform.Find("menu/menubar/titlebar").GetComponent<Text>();
		icon.sprite = null;
		icon.color = new Color(1f, 1f, 1f, 0f);
		// PopulateItemList();
	}
	
	private GameObject spawnEntry(){
		GameObject newObject = Instantiate(Resources.Load("UI/ItemEntry")) as GameObject;
		return newObject;
	}
	
	public void PopulateItemList(HomeCloset.ClosetType type){
		GameObject listObject = transform.Find("menu/body/ItemList").gameObject;
		List<string> itemList = GameManager.Instance.data.collectedObjects;
		titleText = transform.Find("menu/menubar/titlebar").GetComponent<Text>();
		if (type == HomeCloset.ClosetType.clothing){
			itemList = GameManager.Instance.data.collectedClothes;
			titleText.text = "Collected Clothing";
		} else if (type == HomeCloset.ClosetType.food){
			itemList = GameManager.Instance.data.collectedFood;
			titleText.text = "Collected Food";
		} else if (type == HomeCloset.ClosetType.items){
			itemList = GameManager.Instance.data.collectedItems;
			titleText.text = "Collected Items";
		}
		// foreach (string name in GameManager.Instance.data.collectedItems){
		foreach (string name in itemList){
			GameObject newEntry = spawnEntry();
			ItemEntryScript entryScript = newEntry.GetComponent<ItemEntryScript>();
			entryScript.itemName = name;
			entryScript.enableItem = !GameManager.Instance.data.itemCheckedOut[name];
			Text entryText = newEntry.GetComponent<Text>();
			entryText.text = Toolbox.Instance.ScrubText(name);
			newEntry.transform.SetParent(listObject.transform, false);
		}
	}
	public void CloseButtonClick(){
		UINew.Instance.CloseClosetMenu();
	}
	
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
