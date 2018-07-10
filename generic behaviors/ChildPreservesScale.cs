using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildPreservesScale : MonoBehaviour {
	void LateUpdate () {
		if (transform.lossyScale.x < 0){
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
	}
}
