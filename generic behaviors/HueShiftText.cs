using UnityEngine;
// using System.Collections;
using UnityEngine.UI;

public class HueShiftText : MonoBehaviour {
    private Text text;
    private HSBColor color;
    public float speedConst = 10f;
    // Use this for initialization
    void Start() {
        text = GetComponent<Text>();
        color = HSBColor.FromColor(text.color);
    }
    void OnEnable() {
        if (!text) {
            text = GetComponent<Text>();
        }
        color = HSBColor.FromColor(text.color);
    }
    void Update() {
        color.h += Time.deltaTime / speedConst;
        if (color.h > 1)
            color.h -= 1f;
        text.color = color.ToColor();
    }
}
