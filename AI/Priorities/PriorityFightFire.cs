using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityFightFire : Priority {
        public float updateInterval;
        GoalGetItem getExt;
        GoalUsePhone callFD;
        Goal useFireExtinguisher;
        private Dictionary<BuffType, Buff> netBuffs = new Dictionary<BuffType, Buff>();
        public PriorityFightFire(GameObject g, Controller c) : base(g, c) {
            priorityName = "fight fire";
            getExt = new GoalGetItem(gameObject, control, "fire_extinguisher");

            Goal wander = new GoalWander(gameObject, control);
            wander.successCondition = new ConditionKnowAboutFire(gameObject);
            wander.requirements.Add(getExt);

            Goal approach = new GoalWalkToObject(gameObject, control, awareness.nearestFire, range: 0.6f);
            approach.requirements.Add(wander);

            useFireExtinguisher = new GoalHoseDown(gameObject, control, awareness.nearestFire);
            useFireExtinguisher.requirements.Add(approach);

            goal = useFireExtinguisher;

            callFD = new GoalUsePhone(gameObject, control);
            Goal walkToPhone = new GoalWalkToObject(gameObject, control, typeof(Telephone));
            callFD.requirements.Add(walkToPhone);
        }
        public override void Update() {
            if (awareness.nearestFire.val != null) {
                if (netBuffs != null && netBuffs.ContainsKey(BuffType.enraged) && netBuffs[BuffType.enraged].active())
                    urgency = Priority.urgencyMinor;
                else
                    urgency = Priority.urgencyPressing;
            } else {
                if (urgency > 0) {
                    urgency -= Time.deltaTime;
                }
            }
            if (getExt.findingFail && goal != callFD && !callFD.phoneCalled) {
                goal = callFD;
            }
            if (goal == callFD && callFD.phoneCalled) {
                goal = useFireExtinguisher;
            }
            if (getExt.findingFail && callFD.findingFail) {
                urgency = 0;
            }
        }
        public override void ReceiveMessage(Message incoming) {
            if (incoming is MessageNetIntrinsic) {
                MessageNetIntrinsic message = (MessageNetIntrinsic)incoming;
                netBuffs = message.netBuffs;
            }
        }
    }
}