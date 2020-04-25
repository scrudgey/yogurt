using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineRetrieveNamedFromInv : Routine {
        private Inventory inv;
        private string targetName;
        public RoutineRetrieveNamedFromInv(GameObject g, Controller c, string names) : base(g, c) {
            routineThought = "I'm checking my pockets for a " + names + ".";
            targetName = names;
            inv = g.GetComponent<Inventory>();
        }
        protected override status DoUpdate() {
            if (inv) {
                foreach (GameObject g in inv.items) {
                    if (targetName == g.name) {
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