using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HallucinationTrail : MonoBehaviour {
    public GameObject trailPrefab;
    public float interval = 0.1f;
    public Color color;
    HSBColor myColor;
    public float timer;
    public SpriteRenderer mySpriteRenderer;
    public float trailLifetime = 1f;
    public float colorInterval = 0.1f;
    // public 
    void Start() {
        myColor = new HSBColor(color);
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        timer += Time.deltaTime;
        if (timer > interval) {
            timer = 0f;
            SpawnTrail();
        }
    }

    void SpawnTrail() {
        GameObject trail = GameObject.Instantiate(trailPrefab, transform.position, Quaternion.identity) as GameObject;
        SpriteRenderer spriteRenderer = trail.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            spriteRenderer.sprite = mySpriteRenderer.sprite;
            spriteRenderer.color = myColor.ToColor();
        }
        trail.transform.localScale = transform.localScale;
        Destroy(trail, trailLifetime);
        myColor.h += colorInterval;
        if (myColor.h > 1)
            myColor.h -= 1f;
    }
}
