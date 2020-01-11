using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour {
    public SpriteRenderer spriteRenderer;
    public AnimateFrames animateFrames;
    public FlipSpriteRandom flipSpriteRandom;
    public float maxLife;
    public float minLife;
    public float minSpeed;
    public float maxSpeed;
    private float speed;
    private float lifeTime;
    private float timer;
    public float maxAnimSpeed;
    public float minAnimSpeed;

    public Transform footPoint;
    void Start() {
        lifeTime = Random.Range(minLife, maxLife);
        speed = Random.Range(minSpeed, maxSpeed);
    }
    void Update() {
        timer += Time.deltaTime;
        if (timer > lifeTime) {
            animateFrames.enabled = true;
            animateFrames.frameTime = Random.Range(minAnimSpeed, maxAnimSpeed);
            flipSpriteRandom.enabled = true;
        }
        Vector3 newPos = transform.position;
        newPos.y += speed * Time.deltaTime;

        Vector3 footPos = footPoint.localPosition;
        footPos.y -= speed * Time.deltaTime;

        footPoint.localPosition = footPos;
        transform.position = newPos;
    }
}
