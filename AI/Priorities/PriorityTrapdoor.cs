using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityTrapdoor : Priority {
        public Ref<Vector2> standPoint;
        public Ref<GameObject> player;
        public bool trapTriggered = false;
        public ConditionBoolSwitch boolSwitch;
        public TrapDoor trapdoor;
        public PriorityTrapdoor(GameObject g, Controllable c, Vector3 guardPoint) : base(g, c) {
            trapdoor = GameObject.Find("trapdoor").GetComponent<TrapDoor>();

            priorityName = "trapdoor";
            player = new Ref<GameObject>(GameManager.Instance.playerObject);
            Goal getToZone = new GoalWalkToPoint(gameObject, control, new Ref<Vector2>((Vector2)guardPoint));
            Goal lookGoal = new GoalLookInDirection(gameObject, control, Vector2.down);
            Goal observe = new GoalObserveObject(gameObject, control, player);
            GoalTriggerTrapdoor trap = new GoalTriggerTrapdoor(gameObject, control);
            boolSwitch = trap.successSwitch;
            lookGoal.requirements.Add(getToZone);
            observe.requirements.Add(lookGoal);
            trap.requirements.Add(observe);
            goal = trap;
        }
        public override float Urgency(Personality personality) {
            player.val = GameManager.Instance.playerObject;
            if (!trapdoor.active) {
                return Priority.urgencySmall; ;
            } else {
                return -1f;
            }
        }
    }
}