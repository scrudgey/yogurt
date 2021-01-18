using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalTauntTarget : Goal {
        public bool findingFail;

        public GoalTauntTarget(GameObject g, Controller c, RoutineLookForTarget finder) : base(g, c) {
            RoutineInsultTarget insultRoutine = new RoutineInsultTarget(g, c, finder);
            routines.Add(insultRoutine);
        }
    }
}
