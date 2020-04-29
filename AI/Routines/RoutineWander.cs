using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineWander : Routine {
        private float wanderTime = 0;
        private DirectionEnum dir;
        public RoutineWander(GameObject g, Controller c) : base(g, c) {
            routineThought = "I'm wandering around.";
        }
        public override void Configure() {
            wanderTime = UnityEngine.Random.Range(0, 2);
            dir = (DirectionEnum)(UnityEngine.Random.Range(0, 4));
        }
        protected override status DoUpdate() {
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
            return status.neutral;
        }
    }
}