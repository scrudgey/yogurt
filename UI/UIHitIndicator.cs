using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Easings;

public class UIHitIndicator : MonoBehaviour {
    public List<Image> indicators;
    public float timer;
    public Color baseColor;
    private HSBColor color;
    public void Hit() {
        if (timer < 1.5f)
            return;
        timer = 0f;
        SetColors();
    }
    public void Update() {
        if (timer < 1.5f) {
            timer += Time.deltaTime;
            if (timer < 1f) {
                SetColors();
            }
        }
    }
    public void SetColors() {
        float alpha = (float)PennerDoubleAnimation.BackEaseOut(timer, 1, -1, 1);
        Color color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        foreach (Image image in indicators) {
            image.color = color;
        }
    }
}
