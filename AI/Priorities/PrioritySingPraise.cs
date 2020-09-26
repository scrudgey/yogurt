using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PrioritySingPraise : PriorityWander {
        public PrioritySingPraise(GameObject g, Controller c) : base(g, c) {
            priorityName = "sing praise";
            Ref<GameObject> satanObject = new Ref<GameObject>(null);
            satanObject.val = GameObject.Find("Satan");
            GoalWalkToObject walkGoal = new GoalWalkToObject(g, c, satanObject, range: 0.4f);

            GoalInflateBalloons balloonGoal = new GoalInflateBalloons(g, c, "data/dialogue/praise_phrases", low: 2f, high: 10f);
            balloonGoal.requirements.Add(walkGoal);

            goal = balloonGoal;
        }
        public override float Urgency(Personality personality) {
            return Priority.urgencyMinor;
        }
    }
}