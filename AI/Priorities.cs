using UnityEngine;

namespace AI {
    [System.Serializable]
    public class Priority : IMessagable {
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
    }
    public class PriorityFightFire : Priority {
        public float updateInterval;
        GoalGetItem getExt;
        GoalUsePhone callFD;
        Goal useFireExtinguisher;
        public PriorityFightFire(GameObject g, Controllable c) : base(g, c) {
            priorityName = "fight fire";
            getExt = new GoalGetItem(gameObject, control, "fire_extinguisher");

            Goal wander = new GoalWander(gameObject, control);
            wander.successCondition = new ConditionKnowAboutFire(gameObject);
            wander.requirements.Add(getExt);

            Goal approach = new GoalWalkToObject(gameObject, control, awareness.nearestFire);
            approach.requirements.Add(wander);

            useFireExtinguisher = new GoalHoseDown(gameObject, control, awareness.nearestFire);
            useFireExtinguisher.requirements.Add(approach);

            goal = useFireExtinguisher;

            callFD = new GoalUsePhone(gameObject, control);
            Goal walkToPhone = new GoalWalkToObject(gameObject, control, typeof(Telephone));
            callFD.requirements.Add(walkToPhone);
        }
        public override void Update() {
            if (awareness.nearestFire.val != null)
                urgency = Priority.urgencyPressing;
            if (getExt.findingFail && goal != callFD && !callFD.phoneCalled) {
                goal = callFD;
            }
            if (goal == callFD && callFD.phoneCalled) {
                goal = useFireExtinguisher;
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
                urgency = 0;
            } else {
                urgency = urgencyLarge;
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
                return Priority.urgencyMinor;
            } else {
                urgency = Priority.urgencySmall;
                return Priority.urgencySmall;
            }
        }
    }
    public class PriorityWander : Priority {
        public PriorityWander(GameObject g, Controllable c) : base(g, c) {
            priorityName = "wander";
            goal = new GoalWander(g, c);
        }
        public override float Urgency(Personality personality) {
            if (personality.actor == Personality.Actor.yes) {
                return -1;
            } else {
                return Priority.urgencyMinor;
            }
        }
    }
    public class PriorityRunAway : Priority {
        public PriorityRunAway(GameObject g, Controllable c) : base(g, c) {
            priorityName = "run away";
            goal = new GoalRunFromObject(gameObject, control, awareness.nearestEnemy);
        }
        public override void ReceiveMessage(Message incoming) {
            if (incoming is MessageDamage) {
                // MessageDamage dam = (MessageDamage)incoming;
                urgency += Priority.urgencyMinor;
            }
            if (incoming is MessageInsult) {
                urgency += Priority.urgencyMinor;
            }
            if (incoming is MessageThreaten) {
                urgency += Priority.urgencySmall;
            }
        }
        public override float Urgency(Personality personality) {
            if (personality.bravery == Personality.Bravery.brave)
                return urgency / 10f;
            if (personality.bravery == Personality.Bravery.cowardly)
                return urgency * 2f;
            return urgency;
        }
        public override void Update() {
            if (awareness.nearestEnemy.val == null)
                urgency -= Time.deltaTime / 10f;
        }
    }
    public class PriorityAttack : Priority {
        private Inventory inventory;
        private float updateInterval;
        public PriorityAttack(GameObject g, Controllable c) : base(g, c) {
            priorityName = "attack";
            inventory = gameObject.GetComponent<Inventory>();

            Goal dukesUp = new GoalDukesUp(gameObject, control, inventory);
            dukesUp.successCondition = new ConditionInFightMode(g, control);

            Goal approachGoal = new GoalWalkToObject(gameObject, control, awareness.nearestEnemy);
            approachGoal.successCondition = new ConditionCloseToObject(gameObject, awareness.nearestEnemy);
            approachGoal.requirements.Add(dukesUp);

            Goal punchGoal = new Goal(gameObject, control);
            punchGoal.routines.Add(new RoutinePunchAt(gameObject, control, awareness.nearestEnemy));
            punchGoal.requirements.Add(approachGoal);

            goal = punchGoal;
        }
        public override void ReceiveMessage(Message incoming) {
            if (incoming is MessageDamage) {
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
        }
        public override void Update() {
            if (awareness.nearestEnemy.val == null)
                urgency -= Time.deltaTime / 10f;
        }
        public override float Urgency(Personality personality) {
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
                    return 0.1f;
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
                VideoCamera director = GameObject.FindObjectOfType<VideoCamera>();
                Vector3 dif = director.transform.position - gameObject.transform.position;
                Vector2 direction = (Vector2)dif;
                control.direction = direction;
                control.SetDirection(direction);

                Toolbox.Instance.SendMessage(gameObject, control, new MessageSpeech(nextLine));
                nextLine = null;
            }
        }
    }
}