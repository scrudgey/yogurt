using UnityEngine;

namespace AI {
    public class ConditionLookingAtObject : Condition {
        public Ref<GameObject> target;
        private Controller controller;
        public ConditionLookingAtObject(GameObject g, Controller c, Ref<GameObject> target) : base(g) {
            this.target = target;
            controller = c;
        }
        public override status Evaluate() {
            float angledif = Vector2.Angle(controller.controllable.direction, (Vector2)target.val.transform.position - (Vector2)gameObject.transform.position);
            if (angledif < 20) {
                return status.success;
            } else {
                return status.neutral;
            }
        }
    }
}