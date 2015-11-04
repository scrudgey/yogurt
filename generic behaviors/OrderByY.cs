using UnityEngine;
using System.Collections;

public class OrderByY : MonoBehaviour {

	SpriteRenderer spriteRenderer;
	bool loadInit;

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
			int pos = Mathf.RoundToInt(transform.position.y);
			pos /= 3;
//			spriteRenderer.sortingOrder = (pos * -1) + OrderOffset;
//			spriteRenderer.sortingOrder = (pos * -1);
			spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;
		}
	}
}
