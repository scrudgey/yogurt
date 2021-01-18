using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PrioritySentry : Priority {
        public PrioritySentry(GameObject g, Controller c, Vector3 guardPoint, Vector3 direction) : base(g, c) {
            priorityName = "sentry";
            // goal = new Goal(g, c);
            Goal getToZone = new GoalWalkToPoint(gameObject, control, new Ref<Vector2>((Vector2)guardPoint));
            Goal lookGoal = new GoalLookInDirection(gameObject, control, direction);
            lookGoal.requirements.Add(getToZone);
            goal = lookGoal;
        }
        public override float Urgency(Personality personality) {
            return Priority.urgencyMinor;
        }
    }
}