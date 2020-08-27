using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Easings;
using UnityEngine.InputSystem;

// TODO: change this

public class WASDGraphic : MonoBehaviour {
    public List<Text> letters;
    public List<Outline> outlines;
    public SpriteRenderer spriteRenderer;
    public float alpha;
    public float fadeInTime = 5;
    public float timer;

    public void Update() {
        timer += Time.deltaTime;

        Color color = spriteRenderer.color;
        color.a = (float)PennerDoubleAnimation.QuintEaseIn(timer, 0, 1, fadeInTime);
        spriteRenderer.color = color;
        foreach (Text text in letters) {
            Color letterColor = text.color;
            letterColor.a = (float)PennerDoubleAnimation.QuintEaseIn(timer, 0, 1, fadeInTime);
            text.color = letterColor;

            // set text to inputcontroller
        }
        foreach (Outline outline in outlines) {
            // adjust colr here
            Color outlineColor = outline.effectColor;
            outlineColor.a = (float)PennerDoubleAnimation.QuintEaseIn(timer, 0, 1, fadeInTime);
            outline.effectColor = outlineColor;
        }

        if (Keyboard.current.anyKey.isPressed) {
            // stop
            Destroy(gameObject);
        }
    }
}
