using UnityEngine;

public class Stimulus : MonoBehaviour {
    public float size;
    private CircleCollider2D circle;
    void Start() {
        circle = gameObject.GetComponent<CircleCollider2D>();
    }
    void Update() {
        if (circle.radius < size) {
            circle.radius += Time.deltaTime;
        } else {
            ClaimsManager.Instance.WasDestroyed(gameObject);
            Destroy(gameObject);
        }
    }
}
