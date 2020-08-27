using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*

it's a 2 step cycle
1. flip and increment y 
2. decrement y

*/

public class FireDance : MonoBehaviour {
    enum Cycle { none, flip, pop }
    Cycle nextAction;
    public float dutyCycle = 0;
    public float timer;
    public void Start() {
        nextAction = Cycle.flip;
        Flip();
    }
    public void Flip() {
        Vector3 position = transform.localPosition;
        Vector3 rot = transform.localScale;
        position.y += 0.01f;
        rot.x = rot.x * -1f;
        transform.localScale = rot;
        transform.localPosition = position;
    }
    public void Pop() {
        Vector3 position = transform.localPosition;
        position.y -= 0.01f;
        transform.localPosition = position;
    }
    public void Update() {
        if (dutyCycle <= 0)
            return;
        timer += Time.deltaTime;
        if (timer > dutyCycle) {
            timer = 0;
            if (nextAction == Cycle.flip) {
                Flip();
                nextAction = Cycle.pop;
            } else if (nextAction == Cycle.pop) {
                Pop();
                nextAction = Cycle.flip;
            }
        }
    }
}
