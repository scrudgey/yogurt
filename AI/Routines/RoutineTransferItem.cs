using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineTransferItem : Routine {
        public Ref<GameObject> target;
        public GoalBlendSmoothie smoothieGoal;
        public Inventory inventory;
        public RoutineTransferItem(GameObject g, Controller c, Ref<GameObject> target, GoalBlendSmoothie smoothieGoal) : base(g, c) {
            this.target = target;
            this.smoothieGoal = smoothieGoal;
            inventory = gameObject.GetComponent<Inventory>();
        }
        protected override status DoUpdate() {
            if (target.val == null)
                return status.neutral;
            if (inventory.holding != null) {
                // condition.conditionMet = true;
                Inventory theirInv = target.val.GetComponent<Inventory>();
                if (theirInv != null) {
                    theirInv.GetItem(inventory.holding);
                } else {
                    inventory.DropItem();
                }
                smoothieGoal.Reset();
                return status.success;
            } else {
                smoothieGoal.Reset();
                return status.failure;
            }
        }
    }
}