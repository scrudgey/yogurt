using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityProtectPossessions : Priority {
        Ref<Vector2> returnPosition = new Ref<Vector2>(Vector2.zero);
        Ref<GameObject> possession = new Ref<GameObject>(null);
        public PriorityProtectPossessions(GameObject g, Controllable c) : base(g, c) {
            priorityName = "possessions";
            Goal findPossession = new GoalWander(gameObject, control);
            findPossession.successCondition = new ConditionSawObjectRecently(gameObject, control, possession);

            Goal gainPossession = new GoalGetItem(gameObject, control, possession);
            Goal walkToReturnPoint = new GoalWalkToPoint(gameObject, control, returnPosition);
            Goal returnObject = new Goal(gameObject, control);
            returnObject.routines.Add(new RoutinePlaceObject(gameObject, control, returnPosition));
            returnObject.successCondition = new ConditionObjectInPlace(gameObject, control, possession, returnPosition);

            returnObject.requirements.Add(walkToReturnPoint);
            walkToReturnPoint.requirements.Add(gainPossession);
            gainPossession.requirements.Add(findPossession);

            goal = returnObject;
        }
        public override void Update() {
            if (awareness.PossessionsAreOkay()) {
                urgency = -1;
            } else {
                urgency = urgencySmall;
                possession.val = awareness.possession;
                returnPosition.val = awareness.possessionDefaultState.lastSeenPosition;
            }
        }
    }
}