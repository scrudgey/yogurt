using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZone : MonoBehaviour {
    public CameraControl cam;
    public Vector3 offset;
    public void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject == GameManager.Instance.playerObject) {
            cam.offset = Vector3.Lerp(cam.offset, offset, 0.05f);
        }
    }
}
