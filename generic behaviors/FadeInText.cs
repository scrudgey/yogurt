using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FadeInText : MonoBehaviour {
    private Text text;
    private HSBColor color;
    private HSBColor outlineColor;
    private List<Outline> outlines;
    public float speedConst = 10f;
    public bool fadeOut;
    public float fadeOutTimer;
    public float timeTillFadeOut;
    public float delay;
    public float timer;
    void Start() {
        text = GetComponent<Text>();
        outlines = new List<Outline>(GetComponents<Outline>());
        color = HSBColor.FromColor(text.color);
        if (outlines.Count > 0) {
            outlineColor = HSBColor.FromColor(outlines[0].effectColor);
        }
        color.a = 0f;
        outlineColor.a = 0f;
        if (!fadeOut) {
            text.color = Color.clear;
            foreach (Outline outline in outlines) {
                outline.effectColor = Color.clear;
            }
        }
    }
    public void Reset() {
        color.a = 0f;
        outlineColor.a = 0f;
        fadeOutTimer = 0f;
    }
    void OnEnable() {
        if (!text) {
            text = GetComponent<Text>();
        }
        color = HSBColor.FromColor(text.color);
        color.a = 0f;
    }
    void Update() {
        timer += Time.deltaTime;
        if (timer < delay)
            return;
        if (fadeOut) {
            fadeOutTimer += Time.deltaTime;
        }
        if (!fadeOut || (fadeOutTimer < timeTillFadeOut)) {
            color.a += Time.deltaTime / speedConst;
            outlineColor.a += Time.deltaTime / speedConst;
        }
        if (fadeOut && fadeOutTimer > timeTillFadeOut) {
            color.a -= Time.deltaTime / speedConst;
            outlineColor.a -= Time.deltaTime / speedConst;
            if (outlineColor.a <= 0) {
                gameObject.SetActive(false);
            }
        }
        text.color = color.ToColor();
        foreach (Outline outline in outlines) {
            outline.effectColor = outlineColor.ToColor();
        }
    }
}
