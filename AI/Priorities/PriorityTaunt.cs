using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityTaunt : Priority {
        public PriorityTaunt(GameObject g, Controller c) : base(g, c) {
            priorityName = "sentry";
            GoalFindTarget findTarget = new GoalFindTarget(g, c);
            GoalTauntTarget tauntTarget = new GoalTauntTarget(g, c, findTarget.routine);
            tauntTarget.requirements.Add(findTarget);
            goal = tauntTarget;
        }
        public override float Urgency(Personality personality) {
            return Priority.urgencyMinor;
        }
    }
}