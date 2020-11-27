using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularMotion : MonoBehaviour {
    Vector3 initialPosition;
    public float radius = 0.2f;
    public float timer;
    public float frequency;
    void Start() {
        initialPosition = transform.position;
    }
    public void FixedUpdate() {
        timer += Time.fixedDeltaTime;
        float x = initialPosition.x + radius * Mathf.Sin(timer * frequency);
        float y = initialPosition.y + radius * Mathf.Cos(timer * frequency);
        Vector3 newPos = new Vector3(x, y, initialPosition.z);
        transform.position = newPos;
    }
}
