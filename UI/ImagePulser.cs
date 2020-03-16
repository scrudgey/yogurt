using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImagePulser : MonoBehaviour {
    public Image image;
    public Shadow shadow;
    public float timer;
    public float rotateBaseline;
    public float zoomBaseline;
    public float rotateAmplitude;
    public float rotateFreq;
    public float zoomAmplitude;
    public float zoomFreq;
    public RectTransform imageTransform;
    void Start() {
        imageTransform = image.GetComponent<RectTransform>();
    }
    void Update() {
        timer += Time.unscaledDeltaTime;

        float scaleValue = zoomBaseline + zoomAmplitude * Mathf.Sin(timer * zoomFreq);
        Vector3 scale = new Vector3(scaleValue, scaleValue, scaleValue);

        float angle = rotateAmplitude * Mathf.Sin(timer * rotateFreq);
        Quaternion rotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), new Vector3(Mathf.Sin(angle), Mathf.Cos(angle)));

        imageTransform.localScale = scale;
        imageTransform.localRotation = rotation;
    }
}
