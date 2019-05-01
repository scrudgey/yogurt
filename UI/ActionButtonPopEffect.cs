using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Easings;

public class ActionButtonPopEffect : MonoBehaviour {
    public float timer;
    public float totalTime = 2f;
    public Color baseColor;
    public Transform targetTransform;
    Image image;
    public void Awake() {
        image = GetComponent<Image>();
        baseColor = image.color;
        timer = totalTime;
        image.enabled = false;
    }
    public void Pop() {
        timer = 0;
        image.enabled = true;
    }
    public void Update() {
        if (timer < totalTime) {
            transform.position = targetTransform.position;
            Vector3 scale = transform.localScale;
            scale.x = (float)PennerDoubleAnimation.ExpoEaseOut(timer, 1, 1, totalTime);
            scale.y = (float)PennerDoubleAnimation.ExpoEaseOut(timer, 1, 1, totalTime);
            float alpha = (float)PennerDoubleAnimation.ExpoEaseOut(timer, 1, -1, totalTime);

            Color newColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            transform.localScale = scale;
            image.color = newColor;

            timer += Time.deltaTime;
        }
    }
}
