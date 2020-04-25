using UnityEngine;
using System.Collections.Generic;
using AI;
namespace AI {
    public class PriorityDeliverPizza : Priority {
        public Ref<GameObject> target = new Ref<GameObject>(null);
        public Ref<GameObject> playerTarget = new Ref<GameObject>(null);
        public ConditionBoolSwitch boolSwitch;
        public bool pizzaDelivered = false;
        public PriorityDeliverPizza(GameObject g, Controller c) : base(g, c) {
            priorityName = "deliver pizza";
            Config();
        }
        public void Config() {
            GoalWalkToObject walkTo = new GoalWalkToObject(gameObject, control, playerTarget, range: 0.25f);
            GoalLookAtObject lookAt = new GoalLookAtObject(gameObject, control, playerTarget);
            GoalDeliverPizza deliver = new GoalDeliverPizza(gameObject, control, playerTarget);
            boolSwitch = deliver.boolSwitch;
            lookAt.requirements.Add(walkTo);
            deliver.requirements.Add(lookAt);

            goal = deliver;
        }
        public override void Update() {
            playerTarget.val = GameManager.Instance.playerObject;
        }

        public override float Urgency(Personality personality) {
            if (!boolSwitch.conditionMet) {
                return urgencyLarge;
            } else {
                return -1f;
            }
        }
    }
}