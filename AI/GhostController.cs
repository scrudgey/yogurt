using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour {
    enum direction { left, right, up, down, none }
    private direction dir;
    private float wanderTime = 0;
    public Controllable control;

    void Start() {
        control = GetComponent<Controllable>();
        wanderTime = UnityEngine.Random.Range(0, 2);
        dir = (direction)(UnityEngine.Random.Range(0, 4));
        // control.
    }
    public void Update() {
        control.ResetInput();
        if (wanderTime > 0) {
            switch (dir) {
                case direction.down:
                    control.downFlag = true;
                    break;
                case direction.left:
                    control.leftFlag = true;
                    break;
                case direction.right:
                    control.rightFlag = true;
                    break;
                case direction.up:
                    control.upFlag = true;
                    break;
            }
        }
        if (wanderTime < -1f) {
            wanderTime = UnityEngine.Random.Range(0, 2);
            dir = (direction)(UnityEngine.Random.Range(0, 4));
        } else {
            wanderTime -= Time.deltaTime;
        }
    }
}
