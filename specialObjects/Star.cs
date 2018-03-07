using UnityEngine;

public class Star : MonoBehaviour {
    public int size = 1;
    public SpriteRenderer spriteRenderer;
    public Vector2 maxXY;
    public StarSpawner spawner;
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.localScale = (float)size / 2.5f * Vector3.one;
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        body.AddForce(Vector2.right * 25f * size);
    }
    void Update() {
        if (transform.position.x > maxXY.x) {
            Destroy(gameObject);
            spawner.Spawn(left: true);
        }
    }
}
