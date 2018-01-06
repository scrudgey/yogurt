using UnityEngine;
using UnityEngine.Rendering;

public class OrderByY : MonoBehaviour {
	[System.Serializable]
	public struct Follower {
		public SpriteRenderer renderer;
		public int offset;
		public Follower (SpriteRenderer renderer, int offset=1){
			this.renderer = renderer;
			this.offset = offset;
		}
	}
	SpriteRenderer spriteRenderer;
	SortingGroup sortGroup;
	public float offset;
	void Start () {
		spriteRenderer = GetComponentInParent<SpriteRenderer>();
		sortGroup = GetComponent<SortingGroup>();
	}
	void Update () {
		if (spriteRenderer.isVisible){
			int pos = Mathf.RoundToInt((transform.position.y + offset) * 25f);
			if (sortGroup != null){
				sortGroup.sortingOrder = (pos * -1);
			} else {
				spriteRenderer.sortingOrder = (pos * -1);
			}
		}
	}
}
