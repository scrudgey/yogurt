using UnityEngine;

public class AlphaFader : MonoBehaviour {
	Color color;
	SpriteRenderer sprite;
	public float period;
	float timer;
	void Start(){
		sprite = GetComponent<SpriteRenderer>();
		color = sprite.color;
	}
	void Update(){
		timer += Time.deltaTime;
		color.a = (Mathf.Cos((6.28f / period) * timer) + 1f) / 2f;
		sprite.color = color;
	}
}
