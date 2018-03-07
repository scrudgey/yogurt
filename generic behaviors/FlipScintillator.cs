// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

public class FlipScintillator : MonoBehaviour {
    public float interval;
    public float timer;
    public void Update() {
        timer += Time.deltaTime;
        if (timer > interval) {
            timer = 0;
            Vector3 newScale = transform.localScale;
            newScale.x = newScale.x * -1;
            transform.localScale = newScale;
        }
    }
}
