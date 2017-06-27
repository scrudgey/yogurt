using UnityEngine;

public class UIColliderManager : MonoBehaviour {
	public BoxCollider2D leftBox;
	public BoxCollider2D rightBox;
	public BoxCollider2D topBox;
	public BoxCollider2D bottomBox;
	public RectTransform rt;
	private float updateTimer;
	void Start (){
		rt = GetComponent<RectTransform>();
		Vector2 leftSize = new Vector2(25, rt.rect.height);
		Vector2 leftOffset = new Vector2(-1f * rt.rect.width / 2f, 0);

		Vector2 rightSize = new Vector2(25, rt.rect.height);
		Vector2 rightOffset = new Vector2(rt.rect.width / 2f, 0);

		Vector2 topSize = new Vector2(rt.rect.width, 25);
		Vector2 topOffset = new Vector2(0, rt.rect.height / 2f);

		Vector2 bottomSize = new Vector2(rt.rect.width, 25);
		Vector2 bottomOffset = new Vector2(0, -1f * rt.rect.height / 2f);

		leftBox.size = leftSize;
		leftBox.offset = leftOffset;
		rightBox.size = rightSize;
		rightBox.offset = rightOffset;
		bottomBox.offset = bottomOffset;
		bottomBox.size = bottomSize;
		topBox.size = topSize;
		topBox.offset = topOffset;
	}
	void Update(){
		updateTimer += Time.deltaTime;
		if (updateTimer > 1){
			updateTimer = 0;
			Start();
		}
	}
	// void Update () {
	// 	Vector2 leftSize = new Vector2(10, rt.rect.height);
	// 	Vector2 leftOffset = new Vector2(-1f * rt.rect.width / 2f, 0);

	// 	Vector2 rightSize = new Vector2(10, rt.rect.height);
	// 	Vector2 rightOffset = new Vector2(rt.rect.width / 2f, 0);

	// 	Vector2 topSize = new Vector2(rt.rect.width, 10);
	// 	Vector2 topOffset = new Vector2(0, rt.rect.height / 2f);

	// 	Vector2 bottomSize = new Vector2(rt.rect.width, 10);
	// 	Vector2 bottomOffset = new Vector2(0, -1f * rt.rect.height / 2f);

	// 	leftBox.size = leftSize;
	// 	leftBox.offset = leftOffset;
	// 	rightBox.size = rightSize;
	// 	rightBox.offset = rightOffset;
	// 	bottomBox.offset = bottomOffset;
	// 	bottomBox.size = bottomSize;
	// 	topBox.size = topSize;
	// 	topBox.offset = topOffset;
	// }
}
