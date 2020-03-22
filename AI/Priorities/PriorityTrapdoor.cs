using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityTrapdoor : Priority {
        public Ref<Vector2> standPoint;
        public Ref<GameObject> player;
        public PriorityTrapdoor(GameObject g, Controllable c, Vector3 guardPoint) : base(g, c) {
            priorityName = "trapdoor";
            player = new Ref<GameObject>(GameManager.Instance.playerObject);
            Goal getToZone = new GoalWalkToPoint(gameObject, control, new Ref<Vector2>((Vector2)guardPoint));
            Goal lookGoal = new GoalLookInDirection(gameObject, control, Vector2.down);
            Goal observe = new GoalObserveObject(gameObject, control, player);
            Goal trap = new GoalTriggerTrapdoor(gameObject, control);
            lookGoal.requirements.Add(getToZone);
            observe.requirements.Add(lookGoal);
            trap.requirements.Add(observe);
            goal = trap;
        }
        public override float Urgency(Personality personality) {
            player.val = GameManager.Instance.playerObject;
            urgency = Priority.urgencySmall;
            return Priority.urgencySmall;
        }
    }
}