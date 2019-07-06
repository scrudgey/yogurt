using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinusoidalMotion : MonoBehaviour {
    public float angularFreq = 1;
    public float amplitude = 0.2f;
    public Vector3 initPosition;
    float time;
    public void Start() {
        initPosition = transform.position;
    }
    public void Update() {
        time += Time.deltaTime;
        float y = initPosition.y + amplitude * Mathf.Sin(angularFreq * time);
        Vector3 newPos = new Vector3(initPosition.x, y, initPosition.z);
        transform.position = newPos;
    }
}
