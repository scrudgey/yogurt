using UnityEngine;
using System.Collections.Generic;

namespace AI {
    [System.Serializable]
    public class Priority {
        public string priorityName = "priority";
        public const float urgencyMinor = 1f;
        public const float urgencySmall = 2.5f;
        public const float urgencyLarge = 5f;
        public const float urgencyPressing = 10f;
        public const float urgencyMaximum = 10f;
        public float minimumUrgency = 0;
        public float urgency;
        public Awareness awareness;
        public Controllable control;
        public GameObject gameObject;
        public Goal goal;
        public Priority(GameObject g, Controllable c) {
            InitReferences(g, c);
        }
        public void InitReferences(GameObject g, Controllable c) {
            gameObject = g;
            control = c;
            awareness = g.GetComponent<Awareness>();
        }
        public virtual void Update() { }
        public virtual void DoAct() {
            if (goal != null) {
                goal.Update();
            }
        }
        public virtual float Urgency(Personality personality) {
            return urgency;
        }
        public virtual void ReceiveMessage(Message m) { }
        // public virtual void ObserveOccurrence(OccurrenceData data){}
        public virtual void EnterPriority() { }
        public virtual void ExitPriority() { }
    }
    public class PriorityFightFire : Priority {
        public float updateInterval;
        GoalGetItem getExt;
        GoalUsePhone callFD;
        Goal useFireExtinguisher;
        private Dictionary<BuffType, Buff> netBuffs = new Dictionary<BuffType, Buff>();
        public PriorityFightFire(GameObject g, Controllable c) : base(g, c) {
            priorityName = "fight fire";
            getExt = new GoalGetItem(gameObject, control, "fire_extinguisher");

            Goal wander = new GoalWander(gameObject, control);
            wander.successCondition = new ConditionKnowAboutFire(gameObject);
            wander.requirements.Add(getExt);

            Goal approach = new GoalWalkToObject(gameObject, control, awareness.nearestFire, range: 0.6f);
            approach.requirements.Add(wander);

            useFireExtinguisher = new GoalHoseDown(gameObject, control, awareness.nearestFire);
            useFireExtinguisher.requirements.Add(approach);

            goal = useFireExtinguisher;

            callFD = new GoalUsePhone(gameObject, control);
            Goal walkToPhone = new GoalWalkToObject(gameObject, control, typeof(Telephone));
            callFD.requirements.Add(walkToPhone);
        }
        public override void Update() {
            if (awareness.nearestFire.val != null) {
                if (netBuffs != null && netBuffs.ContainsKey(BuffType.enraged) && netBuffs[BuffType.enraged].active())
                    urgency = Priority.urgencyMinor;
                else
                    urgency = Priority.urgencyPressing;
            } else {
                if (urgency > 0) {
                    urgency -= Time.deltaTime;
                }
            }
            if (getExt.findingFail && goal != callFD && !callFD.phoneCalled) {
                goal = callFD;
            }
            if (goal == callFD && callFD.phoneCalled) {
                goal = useFireExtinguisher;
            }
        }
        public override void ReceiveMessage(Message incoming) {
            if (incoming is MessageNetIntrinsic) {
                MessageNetIntrinsic message = (MessageNetIntrinsic)incoming;
                netBuffs = message.netBuffs;
            }
        }
    }
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
    public class PriorityDeliverPizza : Priority {
        public Ref<GameObject> target = new Ref<GameObject>(null);
        public Ref<GameObject> playerTarget = new Ref<GameObject>(null);
        public ConditionBoolSwitch boolSwitch;
        public bool pizzaDelivered = false;
        public PriorityDeliverPizza(GameObject g, Controllable c) : base(g, c) {
            priorityName = "deliver pizza";
            Config();
        }
        public void Config() {
            GoalWalkToObject walkTo = new GoalWalkToObject(gameObject, control, playerTarget, range: 1f);
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
    public class PriorityProtectZone : Priority {
        public Collider2D zone;
        public PriorityProtectZone(GameObject g, Controllable c, Collider2D zone, Vector3 guardPoint) : base(g, c) {
            this.zone = zone;
            priorityName = "protectZone";
            Goal getToZone = new GoalWalkToPoint(gameObject, control, new Ref<Vector2>((Vector2)guardPoint));
            Goal lookGoal = new GoalLookInDirection(gameObject, control, Vector2.right);
            lookGoal.requirements.Add(getToZone);
            goal = lookGoal;
        }
        public override float Urgency(Personality personality) {
            if (zone.bounds.Contains(gameObject.transform.position)) {
                urgency = Priority.urgencyMinor;
                return Priority.urgencyMinor;
            } else {
                urgency = Priority.urgencySmall;
                return Priority.urgencySmall;
            }
        }
    }
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
    public class PrioritySentry : Priority {
        public PrioritySentry(GameObject g, Controllable c) : base(g, c) {
            priorityName = "sentry";
            goal = new Goal(g, c);
        }
        public override float Urgency(Personality personality) {
            return Priority.urgencyMinor;
        }
    }
    public class PriorityWander : Priority {
        public PriorityWander(GameObject g, Controllable c) : base(g, c) {
            priorityName = "wander";
            goal = new GoalWander(g, c);
        }
        public override float Urgency(Personality personality) {
            return Priority.urgencyMinor;
        }
    }
    public class PriorityMakeBalloonAnimals : PriorityWander {
        public PriorityMakeBalloonAnimals(GameObject g, Controllable c) : base(g, c) {
            priorityName = "ballon animals";
            goal = new GoalInflateBalloons(g, c);
        }
        public override float Urgency(Personality personality) {
            return Priority.urgencyMinor;
        }
    }
    public class PriorityPanic : Priority {
        public PriorityPanic(GameObject g, Controllable c) : base(g, c) {
            priorityName = "panic";
            goal = new GoalPanic(g, c);
        }
        public override void Update() {
            if (urgency > 0) {
                urgency -= Time.deltaTime;
            }
        }
        public override float Urgency(Personality personality) {
            if (awareness.imOnFire) {
                return 2 * Priority.urgencyMaximum;
            }
            if (personality.bravery == Personality.Bravery.brave) {
                return urgency / 100f;
            }
            // return urgency / 100f;
            if (personality.bravery == Personality.Bravery.cowardly)
                return urgency * 2f;
            return urgency;
        }
        public override void ReceiveMessage(Message incoming) {
            if (awareness.imOnFire)
                if (awareness.decisionMaker.personality.bravery == Personality.Bravery.cowardly) {
                    if (incoming is MessageDamage) {
                        urgency += Priority.urgencyMinor / 2f;
                    }
                    if (incoming is MessageThreaten) {
                        urgency += Priority.urgencyMinor / 2f;
                    }
                }
            if (incoming is MessageOccurrence) {
                MessageOccurrence message = (MessageOccurrence)incoming;
                foreach (EventData data in message.data.events) {
                    urgency += data.ratings[Rating.disturbing] / 2f;
                }
            }
        }
    }
    public class PriorityRunAway : Priority {
        public Ref<GameObject> lastAttacker = new Ref<GameObject>(null);
        public Ref<Vector2> lastDamageArea = new Ref<Vector2>(Vector2.zero);
        private GoalWalkToObject goalRunFromObject;
        // private GoalWalkToPoint goalRunFromPoint;
        public PriorityRunAway(GameObject g, Controllable c) : base(g, c) {
            priorityName = "run away";
            goalRunFromObject = new GoalWalkToObject(gameObject, control, lastAttacker, invert: true);
            // goalRunFromPoint = new GoalWalkToPoint(gameObject, control, lastDamageArea, invert: true);
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
                urgency += Priority.urgencyMinor;
            }
            if (incoming is MessageThreaten) {
                urgency += Priority.urgencySmall;
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
    public class PriorityInvestigateNoise : Priority {
        public Ref<Vector2> lastHeardNoise = new Ref<Vector2>(Vector2.zero);
        public PriorityInvestigateNoise(GameObject g, Controllable c) : base(g, c) {
            priorityName = "investigate noise";

            GoalWalkToPoint walkTo = new GoalWalkToPoint(g, c, lastHeardNoise, 0.3f);

            goal = walkTo;
        }
        public override void ReceiveMessage(Message incoming) {
            if (incoming is MessageNoise) {
                MessageNoise message = (MessageNoise)incoming;
                lastHeardNoise.val = message.location;
                urgency = Priority.urgencyLarge;
            }
        }
        public override void Update() {
            urgency -= Time.deltaTime;
        }
    }
    public class PriorityAttack : Priority {
        private Inventory inventory;
        private float updateInterval;
        private Dictionary<BuffType, Buff> netBuffs = new Dictionary<BuffType, Buff>();
        public PriorityAttack(GameObject g, Controllable c) : base(g, c) {
            priorityName = "attack";
            inventory = gameObject.GetComponent<Inventory>();

            Goal dukesUp = new GoalDukesUp(gameObject, control, inventory);
            dukesUp.successCondition = new ConditionInFightMode(g, control);

            Goal approachGoal = new GoalWalkToObject(gameObject, control, awareness.nearestEnemy);
            approachGoal.successCondition = new ConditionCloseToObject(gameObject, awareness.nearestEnemy);
            approachGoal.requirements.Add(dukesUp);

            Goal punchGoal = new Goal(gameObject, control);
            punchGoal.routines.Add(new RoutinePunchAt(gameObject, control, awareness.nearestEnemy, awareness.decisionMaker.personality.combatProficiency));
            punchGoal.requirements.Add(approachGoal);

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
            if (awareness.nearestEnemy.val == null)
                urgency -= Time.deltaTime / 10f;
        }
        public override float Urgency(Personality personality) {
            if (netBuffs != null && netBuffs.ContainsKey(BuffType.enraged) && netBuffs[BuffType.enraged].active())
                return Priority.urgencyLarge;
            if (personality.bravery == Personality.Bravery.brave)
                return urgency * 2f;
            if (personality.bravery == Personality.Bravery.cowardly)
                return urgency / 2f;
            return urgency;
        }
    }
    public class PriorityReadScript : Priority {
        string nextLine;
        // ScriptDirector director;
        public PriorityReadScript(GameObject g, Controllable c) : base(g, c) {
            priorityName = "readscript";
            VideoCamera video = GameObject.FindObjectOfType<VideoCamera>();
            Goal goalWalkTo = new GoalWalkToPoint(g, c, new Ref<Vector2>(new Vector2(0.186f, 0.812f)));
            if (video != null) {
                Goal lookGoal = new GoalLookAtObject(g, c, new Ref<GameObject>(video.gameObject));
                lookGoal.requirements.Add(goalWalkTo);
                goal = lookGoal;
            }
        }
        public override float Urgency(Personality personality) {
            if (personality.actor == Personality.Actor.yes) {
                if (nextLine != null) {
                    return Priority.urgencyPressing;
                } else {
                    return Priority.urgencyMinor;
                }
            } else {
                return -1;
            }
        }
        public override void DoAct() {
            if (goal != null) {
                goal.Update();
            }
            if (nextLine != null) {
                // VideoCamera director = GameObject.FindObjectOfType<VideoCamera>();
                // Vector3 dif = director.transform.position - gameObject.transform.position;
                // Vector2 direction = (Vector2)dif;
                // control.direction = direction;
                // control.SetDirection(direction);

                Toolbox.Instance.SendMessage(gameObject, control, new MessageSpeech(nextLine));
                nextLine = null;
            }
        }
    }
}