using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : MonoBehaviour {
    public List<Behaviour> blinkers = new List<Behaviour>();
    public float timer;
    public float onInterval;
    public float offInterval;
    private bool blinkersOn;
    void Update() {
        timer += Time.unscaledDeltaTime;
        if ((timer > onInterval && blinkersOn) || (timer > offInterval && !blinkersOn)) {
            timer = 0;
            blinkersOn = !blinkersOn;
            foreach (Behaviour blinker in blinkers) {
                blinker.enabled = blinkersOn;
            }
        }
    }
}
