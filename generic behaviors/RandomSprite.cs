using UnityEngine;

public class RandomSprite : MonoBehaviour {
	private SpriteRenderer spriteRenderer;
	public Sprite[] variableSprites;
	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = variableSprites[Random.Range(0, variableSprites.Length)];
	}

}
