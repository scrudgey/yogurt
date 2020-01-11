using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnceStopped : MonoBehaviour {
    public Rigidbody2D body;
    private Vector3 lastPosition;
    void FixedUpdate() {
        if (lastPosition != null && lastPosition == transform.position)
            Destroy(gameObject);
        lastPosition = transform.position;
    }
}
