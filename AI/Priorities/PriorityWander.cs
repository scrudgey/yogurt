using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityWander : Priority {
        public PriorityWander(GameObject g, Controllable c) : base(g, c) {
            priorityName = "wander";
            goal = new GoalWander(g, c);
        }
        public override float Urgency(Personality personality) {
            return Priority.urgencyMinor;
        }
    }
}