using UnityEngine;
public class FollowGameObjectInCamera : MonoBehaviour {
    public GameObject target;
    private RectTransform rect;
    private RectTransform canvasRect;
	void Start(){
        rect = (RectTransform)transform;
        canvasRect = transform.parent.GetComponent<RectTransform>();
	}
    public void Update(){
        if (target == null)
            return;
        Rect camRect = Camera.main.pixelRect;
        Vector2 orig = target.transform.position + new Vector3(0f, 0.25f, 0f);
        Vector2 pos = Camera.main.WorldToViewportPoint(orig);
        pos.y = Mathf.Clamp(pos.y, 0.1f, 0.9f);
        Vector2 screenPos = new Vector2(
            pos.x * canvasRect.sizeDelta.x,
            pos.y * canvasRect.sizeDelta.y
            );
        screenPos.x = Mathf.Max(screenPos.x, rect.rect.width/2f);
        screenPos.x = Mathf.Min(screenPos.x, camRect.width - rect.rect.width);
        screenPos.y = Mathf.Max(screenPos.y, rect.rect.height/2f);
        screenPos.y = Mathf.Min(screenPos.y, camRect.height - rect.rect.height);
        rect.anchoredPosition = screenPos;
    }
    public void PreemptiveUpdate(){
        if (rect == null){
            rect = (RectTransform)transform;
            canvasRect = transform.parent.GetComponent<RectTransform>();
            Update();
        }
    }
}
