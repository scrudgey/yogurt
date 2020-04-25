using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalLookInDirection : Goal {
        public Vector2 dir;
        public GoalLookInDirection(GameObject g, Controller c, Vector2 dir) : base(g, c) {
            goalThought = "I'm looking over there.";
            this.dir = dir;
            successCondition = new ConditionLookingInDirection(g, c, dir);
            routines.Add(new RoutineLookInDirection(g, c, dir));
        }
        public override void Update() {
            base.Update();
        }
    }
}
