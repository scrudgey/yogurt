using UnityEngine;
using System.Collections.Generic;

public class AnimateFrames : MonoBehaviour {
	public List<Sprite> frames;
	private SpriteRenderer spriteRenderer;
	private float animationTimer;
	public float frameTime = 1f;
	private int frameIndex = 0;
	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer == null){
			Destroy(this);
		}
		spriteRenderer.sprite = frames[0];
	}
	
	// Update is called once per frame
	void Update () {
		animationTimer += Time.deltaTime;
		if (animationTimer > frameTime){
			frameIndex += 1;
			if (frameIndex == frames.Count){
				frameIndex = 0;
			}
			animationTimer = 0f;
			spriteRenderer.sprite = frames[frameIndex];
		}
	}
}
