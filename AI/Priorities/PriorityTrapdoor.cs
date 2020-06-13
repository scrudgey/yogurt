using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace AI {
    public class PriorityTrapdoor : Priority {
        public Ref<Vector2> standPoint;
        public Ref<GameObject> player;
        public bool trapTriggered = false;
        public ConditionBoolSwitch boolSwitch;
        public TrapDoor trapdoor;
        public bool atMansion;
        public PriorityTrapdoor(GameObject g, Controller c, Vector3 guardPoint) : base(g, c) {
            GameObject trapdoorObj = GameObject.Find("trapdoor");
            if (trapdoorObj != null)
                trapdoor = trapdoorObj.GetComponent<TrapDoor>();

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

            atMansion = SceneManager.GetActiveScene().name == "vampire_house";
        }
        public override float Urgency(Personality personality) {
            if (!atMansion)
                return -1f;
            player.val = GameManager.Instance.playerObject;
            if (trapdoor != null && !trapdoor.active) {
                return Priority.urgencySmall;
            } else {
                return -1f;
            }
        }
    }
}