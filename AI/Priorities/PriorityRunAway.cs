using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityRunAway : Priority {
        public Ref<GameObject> lastAttacker = new Ref<GameObject>(null);
        public Ref<Vector2> lastDamageArea = new Ref<Vector2>(Vector2.zero);
        private GoalWalkToObject goalRunFromObject;
        // private GoalWalkToPoint goalRunFromPoint;
        public PriorityRunAway(GameObject g, Controller c) : base(g, c) {
            priorityName = "run away";
            goalRunFromObject = new GoalWalkToObject(gameObject, control, lastAttacker, invert: true);
            goal = goalRunFromObject;
        }
        public override void ReceiveMessage(Message incoming) {
            // TODO: switch goals 
            if (incoming is MessageDamage) {
                MessageDamage dam = (MessageDamage)incoming;

                if (dam.type == damageType.asphyxiation)
                    return;

                lastAttacker.val = dam.messenger.gameObject;

                if (dam.type == damageType.fire) {
                    urgency = Priority.urgencySmall;
                } else {
                    urgency += Priority.urgencyMinor;
                }
            }
            if (incoming is MessageInsult) {
                MessageInsult dam = (MessageInsult)incoming;
                lastAttacker.val = dam.messenger.gameObject;
                urgency += Priority.urgencyMinor;
            }
            if (incoming is MessageThreaten) {
                MessageThreaten dam = (MessageThreaten)incoming;
                lastAttacker.val = dam.messenger.gameObject;
                // urgency += Priority.urgencySmall;
                urgency += 3f;
            }
        }
        public override float Urgency(Personality personality) {
            if (personality.bravery == Personality.Bravery.brave) {
                return urgency / 10f;
            }
            if (personality.bravery == Personality.Bravery.cowardly)
                return urgency * 2f;
            return urgency;
        }
        public override void Update() {
            if (awareness.nearestEnemy.val == null)
                urgency -= Time.deltaTime / 2f;
        }
    }
}