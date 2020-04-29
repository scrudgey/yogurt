using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityDance : Priority {
        public PriorityDance(GameObject g, Controller c) : base(g, c) {
            priorityName = "dance";

            Goal positionGoal = new GoalWalkToPoint(gameObject, control, new Ref<Vector2>(g.transform.position), minDistance: 0.1f);

            Goal danceGoal = new GoalDance(gameObject, control, g.GetComponent<DecisionMaker>().personality);
            danceGoal.requirements.Add(positionGoal);
            goal = danceGoal;
        }
        public override float Urgency(Personality personality) {
            return urgencyLarge;
        }
    }
}