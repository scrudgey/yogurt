using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineInsultTarget : Routine {
        public RoutineLookForTarget lookForTarget;
        public float slewInterval = 10f;
        public float timer;
        public Speech speech;
        public RoutineInsultTarget(GameObject g, Controller c, RoutineLookForTarget lookForTarget) : base(g, c) {
            routineThought = "Watch out for my acid tongue!!!";
            speech = g.GetComponent<Speech>();
            this.lookForTarget = lookForTarget;
        }
        protected override status DoUpdate() {
            timer -= Time.deltaTime;
            if (timer <= 0) {
                timer = slewInterval;
                if (lookForTarget.target != null)
                    DoInsult();
            }
            return status.neutral;
        }
        public void DoInsult() {
            speech.InsultMonologue(lookForTarget.target);
            lookForTarget.target = null;
        }
    }
}