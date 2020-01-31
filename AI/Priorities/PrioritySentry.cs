using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PrioritySentry : Priority {
        public PrioritySentry(GameObject g, Controllable c) : base(g, c) {
            priorityName = "sentry";
            goal = new Goal(g, c);
        }
        public override float Urgency(Personality personality) {
            return Priority.urgencyMinor;
        }
    }
}