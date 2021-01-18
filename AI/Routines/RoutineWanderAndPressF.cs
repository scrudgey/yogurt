using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineWanderAndPressF : RoutineWander {
        public float timeSinceF;
        public LoHi timeRange;
        // public const LoHi defaultLoHi = new LoHi(1.5f, 20.5f);
        public RoutineWanderAndPressF(GameObject g, Controller c, float lowTime = 1.5f, float hiTime = 20.5f) : base(g, c) {
            routineThought = "I'm a clown.";
            timeSinceF = UnityEngine.Random.Range(1.0f, 1.5f);
            this.timeRange = new LoHi(lowTime, hiTime);
        }
        protected override status DoUpdate() {
            timeSinceF -= Time.deltaTime;
            if (timeSinceF <= 0) {
                timeSinceF = UnityEngine.Random.Range(timeRange.low, timeRange.high);
                control.ShootPressed();
            }
            return base.DoUpdate();
        }
    }
}