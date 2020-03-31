using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutlineFader : MonoBehaviour {
    public Outline outline;
    public float period = 0.5f;
    public int cycles = 3;
    public float timer;
    public float maxTime = 4;

    void Update() {
        timer += Time.unscaledDeltaTime;

        if (timer > maxTime) {
            Destroy(this);
            Destroy(outline);
        } else if (timer > period * cycles) {
            // do nothing
        } else {
            Color color = outline.effectColor;
            color.a = Mathf.Cos(timer * 6.28f / period);
            outline.effectColor = color;
        }
    }
}
