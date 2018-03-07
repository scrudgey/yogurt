using UnityEngine;
using Easings;
using UnityEngine.UI;

public class TextFX : MonoBehaviour {
    public enum FXstyle {
        normal, blink, bounce
    }
    private float styleTime;
    private FXstyle _style;
    public FXstyle style {
        get {
            return _style;
        }
        set {
            // quick hack. does it work in general?
            if (text == null) {
                Start();
            }
            _style = value;
            styleTime = 0;
            text.enabled = true;
        }
    }
    private RectTransform trans;
    private Vector3 tempScale;
    public float blinkInterval = 1f;
    private Text text;

    void Start() {
        trans = GetComponent<RectTransform>();
        tempScale = Vector3.one;
        text = GetComponent<Text>();
    }
    void Update() {
        styleTime += Time.deltaTime;
        switch (style) {
            case FXstyle.normal:
                break;
            case FXstyle.blink:
                if (styleTime < blinkInterval) {
                    text.enabled = true;
                } else {
                    text.enabled = false;
                }
                if (styleTime >= 2 * blinkInterval) {
                    styleTime = 0f;
                }
                break;
            case FXstyle.bounce:
                tempScale.x = (float)PennerDoubleAnimation.ElasticEaseOut(styleTime, 0.001, 1, 0.8);
                tempScale.y = (float)PennerDoubleAnimation.ElasticEaseOut(styleTime, 0.001, 1, 1.0);
                trans.localScale = tempScale;
                break;
            default:
                break;
        }
    }
}
