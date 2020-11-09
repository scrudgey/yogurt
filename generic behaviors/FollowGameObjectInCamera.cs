using UnityEngine;
public class FollowGameObjectInCamera : MonoBehaviour {
    public GameObject target;
    private RectTransform rect;
    private RectTransform canvasRect;
    public Vector3 offset = new Vector3(0f, 0.25f, 0f);
    void Start() {
        rect = (RectTransform)transform;
        canvasRect = transform.parent.GetComponent<RectTransform>();
    }
    public void Update() {
        if (target == null)
            return;
        Rect camRect = Camera.main.pixelRect;

        // world coordinates
        Vector2 orig = target.transform.position + offset;

        // viewport coordinates: range from (0, 1)
        Vector2 pos = Camera.main.WorldToViewportPoint(orig);

        // clamp the y coordinate
        pos.y = Mathf.Clamp(pos.y, 0.1f, 0.9f);

        // viewport to screen coordinates
        Vector2 screenPos = new Vector2(
            pos.x * canvasRect.sizeDelta.x,
            pos.y * canvasRect.sizeDelta.y
            );
        // prevent going off screen on the left
        screenPos.x = Mathf.Max(screenPos.x, rect.rect.width / 4f);

        // Debug.Log($"{screenPos.x} : {canvasRect.rect.width} - {rect.rect.width / 2f} = {canvasRect.rect.width - rect.rect.width / 2f}");

        // prevent going off screen on the right
        screenPos.x = Mathf.Min(screenPos.x, canvasRect.rect.width - (rect.rect.width / 2f));

        // prevent ging below the screen
        screenPos.y = Mathf.Max(screenPos.y, rect.rect.height / 4f);

        // prevent going above the screen
        screenPos.y = Mathf.Min(screenPos.y, camRect.height - (rect.rect.height / 2f));
        rect.anchoredPosition = screenPos;
    }
    public void PreemptiveUpdate() {
        if (rect == null) {
            rect = (RectTransform)transform;
            canvasRect = transform.parent.GetComponent<RectTransform>();
            Update();
        }
    }
}
