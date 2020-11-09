using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firefly : MonoBehaviour {
    public Vector3 initialPosition;
    public float amplitude = 0.1f;
    public float frequency = 10f;
    public float xRange = 1f;
    public float dx = 0.1f;
    private float timer;
    private float phase;
    public SpriteRenderer[] renderers;
    public float visibilityTimer;
    void Start() {
        phase = Random.Range(0, 100);
        xRange = Random.Range(1f, 2f);
    }
    void LateUpdate() {
        if (initialPosition == Vector3.zero)
            initialPosition = transform.localPosition;
    }

    void Update() {
        if (visibilityTimer <= 0) {
            foreach (SpriteRenderer renderer in renderers) {
                renderer.enabled = false;
            }
            if (Random.Range(0, 500f) < 1f) {
                visibilityTimer = Random.Range(3.5f, 8f);
                // Debug.Log(visibilityTimer);
                foreach (SpriteRenderer renderer in renderers) {
                    renderer.enabled = true;
                }
            }
            return;
        }
        visibilityTimer -= Time.deltaTime;
        if (initialPosition == Vector3.zero)
            return;
        timer += Time.deltaTime;
        Vector3 newPos = transform.localPosition;
        newPos.x = newPos.x + Time.deltaTime * dx;
        newPos.y = initialPosition.y + amplitude * Mathf.Sin(timer * frequency + phase);
        if (newPos.x > initialPosition.x + xRange) {
            dx *= -1f;
        }
        if (newPos.x < initialPosition.x - xRange) {
            dx *= -1f;
        }
        transform.localPosition = newPos;
    }
}
