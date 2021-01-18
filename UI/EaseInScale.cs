using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Easings;

public class EaseInScale : MonoBehaviour {
    public float initScale = 0f;
    public float totalTime = 1f;
    private float timer;
    public RectTransform rect;
    public void Awake() {
        rect = GetComponent<RectTransform>();
        rect.localScale = Vector3.one * initScale;
    }

    public void Update() {
        timer += Time.unscaledDeltaTime;
        if (timer < totalTime) {
            float scale = (float)PennerDoubleAnimation.BackEaseOut(timer, initScale, 1f - initScale, totalTime);
            rect.localScale = Vector3.one * scale;
        } else if (timer > totalTime) {
            rect.localScale = Vector3.one;
            Destroy(this);
        }
    }
}
