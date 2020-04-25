using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineGetRefFromEnvironment : Routine {
        private Inventory inv;
        private Ref<GameObject> target = new Ref<GameObject>(null);
        private RoutineWalkToGameobject walkToRoutine;
        public RoutineGetRefFromEnvironment(GameObject g, Controller c, Ref<GameObject> target) : base(g, c) {
            // routineThought = "I'm going to pick up that " + t + ".";
            this.target = target;
            inv = gameObject.GetComponent<Inventory>();
            Configure();
        }
        public override void Configure() {
            walkToRoutine = new RoutineWalkToGameobject(gameObject, control, target);
        }
        protected override status DoUpdate() {
            if (target.val && target.val.activeInHierarchy) {
                if (Vector2.Distance(transform.position, target.val.transform.position) > 0.2f) {
                    return walkToRoutine.Update();
                } else {
                    // this bit is shakey
                    inv.GetItem(target.val.GetComponent<Pickup>());
                    return status.success;
                }
            } else {
                return status.failure;
            }
        }
    }
}