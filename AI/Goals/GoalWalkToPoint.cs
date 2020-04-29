using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalWalkToPoint : Goal {
        public Ref<Vector2> target;
        public GoalWalkToPoint(GameObject g, Controller c, Ref<Vector2> target, float minDistance = 0.2f, bool invert = false, bool jitter = false) : base(g, c) {
            goalThought = "I want to be over there.";
            this.target = target;
            ConditionLocation condition = new ConditionLocation(g, target);
            condition.minDistance = minDistance;
            successCondition = condition;

            RoutineWalkToPoint routineWalk = new RoutineWalkToPoint(g, c, target, minDistance, invert: invert);
            routines.Add(routineWalk);
            if (jitter) {
                RoutineWander wanderRoutine = new RoutineWander(g, c);
                routineWalk.timeLimit = 0.5f;
                wanderRoutine.timeLimit = 0.5f;

                routines.Add(wanderRoutine);
            }
        }
        public GoalWalkToPoint(GameObject g, Controller c, Ref<Vector2> target) : this(g, c, target, minDistance: 0.2f, invert: false) { }
    }
}
