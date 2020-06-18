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

            // float distance = Vector2.Distance(other.transform.position, transform.position);
            float distance = Vector2.Distance(other.ClosestPoint(transform.position), transform.position);
            // float distance = other.ClosestPoint(transform.position);
            float zoom = (float)PennerDoubleAnimation.Linear(circle.radius - distance, minZoom, maxZoom - minZoom, circle.radius);
            // zoom = Mathf.Max(zoom, minZoom);
            // zoom = Mathf.Min(zoom, maxZoom);
            control.maxSize = Mathf.Max(zoom, minZoom);
            // TODO: change this!
        }
    }
}
