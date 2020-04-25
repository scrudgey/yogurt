using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour {

    private DirectionEnum dir;
    private float wanderTime = 0;
    public Controller control;

    void Start() {
        // control = GetComponent<Controllable>();

        wanderTime = UnityEngine.Random.Range(0, 2);
        dir = (DirectionEnum)(UnityEngine.Random.Range(0, 4));
        control = new Controller(gameObject);
    }
    public void Update() {
        control.ResetInput();
        if (wanderTime > 0) {
            switch (dir) {
                case DirectionEnum.down:
                    control.downFlag = true;
                    break;
                case DirectionEnum.left:
                    control.leftFlag = true;
                    break;
                case DirectionEnum.right:
                    control.rightFlag = true;
                    break;
                case DirectionEnum.up:
                    control.upFlag = true;
                    break;
            }
        }
        if (wanderTime < -1f) {
            wanderTime = UnityEngine.Random.Range(0, 2);
            dir = (DirectionEnum)(UnityEngine.Random.Range(0, 4));
        } else {
            wanderTime -= Time.deltaTime;
        }
    }
}
