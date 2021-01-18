using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeWhenNear : MonoBehaviour {
    public CameraControl cameraControl;
    public float maxIntensity = 1f;
    public float rolloffDistance = 1f;
    // Start is called before the first frame update
    void Start() {
        cameraControl = GameObject.FindObjectOfType<CameraControl>();
    }

    // Update is called once per frame
    void Update() {
        if (GameManager.Instance.playerObject != null) {
            float distance = Vector2.Distance(GameManager.Instance.playerObject.transform.position, transform.position);
            if (distance < rolloffDistance) {
                float amount = maxIntensity * ((rolloffDistance - distance) / rolloffDistance);
                cameraControl.Shake(amount);
            }
        }
    }
}
