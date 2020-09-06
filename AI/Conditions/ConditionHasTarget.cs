using UnityEngine;

namespace AI {
    public class ConditionHasTarget : Condition {
        RoutineLookForTarget routine;
        public ConditionHasTarget(GameObject g, RoutineLookForTarget routine) : base(g) {
            conditionThought = "I need a target.";
            this.routine = routine;
        }
        public override status Evaluate() {
            if (routine != null) {
                if (routine.target != null) {
                    return status.success;
                } else {
                    return status.neutral;
                }
            } else {
                return status.failure;
            }
        }
    }
}