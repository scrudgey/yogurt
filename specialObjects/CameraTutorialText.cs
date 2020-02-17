using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraTutorialText : MonoBehaviour {
    public Text text;
    public Image image;
    public float timer;
    public float interval = 1f;

    public Color color1;
    public Color color2;
    public Vector3 arrowPosition1;
    public Vector3 arrowPosition2;
    private int state;
    void Awake() {
        Disable();
    }
    public void Disable() {
        text.enabled = false;
        image.enabled = false;
        state = -1;
    }
    public void Enable() {
        state = 0;
        timer = interval;
        text.enabled = true;
        image.enabled = false;
        text.color = color1;
    }
    void Update() {
        if (state == -1)
            return;
        timer += Time.deltaTime;
        if (timer > interval) {
            timer = 0f;

            switch (state) {
                case 0:
                    text.enabled = true;
                    image.enabled = false;
                    text.color = color1;
                    break;
                case 1:
                    text.color = color2;
                    break;
                case 2:
                    text.color = color1;
                    break;
                case 3:
                    text.color = color2;
                    break;
                case 4:
                    text.enabled = false;
                    image.enabled = true;
                    image.transform.localPosition = arrowPosition1;
                    break;
                case 5:
                    image.transform.localPosition = arrowPosition2;
                    break;
                case 6:
                    image.transform.localPosition = arrowPosition1;
                    break;
                case 7:
                    image.transform.localPosition = arrowPosition2;
                    break;
                default: break;
            }

            state += 1;
            if (state > 7) {
                state = 0;
            }
        }
    }

}
