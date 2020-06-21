using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipSpriteRandom : MonoBehaviour {
    public bool flipX = true;
    public bool flipY = true;
    public float dutyCycle = 0;
    public float timer;
    public bool scaleWithVelocity;
    public Rigidbody2D body;
    public void Start() {
        body = GetComponent<Rigidbody2D>();
        Flip();
    }
    public void Flip() {
        Vector3 rot = transform.localScale;
        if (flipX) {
            if (Random.value >= 0.5) {
                rot.x = rot.x * -1f;
            }
        }
        if (flipY) {
            if (Random.value >= 0.5) {
                rot.y = rot.y * -1f;
            }
        }
        transform.localScale = rot;
    }
    public void Update() {
        if (dutyCycle <= 0)
            return;
        if (scaleWithVelocity) {
            timer += Time.deltaTime * (1 + body.velocity.magnitude);
        } else {
            timer += Time.deltaTime;
        }
        if (timer > dutyCycle) {
            timer = 0;
            Flip();
        }
    }
}
