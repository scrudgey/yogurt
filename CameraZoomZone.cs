using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Easings;

public class CameraZoomZone : MonoBehaviour {
    public bool reverse;
    public CameraControl control;
    public float minZoom;
    public float maxZoom;
    public CircleCollider2D circle;
    public Vector3 minOffset;
    public Vector3 maxOffset;
    // public void OnTriggerExit2D(Collider2D other) {
    //     if (other.gameObject == GameManager.Instance.playerObject) {

    //     }
    // }
    public void ForceRecalculate(Collider2D other) {
        // float distance = Vector2.Distance(other.transform.position, transform.position);
        float distance = Vector2.Distance(other.ClosestPoint(transform.position), transform.position);
        // float distance = other.ClosestPoint(transform.position);


        float t = circle.radius - distance;
        if (reverse) {
            t = distance - circle.radius;

        }

        float zoom = (float)PennerDoubleAnimation.Linear(t, minZoom, maxZoom - minZoom, circle.radius);
        float offsetX = (float)PennerDoubleAnimation.Linear(t, minOffset.x, maxOffset.x - minOffset.x, circle.radius);
        float offsetY = (float)PennerDoubleAnimation.Linear(t, minOffset.y, maxOffset.y - minOffset.y, circle.radius);
        float offsetZ = (float)PennerDoubleAnimation.Linear(t, minOffset.z, maxOffset.z - minOffset.z, circle.radius);

        if (reverse) {
            control.maxSize = Mathf.Min(zoom, minZoom);
            offsetX = Mathf.Min(offsetX, minOffset.x);
            offsetY = Mathf.Min(offsetY, minOffset.y);
            offsetZ = Mathf.Min(offsetY, minOffset.z);
        } else {
            control.maxSize = Mathf.Max(zoom, minZoom);
            offsetX = Mathf.Max(offsetX, minOffset.x);
            offsetY = Mathf.Max(offsetY, minOffset.y);
            offsetZ = Mathf.Max(offsetY, minOffset.z);
        }
        // zoom = Mathf.Max(zoom, minZoom);
        // zoom = Mathf.Min(zoom, maxZoom);
        Vector3 offset = new Vector3(offsetX, offsetY, offsetZ);
        control.offset = offset;
    }
    public void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject == GameManager.Instance.playerObject) {
            ForceRecalculate(other);
        }
    }
}
