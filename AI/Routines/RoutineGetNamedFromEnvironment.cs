using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineGetNamedFromEnvironment : Routine {
        private Inventory inv;
        private Awareness awareness;
        private GameObject target;
        private string targetName;
        private RoutineWalkToGameobject walkToRoutine;
        public RoutineGetNamedFromEnvironment(GameObject g, Controller c, string t) : base(g, c) {
            routineThought = "I'm going to pick up that " + t + ".";
            targetName = t;
            inv = gameObject.GetComponent<Inventory>();
            awareness = gameObject.GetComponent<Awareness>();
            Configure();
        }
        public override void Configure() {
            List<GameObject> objs = new List<GameObject>();
            if (awareness) {
                objs = awareness.FindObjectWithName(targetName);
            }
            if (objs.Count > 0) {
                target = objs[0];
            }
            if (target && target.activeInHierarchy) {
                walkToRoutine = new RoutineWalkToGameobject(gameObject, control, new Ref<GameObject>(target));
            }
        }
        protected override status DoUpdate() {
            if (target && target.activeInHierarchy) {
                if (Vector2.Distance(transform.position, target.transform.position) > 0.2f) {
                    return walkToRoutine.Update();
                } else {
                    // this bit is shakey
                    inv.GetItem(target.GetComponent<Pickup>());
                    return status.success;
                }
            } else {
                return status.failure;
            }
        }
    }
}