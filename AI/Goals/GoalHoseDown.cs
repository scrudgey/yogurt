using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalHoseDown : Goal {
        public Ref<GameObject> target;
        public new string goalThought {
            get { return "I've got to do something about that " + target.val.name + "."; }
        }
        public GoalHoseDown(GameObject g, Controller c, Ref<GameObject> r) : base(g, c) {
            successCondition = new ConditionFail(g);
            RoutineUseObjectOnTarget hoseRoutine = new RoutineUseObjectOnTarget(g, c, r);
            RoutineWander wanderRoutine = new RoutineWander(g, c);
            hoseRoutine.timeLimit = 1;
            wanderRoutine.timeLimit = 1;
            routines.Add(hoseRoutine);
            routines.Add(wanderRoutine);
        }
    }
}
