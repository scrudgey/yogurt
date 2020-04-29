using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineLookInDirection : Routine {
        public Vector2 dir;
        public RoutineLookInDirection(GameObject g, Controller c, Vector2 dir) : base(g, c) {
            this.dir = dir;
        }
        protected override status DoUpdate() {
            control.ResetInput();
            control.SetDirection(dir);
            return status.neutral;
        }
    }
}