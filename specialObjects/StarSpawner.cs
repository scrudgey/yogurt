using UnityEngine;

public class StarSpawner : MonoBehaviour {
    public GameObject starPrefab;
    public Vector2 maxXY;
    public Vector2 minXY;
    public float timer;
    void Start() {
        for (int i = 0; i < 50; i++) {
            Spawn();
        }
    }
    public void Spawn(bool left = false) {
        GameObject starObject = Instantiate(starPrefab);
        Vector2 newPosition = new Vector2();
        if (left) {
            newPosition.x = minXY.x;
        } else {
            newPosition.x = Random.Range(minXY.x, maxXY.x);
        }
        newPosition.y = Random.Range(minXY.y, maxXY.y);
        starObject.transform.position = newPosition;
        Star star = starObject.GetComponent<Star>();
        star.size = Random.Range(1, 5);
        star.maxXY = maxXY;
        star.spawner = this;
    }
}
