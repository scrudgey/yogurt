using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityTaunt : Priority {
        public PriorityTaunt(GameObject g, Controller c) : base(g, c) {
            priorityName = "taunt";
            GoalFindTarget findTarget = new GoalFindTarget(g, c);
            GoalTauntTarget tauntTarget = new GoalTauntTarget(g, c, findTarget.routine);
            tauntTarget.requirements.Add(findTarget);
            goal = tauntTarget;
        }
        public override float Urgency(Personality personality) {
            if (personality.insulter == Personality.Insulter.yes) {
                return urgency;
            } else return Priority.urgencyMinor;
        }
        public override void Update() {
            base.Update();
            if (Random.Range(0f, 1000f) < 1f) {
                urgency = Priority.urgencySmall;
            }
            urgency -= Time.deltaTime;
            urgency = Mathf.Max(urgency, 0f);
        }
    }
}