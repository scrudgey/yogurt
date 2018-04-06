using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipSpriteRandom : MonoBehaviour {
    public bool flipX = true;
    public bool flipY = true;
    public void Start() {
        Vector3 rot = transform.localScale;
        if (flipX) {
            if (Random.value >= 0.5){
                rot.x = -1f;
            }
        }
        if (flipY) {
            if (Random.value >= 0.5){
                rot.y = -1f;
            }
        }
        transform.localScale = rot;
    }
}
