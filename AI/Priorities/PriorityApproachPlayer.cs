using UnityEngine;
using System.Collections.Generic;
using AI;
namespace AI {
    public class PriorityApproachPlayer : Priority {
        public Ref<GameObject> playerTarget = new Ref<GameObject>(null);
        public ConditionBoolSwitch boolSwitch;
        public bool pizzaDelivered = false;
        public PriorityApproachPlayer(GameObject g, Controller c) : base(g, c) {
            priorityName = "approach the player";
            Config();
        }
        public void Config() {
            GoalWalkToObject walkTo = new GoalWalkToObject(gameObject, control, playerTarget, range: 0.25f);
            GoalLookAtObject lookAt = new GoalLookAtObject(gameObject, control, playerTarget);

            lookAt.requirements.Add(walkTo);

            boolSwitch = new ConditionBoolSwitch(gameObject);
            // ConditionBoolSwitch switchCondition = boolSwitch;
            Goal talkGoal = new Goal(gameObject, control) {
                successCondition = boolSwitch,
                routines = new List<Routine> { new RoutineSpeechWithPerson(gameObject, control, playerTarget, (ConditionBoolSwitch)boolSwitch) }
            };
            talkGoal.requirements.Add(lookAt);

            goal = talkGoal;
        }
        public override void Update() {
            playerTarget.val = GameManager.Instance.playerObject;
        }

        public override float Urgency(Personality personality) {
            if (!boolSwitch.conditionMet) {
                return urgencyMaximum;
            } else {
                return -1f;
            }
        }
    }
}