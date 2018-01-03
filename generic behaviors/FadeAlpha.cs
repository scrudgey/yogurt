using System.Collections.Generic;
using UnityEngine;

public class FadeAlpha : MonoBehaviour {
	public HashSet<SpriteRenderer> spriteRenderers = new HashSet<SpriteRenderer>();
	public float period = 3f;
	public float minAlpha = 0.75f;
	public float maxAlpha = 1f;
	public float timer;
	public void Start(){
		spriteRenderers.Add(GetComponent<SpriteRenderer>());
	}
	public void Update(){
		timer += Time.deltaTime;
		foreach(SpriteRenderer spriteRenderer in spriteRenderers){
			Color color = spriteRenderer.color;
			color.a = minAlpha + (maxAlpha - minAlpha) * Mathf.Sin(timer * 6.28f / period);
			spriteRenderer.color = color;
		}
	}
}
