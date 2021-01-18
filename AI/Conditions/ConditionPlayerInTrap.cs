using UnityEngine;

namespace AI {
    public class ConditionPlayerInTrap : Condition {
        public Ref<GameObject> target;
        public PolygonCollider2D trapCollider;
        public ConditionPlayerInTrap(GameObject g, Ref<GameObject> target, TrapDoor trapDoor) : base(g) {
            this.target = target;
            if (trapDoor != null) {
                trapCollider = trapDoor.GetComponent<PolygonCollider2D>();
            } else { trapCollider = null; }
        }
        public override status Evaluate() {
            if (trapCollider == null)
                return status.failure;
            Vector3 position = target.val.transform.position - new Vector3(0, 0.165f, 0f);

            if (trapCollider.OverlapPoint(position)) {
                return status.success;
            } else return status.neutral;
        }
    }
}