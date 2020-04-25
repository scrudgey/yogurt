using UnityEngine;

namespace AI {
    public class ConditionObjectInPlace : Condition {
        public Ref<GameObject> target;
        private Ref<Vector2> place;
        public ConditionObjectInPlace(GameObject g, Ref<GameObject> target, Ref<Vector2> place) : base(g) {
            this.target = target;
            this.place = place;
        }
        public override status Evaluate() {
            if (target.val) {
                if (Vector2.Distance(target.val.transform.position, place.val) < 0.10) {
                    return status.success;
                }
                return status.failure;
            }
            return status.failure;
        }
    }
}