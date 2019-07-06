using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Easings;

public class CameraZoomZone : MonoBehaviour {

    public CameraControl control;
    public float minZoom;
    public float maxZoom;
    public CircleCollider2D circle;
    public void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject == GameManager.Instance.playerObject) {
            float distance = Vector2.Distance(other.transform.position, transform.position);
            float zoom = (float)PennerDoubleAnimation.QuadEaseIn(circle.radius - distance, minZoom, maxZoom, circle.radius);
            control.maxSize = zoom;
        }
    }
}
