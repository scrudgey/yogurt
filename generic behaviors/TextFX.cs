using UnityEngine;
using Easings;
using UnityEngine.UI;
using TMPro;

public class TextFX : MonoBehaviour {
    public enum FXstyle {
        normal, blink, bounceScale, bounceMargin
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
            if (text != null) {
                text.enabled = true;
            }
            if (tmText != null) {
                tmText.enabled = true;
            }
        }
    }
    private RectTransform trans;
    private Vector3 tempScale;
    public float blinkInterval = 1f;
    public float bounceInterval = 1f;
    public float bounceIntensity = 1f;
    private Text text;
    private TextMeshProUGUI tmText;

    void Start() {
        trans = GetComponent<RectTransform>();
        tempScale = Vector3.one;
        text = GetComponent<Text>();
        tmText = GetComponent<TextMeshProUGUI>();
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
            case FXstyle.bounceMargin:
            case FXstyle.bounceScale:


                if (style == FXstyle.bounceScale) {
                    float startScale = 1f + bounceIntensity;
                    float delta = 1f - startScale;

                    float factor = (float)PennerDoubleAnimation.BounceEaseOut(styleTime, startScale, delta, bounceInterval);
                    tempScale.x = factor;
                    tempScale.y = factor;
                    trans.localScale = tempScale;
                } else if (style == FXstyle.bounceMargin) {
                    float startScale = bounceIntensity * 10f;
                    float delta = bounceIntensity * -10f;

                    float factor = (float)PennerDoubleAnimation.BounceEaseOut(styleTime, startScale, delta, bounceInterval);
                    factor = Mathf.Min(factor, 15f);
                    tmText.margin = new Vector4(0f, 0f, 0f, factor);
                }
                if (styleTime > bounceInterval) {
                    styleTime = 0;
                    style = FXstyle.normal;

                    // reset thing
                    if (style == FXstyle.bounceScale) {
                        trans.localScale = Vector3.one;
                    } else if (style == FXstyle.bounceMargin) {
                        tmText.margin = Vector4.zero;
                    }
                }
                break;
            default:
                break;
        }
    }

    public void StartBounceScale(float intensity) {
        style = FXstyle.bounceScale;
        styleTime = 0;
        bounceIntensity = intensity;
    }
    public void StartBounce(float intensity) {
        style = FXstyle.bounceMargin;
        styleTime = 0;
        bounceIntensity = intensity;
    }
}
