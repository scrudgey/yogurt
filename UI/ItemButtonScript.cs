using UnityEngine;
using UnityEngine.UI;
// using System.Collections;

public class ItemButtonScript : MonoBehaviour {
	public string itemName;
	
	public void SetButtonAttributes(GameObject item){
		Text buttonText = transform.FindChild("Text").GetComponent<Text>();
		GameObject icon = transform.Find("icon").gameObject;
		Item itemComponent = item.GetComponent<Item>();
		Image iconImage = icon.GetComponent<Image>();
		SpriteRenderer itemRenderer = item.GetComponent<SpriteRenderer>();
		
		buttonText.text = itemComponent.itemName;
		itemName = itemComponent.itemName;
		iconImage.sprite = itemRenderer.sprite;
		Vector2 newAnchor = new Vector2();
		newAnchor.x = itemRenderer.sprite.pivot.x / (200 * itemRenderer.sprite.bounds.extents.x);
		// Debug.Log(itemRenderer.sprite.pivot.x);
		// Debug.Log(itemRenderer.sprite.bounds.extents.x);
		newAnchor.y = itemRenderer.sprite.pivot.y / (200 * itemRenderer.sprite.bounds.extents.y);
		newAnchor = new Vector2(1f, 1f) - newAnchor;
		
		iconImage.rectTransform.anchorMin = newAnchor;
		iconImage.rectTransform.anchorMax = newAnchor;
		iconImage.rectTransform.position.Set(0f, 0f, 10f);
		
	}
	public void Clicked(){
		UINew.Instance.ItemButtonCallback(this);
	}
}
