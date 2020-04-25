using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineLookAtObject : Routine {
        public Ref<GameObject> target = new Ref<GameObject>(null);
        public RoutineLookAtObject(GameObject g, Controller c, Ref<GameObject> target) : base(g, c) {
            this.target = target;
        }
        protected override status DoUpdate() {
            control.ResetInput();
            control.LookAtPoint(target.val.transform.position);
            return status.neutral;
        }
    }
}