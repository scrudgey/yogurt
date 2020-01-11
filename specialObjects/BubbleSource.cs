using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleSource : MonoBehaviour {
    public GameObject bubble;
    public Collider2D emitArea;
    float timer;
    float interval;
    public float minInterval;
    public float maxInterval;

    void Start() {
        interval = Random.Range(minInterval, maxInterval);
    }
    void Update() {
        timer += Time.deltaTime;
        if (timer > interval) {
            timer = 0f;
            interval = Random.Range(minInterval, maxInterval);

            float x = Random.Range(emitArea.bounds.min.x, emitArea.bounds.max.x);
            float y = Random.Range(emitArea.bounds.min.y, emitArea.bounds.max.y);
            GameObject.Instantiate(bubble, new Vector3(x, y, 0), Quaternion.identity);
        }
    }
}
