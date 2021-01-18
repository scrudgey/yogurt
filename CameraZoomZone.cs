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
    public bool ignoreDistanceEffects;
    public Coroutine zoomOutCoroutine;
    public float zoomOutTime = 3f;

    public void ForceRecalculate(Collider2D other) {
        if (other == null)
            return;

        if (ignoreDistanceEffects) {
            control.offset = Vector3.Lerp(control.offset, maxOffset, 0.01f);
            control.maxSize = Mathf.Lerp(control.maxSize, maxZoom, 0.01f);
        } else {

            float distance = Vector2.Distance(other.ClosestPoint(transform.position), transform.position);

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
            Vector3 offset = new Vector3(offsetX, offsetY, offsetZ);
            control.offset = offset;
        }

    }
    public void OnTriggerStay2D(Collider2D other) {
        if (InputController.forbiddenTags.Contains(other.tag))
            return;
        if (other.gameObject == GameManager.Instance.playerObject) {
            if (zoomOutCoroutine != null) {
                StopCoroutine(zoomOutCoroutine);
                zoomOutCoroutine = null;
            }
            ForceRecalculate(other);
        }
    }
    public void OnTriggerEnter2D(Collider2D other) {
        if (InputController.forbiddenTags.Contains(other.tag))
            return;
        if (other.gameObject == GameManager.Instance.playerObject) {
            if (zoomOutCoroutine != null) {
                StopCoroutine(zoomOutCoroutine);
                zoomOutCoroutine = null;
            }
            ForceRecalculate(other);
        }
    }
    public void OnTriggerExit2D(Collider2D other) {
        if (InputController.forbiddenTags.Contains(other.tag))
            return;
        if (ignoreDistanceEffects) {
            zoomOutCoroutine = StartCoroutine(ZoomOut());
        }
    }

    IEnumerator ZoomOut() {
        float timer = 0;
        yield return null;
        while (timer < zoomOutTime) {
            timer += Time.deltaTime;
            float zoom = (float)PennerDoubleAnimation.Linear(timer, control.maxSize, minZoom - control.maxSize, zoomOutTime);
            float offsetX = (float)PennerDoubleAnimation.Linear(timer, control.offset.x, minOffset.x - control.offset.x, zoomOutTime);
            float offsetY = (float)PennerDoubleAnimation.Linear(timer, control.offset.y, minOffset.y - control.offset.y, zoomOutTime);
            float offsetZ = (float)PennerDoubleAnimation.Linear(timer, control.offset.z, minOffset.z - control.offset.z, zoomOutTime);
            control.maxSize = zoom;
            control.offset = new Vector3(offsetX, offsetY, offsetZ);
            yield return null;
        }
        yield return null;
    }
}
