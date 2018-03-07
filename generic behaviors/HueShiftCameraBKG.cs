using UnityEngine;
// using System.Collections;

public class HueShiftCameraBKG : MonoBehaviour {
    private Camera cam;
    private HSBColor color;
    // Use this for initialization
    void Start() {
        cam = GetComponent<Camera>();
        color = HSBColor.FromColor(cam.backgroundColor);
    }

    void Update() {
        color.h += Time.deltaTime / 10f;
        if (color.h > 1)
            color.h -= 1f;
        cam.backgroundColor = color.ToColor();
    }
}
