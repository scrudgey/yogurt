using UnityEngine;
using UnityEngine.Rendering;
// using System.Collections.Generic;

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
	// Sort
	bool loadInit;
	public float offset;
	// public List<Follower> followers = new List<Follower>();
	void Start () {
		if (!loadInit)
			LoadInit();
	}
	void LoadInit(){
		loadInit = true;
		spriteRenderer = GetComponentInParent<SpriteRenderer>();
		sortGroup = GetComponent<SortingGroup>();
	}
	void Update () {
		if (spriteRenderer.isVisible){
			// if (followers.Count > 0){
			// 	spriteRenderer.sortingOrder = (followers[0].renderer.sortingOrder + followers[0].offset);
			// } else {
			int pos = Mathf.RoundToInt((transform.position.y + offset) * 25f);
			if (sortGroup != null){
				sortGroup.sortingOrder = (pos * -1);
			} else {
				spriteRenderer.sortingOrder = (pos * -1);
			}
			// }
		}
	}
	// public void AddFollower(GameObject target, int offset){
	// 	SpriteRenderer targetRenderer = target.GetComponentInParent<SpriteRenderer>();
	// 	if (targetRenderer == null){
	// 		targetRenderer = target.GetComponentInChildren<SpriteRenderer>();
	// 	}
	// 	if (targetRenderer == null){
	// 		return;
	// 	}
	// 	followers.Add(new Follower(targetRenderer, offset));
	// }
	// public void RemoveFollower(GameObject target){
	// 	List<Follower> tempFollowers = new List<Follower>();
	// 	foreach(Follower follower in followers)
	// 		tempFollowers.Add(follower);
	// 	foreach(Follower follower in followers){
	// 		if (follower.renderer.gameObject == target){
	// 			tempFollowers.Remove(follower);
	// 		}
	// 	}
	// 	followers = tempFollowers;
	// }
}
