using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineRetrieveRefFromInv : Routine {
        private Inventory inv;
        private Ref<GameObject> target;
        public RoutineRetrieveRefFromInv(GameObject g, Controller c, Ref<GameObject> target) : base(g, c) {
            // routineThought = "I'm checking my pockets for a " + names + ".";
            this.target = target;
            inv = g.GetComponent<Inventory>();
        }
        protected override status DoUpdate() {
            if (inv) {
                foreach (GameObject g in inv.items) {
                    if (target.val == g) {
                        inv.RetrieveItem(g.name);
                        return status.success;
                    }
                }
                return status.failure;
            } else {
                return status.failure;
            }
        }
    }
}