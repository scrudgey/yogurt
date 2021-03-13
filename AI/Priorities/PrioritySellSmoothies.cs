using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PrioritySellSmoothies : Priority {
        public Ref<int> smoothieOrder = new Ref<int>(-1);
        public Ref<GameObject> playerObject = new Ref<GameObject>(null);
        // public Ref<GameObject> cauldronObject = new Ref<GameObject>(null);
        public LambdaRef<GameObject> cauldronObject = new LambdaRef<GameObject>(null, () => {
            return GameObject.Find("blendingCauldron");
        });
        public PrioritySellSmoothies(GameObject g, Controller c, Vector3 guardPoint) : base(g, c) {
            priorityName = "sellSmoothies";

            // wait for order
            // 1. get to zone
            // 2. look in direction
            // 3. call out
            // blend ingredient
            // 1. hold item
            // 2. walk to blender
            // 3. put in blender
            // 4. turn on blender
            // condition: timer with blender on > 1f
            // deliver smoothie
            // 1. hold smoothie
            // 2. walk to player
            // 3. deliver smoothie

            Goal getToZone = new GoalWalkToPoint(gameObject, control, new Ref<Vector2>((Vector2)guardPoint));

            Goal lookGoal = new GoalLookInDirection(gameObject, control, Vector2.down);
            lookGoal.requirements.Add(getToZone);

            Goal takeSmoothieOrderGoal = new GoalGetSmoothieOrder(gameObject, c, smoothieOrder);
            takeSmoothieOrderGoal.requirements.Add(lookGoal);
            takeSmoothieOrderGoal.ignoreRequirementsIfConditionMet = true;

            Goal holdIngredientGoal = new GoalGetSmoothieIngredient(gameObject, control, smoothieOrder);
            holdIngredientGoal.requirements.Add(takeSmoothieOrderGoal);
            holdIngredientGoal.requirements.Add(lookGoal);
            holdIngredientGoal.ignoreRequirementsIfConditionMet = true;

            Goal approach = new GoalWalkToObject(gameObject, control, cauldronObject, range: 0.1f);
            approach.requirements.Add(holdIngredientGoal);
            approach.ignoreRequirementsIfConditionMet = true;

            GoalBlendSmoothie blendIngredientGoal = new GoalBlendSmoothie(gameObject, c, cauldronObject, smoothieOrder);
            blendIngredientGoal.requirements.Add(approach);
            blendIngredientGoal.ignoreRequirementsIfConditionMet = true;

            Goal walkToPlayerGoal = new GoalWalkToObject(gameObject, c, playerObject);
            walkToPlayerGoal.requirements.Add(blendIngredientGoal);

            Goal deliverSmoothieGoal = new GoalDeliverSmoothie(gameObject, c, playerObject, blendIngredientGoal);
            deliverSmoothieGoal.requirements.Add(walkToPlayerGoal);

            goal = deliverSmoothieGoal;
        }
        public override void Update() {
            base.Update();
            playerObject.val = GameManager.Instance.playerObject;
        }
        public override float Urgency(Personality personality) {
            // if (smoothieOrder.val == -1) {
            // return Priority.urgencySmall;
            // } else {
            return Priority.urgencyLarge;
            // }
        }

        public override void ReceiveMessage(Message incoming) {
            if (incoming is MessageSmoothieOrder) {
                MessageSmoothieOrder message = (MessageSmoothieOrder)incoming;
                smoothieOrder.val = message.idn;
            }
        }
    }
}