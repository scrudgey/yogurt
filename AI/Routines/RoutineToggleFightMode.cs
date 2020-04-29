using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineToggleFightMode : Routine {
        public RoutineToggleFightMode(GameObject g, Controller c) : base(g, c) {
            routineThought = "I need to prepare for battle!";
        }
        protected override status DoUpdate() {
            control.ToggleFightMode();
            return status.success;
        }
    }

}