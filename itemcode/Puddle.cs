using UnityEngine;
using System.Collections;

public class Puddle : MonoBehaviour {

	public float amount;
	private SpriteRenderer spriteRenderer;

	void Start () {
		amount = 1;
		spriteRenderer = GetComponent<SpriteRenderer>();

		// set random small puddle sprite
		Sprite[] sprites = Resources.LoadAll<Sprite>("smallpuddle");
		spriteRenderer.sprite = sprites[Random.Range (0,4)];
	}

	void OnTriggerEnter2D(Collider2D coll){

		if (coll.gameObject.tag != "fire"){
			Puddle otherPuddle = coll.gameObject.GetComponent<Puddle>();

			if (otherPuddle){
				Destroy(coll.gameObject);
				Sprite[] sprites = Resources.LoadAll<Sprite>("mediumpuddle");
				spriteRenderer.sprite = sprites[Random.Range (0,4)];
				amount += otherPuddle.amount;
			}
		}

	}
}
