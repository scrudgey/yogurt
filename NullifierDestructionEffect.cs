using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Easings;

public class NullifierDestructionEffect : MonoBehaviour {
    public SpriteRenderer spriteRenderer;
    public Color color;
    public float timer;
    public float lifetime = 1f;
    public GameObject poofEffect;
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        color = Color.white;
    }

    void Update() {
        timer += Time.deltaTime;

        float colorFactor = (float)PennerDoubleAnimation.Linear(timer, 1, -1, lifetime);
        color.g = colorFactor;
        color.b = colorFactor;
        color.r = 1;
        spriteRenderer.color = color;

        float amplitude = (float)PennerDoubleAnimation.Linear(timer, 0, 0.05f, lifetime);
        Vector2 pos = transform.position;
        pos += amplitude * Random.insideUnitCircle;
        transform.position = pos;

        if (timer > lifetime) {
            Destroy(gameObject);
            GameObject.Instantiate(poofEffect, transform.position, Quaternion.identity);
        }
    }
}
