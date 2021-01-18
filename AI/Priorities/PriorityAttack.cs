using UnityEngine;
using System.Collections.Generic;
namespace AI {
    public class PriorityAttack : Priority {
        private Inventory inventory;
        private float updateInterval;
        private Goal wanderGoal;
        private Goal fightGoal;
        private Dictionary<BuffType, Buff> netBuffs = new Dictionary<BuffType, Buff>();
        public PriorityAttack(GameObject g, Controller c) : base(g, c) {

            priorityName = "attack";
            inventory = gameObject.GetComponent<Inventory>();

            Goal dukesUp = new GoalDukesUp(gameObject, control, inventory);
            dukesUp.successCondition = new ConditionInFightMode(g, control);

            Goal approachGoal = new GoalWalkToObject(gameObject, control, awareness.nearestEnemy);
            approachGoal.successCondition = new ConditionCloseToObject(gameObject, awareness.nearestEnemy);
            approachGoal.requirements.Add(dukesUp);

            Goal punchGoal = new GoalPunch(gameObject, control, awareness.nearestEnemy, awareness.decisionMaker.personality.combatProficiency);
            punchGoal.requirements.Add(approachGoal);

            // Goal punchGoal = new Goal(gameObject, control);
            // punchGoal.routines.Add(new RoutinePunchAt(gameObject, control, awareness.nearestEnemy, awareness.decisionMaker.personality.combatProficiency));
            fightGoal = punchGoal;
            wanderGoal = new GoalWander(g, c);


            goal = punchGoal;
        }
        public override void ReceiveMessage(Message incoming) {
            if (incoming is MessageDamage) {
                MessageDamage message = (MessageDamage)incoming;
                if (!message.impersonal)
                    urgency += Priority.urgencyLarge;
            }
            if (incoming is MessageInsult) {
                urgency += Priority.urgencySmall;
            }
            if (incoming is MessageThreaten) {
                urgency += Priority.urgencyMinor;
            }
            if (incoming is MessageOccurrence) {
                MessageOccurrence message = (MessageOccurrence)incoming;
                if (message.data is OccurrenceViolence) {
                    OccurrenceViolence dat = (OccurrenceViolence)message.data;
                    if (gameObject == dat.attacker || gameObject == dat.victim)
                        return;
                    urgency += Priority.urgencyMinor;
                }
            }
            if (incoming is MessageNetIntrinsic) {
                MessageNetIntrinsic message = (MessageNetIntrinsic)incoming;
                netBuffs = message.netBuffs;
            }
        }
        public override void Update() {
            if (awareness.nearestEnemy.val == null) {
                urgency -= Time.deltaTime / 10f;
                goal = wanderGoal;
            } else {
                goal = fightGoal;
            }
        }
        public override float Urgency(Personality personality) {
            if (awareness.nearestEnemy.val == null)
                return urgencyMinor;
            if (personality.haunt == Personality.Haunt.yes)
                return Priority.urgencyLarge;
            if (netBuffs != null && netBuffs.ContainsKey(BuffType.enraged) && netBuffs[BuffType.enraged].active())
                return Priority.urgencyLarge;
            if (personality.bravery == Personality.Bravery.brave)
                return urgency * 2f;
            if (personality.bravery == Personality.Bravery.cowardly)
                return urgency / 2f;
            return urgency;
        }
    }
}