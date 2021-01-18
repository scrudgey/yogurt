using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalFindTarget : Goal {
        public bool findingFail;
        public RoutineLookForTarget routine;
        public GoalFindTarget(GameObject g, Controller c) : base(g, c) {
            routine = new RoutineLookForTarget(g, c);
            successCondition = new ConditionHasTarget(g, routine);
            routines.Add(routine);
        }
    }
}
