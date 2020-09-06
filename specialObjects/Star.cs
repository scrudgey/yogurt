using UnityEngine;

public class Star : MonoBehaviour {
    public enum motionType { leftToRight, spiral }
    public motionType motion;
    public int size = 1;
    public SpriteRenderer spriteRenderer;
    public Vector2 maxXY;
    public StarSpawner spawner;
    public float timer;
    public float phase;
    public float lifetime;
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.localScale = (float)size / 2.5f * Vector3.one;
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (motion == motionType.leftToRight) {
            body.AddForce(Vector2.right * 25f * size);
        } else if (motion == motionType.spiral) {
            phase = Random.Range(0f, 6.28f);
        }
    }
    void Update() {
        timer += Time.deltaTime;
        if (motion == motionType.leftToRight) {
            if (transform.position.x > maxXY.x) {
                Destroy(gameObject);
                spawner.Spawn(left: true);
            }
        } else if (motion == motionType.spiral) {
            float x = Mathf.Pow(timer, 2) * Mathf.Sin(timer + phase);
            float y = Mathf.Pow(timer, 2) * Mathf.Cos(timer + phase);
            transform.position = new Vector3(x, y, 0);
            if (timer > lifetime) {
                Destroy(gameObject);
            }
        }
    }
}
