using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineWanderAndPressF : RoutineWander {
        public float timeSinceF;
        public RoutineWanderAndPressF(GameObject g, Controller c) : base(g, c) {
            routineThought = "I'm a clown.";
            timeSinceF = UnityEngine.Random.Range(1.0f, 1.5f);
        }
        protected override status DoUpdate() {
            timeSinceF -= Time.deltaTime;
            if (timeSinceF <= 0) {
                timeSinceF = UnityEngine.Random.Range(1.5f, 10.5f);
                control.ShootPressed();
            }
            return base.DoUpdate();
        }
    }
}