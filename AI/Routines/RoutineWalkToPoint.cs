using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineWalkToPoint : Routine {
        public bool invert;
        public float minDistance;
        public Ref<Vector2> target = new Ref<Vector2>(Vector2.zero);
        public RoutineWalkToPoint(GameObject g, Controller c, Ref<Vector2> t, float minDistance, bool invert = false) : base(g, c) {
            routineThought = "I'm walking to a spot.";
            target = t;
            this.minDistance = minDistance;
            this.invert = invert;
        }
        public RoutineWalkToPoint(GameObject g, Controller c, Ref<Vector2> t) : this(g, c, t, 0.1f) { }
        protected override status DoUpdate() {
            float distToTarget = Vector2.Distance(gameObject.transform.position, target.val);
            control.ResetInput();
            if (distToTarget < minDistance) {
                return status.success;
            } else {
                Vector2 comparator = Vector2.zero;

                if (invert) {
                    comparator = target.val - (Vector2)transform.position;
                } else {
                    comparator = (Vector2)transform.position - target.val;
                }

                if (comparator.x > 0) {
                    control.leftFlag = true;
                } else if (comparator.x < 0) {
                    control.rightFlag = true;
                }

                if (comparator.y > 0) {
                    control.downFlag = true;
                } else if (comparator.y < 0) {
                    control.upFlag = true;
                }
                return status.neutral;
            }
        }
    }
}