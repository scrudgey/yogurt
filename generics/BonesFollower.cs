using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonesFollower : MonoBehaviour {
    public Bones follow;
    Transform cachedTransform;
    public new Transform transform {
        get {
            if (cachedTransform == null) {
                cachedTransform = gameObject.GetComponent<Transform>();
            }
            return cachedTransform;
        }
    }
    private Transform _followTransform;
    public Transform followTransform {
        get {
            if (_followTransform == null)
                _followTransform = follow.transform;
            return _followTransform;
        }
    }
    void Update() {
        if (follow == null) {
            DestroyImmediate(gameObject);
        } else {
            transform.position = followTransform.position;
            transform.localScale = followTransform.lossyScale;
            transform.rotation = followTransform.rotation;
        }
    }
}
