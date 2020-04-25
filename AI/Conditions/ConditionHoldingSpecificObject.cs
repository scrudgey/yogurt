using UnityEngine;

namespace AI {
    public class ConditionHoldingSpecificObject : Condition {
        Ref<GameObject> target;
        Inventory inv;
        public ConditionHoldingSpecificObject(GameObject g, Ref<GameObject> target) : base(g) {
            // conditionThought = "I need a "+t;
            // name = t;
            this.target = target;
            inv = gameObject.GetComponent<Inventory>();
        }
        public override status Evaluate() {
            if (target.val == null)
                return status.failure;
            if (inv) {
                if (inv.holding) {
                    if (inv.holding.gameObject == target.val) {
                        return status.success;
                    } else {
                        return status.failure;
                    }
                } else {
                    return status.failure;
                }
            } else {
                return status.failure;
            }
        }
    }
}