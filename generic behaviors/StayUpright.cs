using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayUpright : MonoBehaviour {
    public void Update() {
        if (transform.parent.parent != null) {
            transform.rotation = Quaternion.identity;
            Vector3 worldPosition = transform.parent.parent.position + new Vector3(0, 0.15f, 0);
            transform.localPosition = transform.parent.InverseTransformPoint(worldPosition);
        }
    }
}
