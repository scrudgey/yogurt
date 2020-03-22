using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityMakeBalloonAnimals : PriorityWander {
        public PriorityMakeBalloonAnimals(GameObject g, Controllable c) : base(g, c) {
            priorityName = "ballon animals";
            goal = new GoalInflateBalloons(g, c);
        }
        public override float Urgency(Personality personality) {
            return Priority.urgencyMinor;
        }
    }
}