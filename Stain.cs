using UnityEngine;

public class Stain : MonoBehaviour {
	public SpriteMask parentMask;
	public SpriteRenderer parentRenderer;
	void Update(){
		if (parentRenderer != null){
			parentMask.sprite = parentRenderer.sprite;
		}
	}
}
