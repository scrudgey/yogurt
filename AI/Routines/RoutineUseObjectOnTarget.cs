using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineUseObjectOnTarget : Routine {
        public Ref<GameObject> target;
        public RoutineUseObjectOnTarget(GameObject g, Controller c, Ref<GameObject> targetObject) : base(g, c) {
            routineThought = "I'm using this object on " + g.name + ".";
            target = targetObject;
        }
        protected override status DoUpdate() {
            if (target.val != null) {
                control.SetDirection(Vector2.ClampMagnitude(target.val.transform.position - gameObject.transform.position, 1f));
                control.ShootHeld();
                return status.neutral;
            } else {
                return status.failure;
            }
        }
    }

}