using UnityEngine;
using System.Collections.Generic;
public class HomeCloset : Interactive {
	public enum ClosetType {all, items, food, clothing}
	public ClosetType type;
	GameObject newBubble;
	public void Start () {
		interactions.Add(new Interaction(this, "Open", "OpenCloset"));
		newBubble = transform.Find("newBubble").gameObject;
		CheckBubble();
	}
	public void InitBubble(){
		newBubble = transform.Find("newBubble").gameObject;
	}
	public void CheckBubble(){
		bool activeBubble = false;
		GameManager.Instance.closetHasNew.TryGetValue(type, out activeBubble);
		newBubble.SetActive(activeBubble);
	}
	public void OpenCloset(){
		ClosetButtonHandler menu = UINew.Instance.ShowClosetMenu();
		menu.PopulateItemList(type);
		GameManager.Instance.DetermineClosetNews();
		CheckBubble();
	}
	public string OpenCloset_desc(){
		switch (type){
			case ClosetType.all:
			return "Browse items";
			case ClosetType.items:
			return "Browse closet";
			case ClosetType.food:
			return "Browse refrigerator";
			case ClosetType.clothing:
			return "Browse dress";
			default:
			return "Browse closet";
		}
	}
}
