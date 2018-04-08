using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipSpriteRandom : MonoBehaviour {
    public bool flipX = true;
    public bool flipY = true;
    public float dutyCycle = 0;
    public float timer;
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
    public void Update(){
        if (dutyCycle <= 0)
            return;
        timer += Time.deltaTime;
        if (timer > dutyCycle){
            timer = 0;
            Start();
        }
    }
}
