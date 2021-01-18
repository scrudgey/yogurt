using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayUpright : MonoBehaviour {
    public void Update() {
        if (transform.parent.parent != null) {
            transform.rotation = Quaternion.identity;
            Vector3 offset = new Vector3(0, 0.15f * transform.parent.parent.lossyScale.y, 0);
            Vector3 worldPosition = transform.parent.parent.position + offset;
            transform.localPosition = transform.parent.InverseTransformPoint(worldPosition);
        }
    }
}
