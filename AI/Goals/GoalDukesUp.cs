using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalDukesUp : Goal {
        public GoalDukesUp(GameObject g, Controller c, Inventory i) : base(g, c) {
            successCondition = new ConditionInFightMode(g, control);
            // TODO: add a routine for checking inventory
            routines.Add(new RoutineToggleFightMode(g, c));
            routines.Add(new RoutineToggleFightMode(g, c));
        }
    }
}
