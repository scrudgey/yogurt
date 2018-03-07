using UnityEngine;

public class UIColliderManager : MonoBehaviour {
    public EdgeCollider2D leftEdge;
    public EdgeCollider2D rightEdge;
    public EdgeCollider2D topEdge;
    public EdgeCollider2D bottomEdge;
    public RectTransform rt;
    private float updateTimer;
    public void Start() {
        rt = GetComponent<RectTransform>();
        // Debug.Log(rt.localScale.x);
        AdjustColliders();
        // TODO: make these edge colliders.
    }
    public void AdjustColliders() {
        Vector2 topLeft = new Vector2(rt.rect.width / -2f, rt.rect.height / 2f);
        Vector2 topRight = new Vector2(rt.rect.width / 2f, rt.rect.height / 2f);
        Vector2 bottomLeft = new Vector2(rt.rect.width / -2f, rt.rect.height / -2f);
        Vector2 bottomRight = new Vector2(rt.rect.width / 2f, rt.rect.height / -2f);

        Vector2[] tempPoints = topEdge.points;
        tempPoints[0] = topLeft;
        tempPoints[1] = topRight;

        topEdge.points = tempPoints;

        tempPoints[0] = topLeft;
        tempPoints[1] = bottomLeft;

        leftEdge.points = tempPoints;

        tempPoints[0] = topRight;
        tempPoints[1] = bottomRight;

        rightEdge.points = tempPoints;

        tempPoints[0] = bottomLeft;
        tempPoints[1] = bottomRight;

        bottomEdge.points = tempPoints;
    }
    void Update() {
        updateTimer += Time.deltaTime;
        if (updateTimer > 1) {
            updateTimer = 0;
            AdjustColliders();
        }
    }
}
