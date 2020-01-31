using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PrioritySocialize : Priority {
        public Ref<GameObject> target = new Ref<GameObject>(null);
        public Ref<GameObject> peopleTarget = new Ref<GameObject>(null);
        public PrioritySocialize(GameObject g, Controllable c) : base(g, c) {
            priorityName = "socialize";
            Config();
        }
        public void Config() {
            GoalWalkToObject walkTo = new GoalWalkToObject(gameObject, control, target, range: 1f);
            GoalLookAtObject lookAt = new GoalLookAtObject(gameObject, control, target);
            GoalTalkToPerson talkTo = new GoalTalkToPerson(gameObject, control, awareness, peopleTarget);

            lookAt.requirements.Add(walkTo);
            talkTo.requirements.Add(lookAt);

            goal = talkTo;
        }
        public override void Update() {
            if (awareness.socializationTimer < 0) {
                urgency = 0;
                return;
            }
            if (awareness.newPeopleList.Count > 0) {
                if (awareness.newPeopleList[0].val != target.val) {
                    target.val = awareness.newPeopleList[0].val;
                    peopleTarget.val = awareness.newPeopleList[0].val;
                    urgency = urgencySmall;
                    Config();
                }
            }
            if (awareness.newPeopleList.Count == 0) {
                urgency = 0;
                peopleTarget.val = null;
                target.val = null;
            }
        }
    }
}