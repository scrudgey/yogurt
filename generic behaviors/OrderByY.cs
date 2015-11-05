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
//			int pos = Mathf.RoundToInt(transform.position.y);
//			pos /= 3;
//			spriteRenderer.sortingOrder = (pos * -1) + OrderOffset;
//			spriteRenderer.sortingOrder = (pos * -1);
//			spriteRenderer.sortingOrder = Mathf.RoundToInt(transform.position.y * 100f) * -1;

//			Vector3 tempPos = transform.position;
//			tempPos.z = Mathf.RoundToInt(transform.position.y * 100f) * -1;
//			tempPos.z = tempPos.y + 50f;
//			transform.position = tempPos;
			int pos = Mathf.RoundToInt(transform.position.y * 25f);
//			pos /= 5; //Remember division of an INT and the modulus operator %? This isn't a float. We WANT to get rid of the remainder.
//			spriteRenderer.sortingOrder = (pos * -1) + OrderOffset; 
			spriteRenderer.sortingOrder = (pos * -1);
		}
	}
}
