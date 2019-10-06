// See page 27 of the big dev notebook
using UnityEngine;
using System;
using System.Collections.Generic;
public class Parallax : MonoBehaviour {
    [Serializable]
    public struct ParallaxLayer {
        public Vector2 originalPosition;
        public Transform transform;
        public float speed;
        public Vector2 offset;
    }
    public Transform baseLayer;
    public Vector2 originalPosition;
    public List<ParallaxLayer> layers = new List<ParallaxLayer>();
    public bool applyOffsets;
    public Vector2 layerNeutralAtPosition;
    void Start() {
        Transform cameraTransform = GameObject.FindObjectOfType<Camera>().transform;
        originalPosition = cameraTransform.position;
        for (int i = 0; i < layers.Count; i++) {
            ParallaxLayer layer = layers[i];
            layer.originalPosition = layers[i].transform.localPosition;
            // if (applyOffsets) {
            //     // Vector2 offset = new Vector2();
            //     // offset.x = -1.0f * layerNeutralAtPosition.x * layers[i].speed;
            //     // offset.y = -1.0f * layerNeutralAtPosition.y * layers[i].speed;
            //     // layer.offset = offset;
            //     layer.offset = -1f * layerNeutralAtPosition * layers[i].speed;
            // } else {
            //     layer.offset = Vector2.zero;
            // }
            layers[i] = layer;
        }
    }
    void Update() {
        Vector3 difference = (Vector2)GameManager.Instance.cam.transform.position - originalPosition;
        foreach (ParallaxLayer layer in layers) {
            layer.transform.position = baseLayer.position + difference * layer.speed + (Vector3)layer.offset;
        }
    }
}
