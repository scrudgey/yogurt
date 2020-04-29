using UnityEngine;

namespace AI {
    public class ConditionLookingInDirection : Condition {
        public Vector2 dir;
        public Controller controller;
        public ConditionLookingInDirection(GameObject g, Controller c, Vector2 dir) : base(g) {
            this.dir = dir;
            controller = c;
        }
        public override status Evaluate() {
            if (Vector2.Angle(controller.controllable.direction, dir) < 20) {
                return status.success;
            } else {
                return status.neutral;
            }
        }
    }
}