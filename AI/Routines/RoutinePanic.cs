using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutinePanic : Routine {
        private float wanderTime = 0;
        // private float switchTime = 0;
        private DirectionEnum dir;
        public RoutinePanic(GameObject g, Controller c) : base(g, c) {
            routineThought = "Panic!!!!";
        }
        public override void Configure() {
            wanderTime = UnityEngine.Random.Range(0, 2);
            dir = (DirectionEnum)(UnityEngine.Random.Range(0, 4));
        }
        protected override status DoUpdate() {
            // switchTime += Time.deltaTime;
            // if (switchTime > 0.5f) {
            //     switchTime = 0;
            //     control.ResetInput();
            //     dir = (DirectionEnum)(UnityEngine.Random.Range(0, 4));
            // }
            if (wanderTime > 0) {
                wanderTime -= Time.deltaTime;
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
            } else {
                if (wanderTime < 0f) {
                    wanderTime = UnityEngine.Random.Range(0, 0.75f);
                    dir = (DirectionEnum)(UnityEngine.Random.Range(0, 4));
                }
            }

            return status.neutral;
        }
    }
}