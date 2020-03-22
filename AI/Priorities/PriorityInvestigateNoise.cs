using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityInvestigateNoise : Priority {
        public Ref<Vector2> lastHeardNoise = new Ref<Vector2>(Vector2.zero);
        public PriorityInvestigateNoise(GameObject g, Controllable c) : base(g, c) {
            priorityName = "investigate noise";

            GoalWalkToPoint walkTo = new GoalWalkToPoint(g, c, lastHeardNoise, 0.3f, jitter: true);

            goal = walkTo;
        }
        public override void ReceiveMessage(Message incoming) {
            if (incoming is MessageNoise) {
                MessageNoise message = (MessageNoise)incoming;
                lastHeardNoise.val = message.location;
                urgency = Priority.urgencySmall;
            }
        }
        public override void Update() {
            urgency -= Time.deltaTime;
        }
    }
}