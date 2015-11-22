using UnityEngine;
// using System.Collections;

public class OrderByY : MonoBehaviour {

	SpriteRenderer spriteRenderer;
	bool loadInit;
	public float offset;

	void Start () {
		if (!loadInit)
			LoadInit();
	}

	void LoadInit(){
		loadInit = true;
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	void Update () {
		if (spriteRenderer.isVisible){
			int pos = Mathf.RoundToInt((transform.position.y + offset) * 25f);
			spriteRenderer.sortingOrder = (pos * -1);
		}
	}
}
