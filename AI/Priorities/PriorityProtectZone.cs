using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityProtectZone : Priority {
        public Collider2D zone;
        public PriorityProtectZone(GameObject g, Controllable c, Collider2D zone, Vector3 guardPoint) : base(g, c) {
            this.zone = zone;
            priorityName = "protectZone";
            Goal getToZone = new GoalWalkToPoint(gameObject, control, new Ref<Vector2>((Vector2)guardPoint));
            Goal lookGoal = new GoalLookInDirection(gameObject, control, Vector2.right);
            lookGoal.requirements.Add(getToZone);
            goal = lookGoal;
        }
        public override float Urgency(Personality personality) {
            if (zone.bounds.Contains(gameObject.transform.position)) {
                urgency = Priority.urgencyMinor;
                return Priority.urgencyMinor;
            } else {
                urgency = Priority.urgencySmall;
                return Priority.urgencySmall;
            }
        }
    }
}