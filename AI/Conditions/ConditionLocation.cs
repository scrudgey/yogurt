using UnityEngine;

namespace AI {
    public class ConditionLocation : Condition {
        Ref<Vector2> target = new Ref<Vector2>(Vector2.zero);
        public float minDistance = 0.2f;
        public ConditionLocation(GameObject g, Ref<Vector2> t) : base(g) {
            conditionThought = "I need to be over there.";
            target = t;
        }
        public override status Evaluate() {
            if (Vector2.Distance(gameObject.transform.position, target.val) < minDistance) {
                return status.success;
            } else {
                return status.neutral;
            }
        }
    }
}