using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class FadeInText : MonoBehaviour {
    private Text text;
    private TextMeshProUGUI tmpText;
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
        tmpText = GetComponent<TextMeshProUGUI>();
        outlines = new List<Outline>(GetComponents<Outline>());
        if (text != null)
            color = HSBColor.FromColor(text.color);
        if (tmpText != null)
            color = HSBColor.FromColor(tmpText.faceColor);
        if (outlines.Count > 0) {
            outlineColor = HSBColor.FromColor(outlines[0].effectColor);
        }
        color.a = 0f;
        outlineColor.a = 0f;
        if (!fadeOut) {
            if (text != null)
                text.color = Color.clear;
            if (tmpText != null)
                tmpText.color = Color.clear;
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
        if (!tmpText) {
            tmpText = GetComponent<TextMeshProUGUI>();
        }
        if (text != null)
            color = HSBColor.FromColor(text.color);
        if (tmpText != null)
            color = HSBColor.FromColor(tmpText.color);
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
        if (text != null)
            text.color = color.ToColor();
        if (tmpText != null)
            tmpText.color = color.ToColor();
        foreach (Outline outline in outlines) {
            outline.effectColor = outlineColor.ToColor();
        }
    }
}
