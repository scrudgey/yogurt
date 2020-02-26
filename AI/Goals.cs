using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class Goal {
        public List<Routine> routines = new List<Routine>();
        public int index = 0;
        public List<Goal> requirements = new List<Goal>();
        public Condition successCondition;
        public GameObject gameObject;
        public Controllable control;
        public float slewTime;
        private bool fulfillingRequirements = true;
        public string goalThought = "I'm just doing my thing.";
        public bool ignoreRequirementsIfConditionMet;
        public Goal(GameObject g, Controllable c) {
            gameObject = g;
            control = c;
            slewTime = UnityEngine.Random.Range(0.1f, 0.5f);
        }
        public status Evaluate() {
            if (ignoreRequirementsIfConditionMet && successCondition.Evaluate() == status.success)
                return status.success;
            foreach (Goal requirement in requirements) {
                if (requirement.Evaluate() != status.success) {
                    return status.failure;
                }
            }
            return successCondition.Evaluate();
        }
        public virtual void Update() {

            // if i have any unmet requirements, my update goes to the first unmet one.
            if (!(ignoreRequirementsIfConditionMet && successCondition.Evaluate() == status.success)) {
                foreach (Goal requirement in requirements) {
                    if (requirement.Evaluate() != status.success) {
                        fulfillingRequirements = true;
                        requirement.Update();
                        return;
                    }
                }
            }

            if (fulfillingRequirements) {
                // Debug.Log(control.gameObject.name + " " + this.ToString() + " requirements met"); ;
                control.ResetInput();
            }
            fulfillingRequirements = false;
            if (slewTime > 0) {
                slewTime -= Time.deltaTime;
                return;
            }
            if (routines.Count > 0) {
                try {
                    status routineStatus = routines[index].Update();
                    // Debug.Log(routines[index].GetType().ToString());
                    // Debug.Log(routineStatus);
                    if (routineStatus == status.failure) {
                        control.ResetInput();
                        index++;
                        // get next routine, or fail.
                        if (index < routines.Count) {
                            slewTime = UnityEngine.Random.Range(0.1f, 0.5f);
                        } else {
                            // what do do? reset from the start maybe
                            // index = routines.Count - 1;
                            index = 0;
                        }
                        routines[index].Configure();
                    }
                }
                catch (Exception e) {
                    Debug.LogError(this.ToString() + " fail: " + e.Message);
                    Debug.LogError(e.StackTrace);
                    // Debug.Log(e.TargetSite);
                }
            }
        }
    }
    public class GoalUsePhone : Goal {
        public bool phoneCalled;
        private RoutineUseTelephone telRoutine;
        public GoalUsePhone(GameObject g, Controllable c) : base(g, c) {
            successCondition = new ConditionBoolSwitch(g);
            Telephone phoneObject = GameObject.FindObjectOfType<Telephone>();
            if (phoneObject) {
                Ref<GameObject> phoneRef = new Ref<GameObject>(phoneObject.gameObject);
                telRoutine = new RoutineUseTelephone(g, c, phoneRef, (ConditionBoolSwitch)successCondition);
                routines.Add(telRoutine);
            }
        }
        public override void Update() {
            base.Update();
            if (successCondition.Evaluate() == status.success && !phoneCalled) {
                phoneCalled = true;
            }
        }
    }
    public class GoalTalkToPerson : Goal {
        public Ref<GameObject> target;
        public GoalTalkToPerson(GameObject g, Controllable c, Awareness awareness, Ref<GameObject> target) : base(g, c) {
            this.target = target;
            // this.target = awareness.socializationTarget;
            successCondition = new ConditionBoolSwitch(g);
            RoutineTalkToPerson talkRoutine = new RoutineTalkToPerson(g, c, target, (ConditionBoolSwitch)successCondition, awareness);
            routines.Add(talkRoutine);
        }
    }
    public class GoalSayLine : Goal {
        public ConditionBoolSwitch boolSwitch;
        public GoalSayLine(GameObject g, Controllable c, MessageSpeech message) : base(g, c) {
            boolSwitch = new ConditionBoolSwitch(g);
            RoutineSayLine routine = new RoutineSayLine(g, c, message, boolSwitch);
            successCondition = boolSwitch;
            routines.Add(routine);
        }
    }
    public class GoalDeliverPizza : Goal {
        public Ref<GameObject> target;
        public ConditionBoolSwitch boolSwitch;
        public GoalDeliverPizza(GameObject g, Controllable c, Ref<GameObject> target) : base(g, c) {
            this.target = target;
            successCondition = new ConditionBoolSwitch(g);
            boolSwitch = new ConditionBoolSwitch(g);
            successCondition = boolSwitch;
            RoutineSpeechWithPerson talkRoutine = new RoutineSpeechWithPerson(g, c, target, (ConditionBoolSwitch)successCondition);
            routines.Add(talkRoutine);
        }
    }
    public class GoalUseItem : Goal {
        public Inventory inventory;
        public ConditionBoolSwitch boolSwitch;
        public GoalUseItem(GameObject g, Controllable c) : base(g, c) {
            inventory = g.GetComponent<Inventory>();
            boolSwitch = new ConditionBoolSwitch(g);
            successCondition = boolSwitch;
            RoutinePressF routinePressF = new RoutinePressF(g, c, boolSwitch, count: 2, interval: 5f);
            routines.Add(routinePressF);
        }
    }
    public class GoalGetItem : Goal {
        public bool findingFail;
        public GoalGetItem(GameObject g, Controllable c, Ref<GameObject> target) : base(g, c) {
            // TODO: fill this in
            successCondition = new ConditionHoldingSpecificObject(g, target);
            routines.Add(new RoutineRetrieveRefFromInv(g, c, target));
            routines.Add(new RoutineGetRefFromEnvironment(g, c, target));
        }
        public GoalGetItem(GameObject g, Controllable c, string target) : base(g, c) {
            goalThought = "I need a " + target + ".";
            successCondition = new ConditionHoldingObjectWithName(g, target);
            routines.Add(new RoutineRetrieveNamedFromInv(g, c, target));
            routines.Add(new RoutineGetNamedFromEnvironment(g, c, target));
            routines.Add(new RoutineWanderUntilNamedFound(g, c, target));
        }
        public GoalGetItem(GameObject g, Controllable c, GameObject target) : base(g, c) {
            goalThought = "I need a " + target + ".";
            successCondition = new ConditionHoldingObjectWithName(g, target.name);
            routines.Add(new RoutineRetrieveNamedFromInv(g, c, target.name));
            routines.Add(new RoutineGetNamedFromEnvironment(g, c, target.name));
            routines.Add(new RoutineWanderUntilNamedFound(g, c, target.name));
        }
        public override void Update() {
            base.Update();
            if (index == 2 && !findingFail) {
                findingFail = true;
            }
        }
    }
    public class GoalWalkToObject : Goal {
        public Ref<GameObject> target;
        public new string goalThought {
            get { return "I'm going to check out that " + target.val.name + "."; }
        }
        public GoalWalkToObject(GameObject g, Controllable c, Ref<GameObject> t, float range = 0.2f, bool invert = false, Vector2 localOffset = new Vector2()) : base(g, c) {
            target = t;
            // TODO: if invert, change success condition
            successCondition = new ConditionCloseToObject(g, target, range, localOffset: localOffset);
            routines.Add(new RoutineWalkToGameobject(g, c, target, invert: invert, localOffset: localOffset));
        }
        public GoalWalkToObject(GameObject g, Controllable c, Type objType, float range = 0.2f) : base(g, c) {
            // GameObject targetObject = GameObject.FindObjectOfType<typeof(objType)>();
            UnityEngine.Object obj = GameObject.FindObjectOfType(objType);
            Component targetComponent = (Component)obj;
            successCondition = new ConditionFail(g);
            if (targetComponent != null) {
                GameObject targetObject = targetComponent.gameObject;
                target = new Ref<GameObject>(targetObject);
                routines.Add(new RoutineWalkToGameobject(g, c, target));
                successCondition = new ConditionCloseToObject(g, target, range);
            }
        }
    }

    public class GoalWalkToPoint : Goal {
        public Ref<Vector2> target;
        public GoalWalkToPoint(GameObject g, Controllable c, Ref<Vector2> target, float minDistance = 0.2f, bool invert = false, bool jitter = false) : base(g, c) {
            goalThought = "I want to be over there.";
            this.target = target;
            ConditionLocation condition = new ConditionLocation(g, target);
            condition.minDistance = minDistance;
            successCondition = condition;

            RoutineWalkToPoint routineWalk = new RoutineWalkToPoint(g, c, target, minDistance, invert: invert);
            routines.Add(routineWalk);
            if (jitter) {
                RoutineWander wanderRoutine = new RoutineWander(g, c);
                routineWalk.timeLimit = 0.5f;
                wanderRoutine.timeLimit = 0.5f;

                routines.Add(wanderRoutine);
            }
        }
        public GoalWalkToPoint(GameObject g, Controllable c, Ref<Vector2> target) : this(g, c, target, minDistance: 0.2f, invert: false) { }
    }
    public class GoalLookAtObject : Goal {
        public Ref<GameObject> target;
        public GoalLookAtObject(GameObject g, Controllable c, Ref<GameObject> target) : base(g, c) {
            this.target = target;
            successCondition = new ConditionLookingAtObject(g, c, target);
            routines.Add(new RoutineLookAtObject(g, c, target));
        }
    }
    public class GoalObserveObject : Goal {
        public Ref<GameObject> target;
        public GoalObserveObject(GameObject g, Controllable c, Ref<GameObject> target) : base(g, c) {
            goalThought = "I'm looking for something.";
            this.target = target;
            routines.Add(new Routine(g, c));
            successCondition = new ConditionLookingAtObject(g, c, target);
        }
    }
    public class GoalTriggerTrapdoor : Goal {
        ConditionBoolSwitch successSwitch;
        public GoalTriggerTrapdoor(GameObject g, Controllable c) : base(g, c) {
            goalThought = "I'm sprining my trap.";
            Speech speech = g.GetComponent<Speech>();

            successSwitch = new ConditionBoolSwitch(g);
            routines.Add(new RoutineTrapdoor(g, c, speech, successSwitch));

            // successSwitch.conditionMet = true;
            successCondition = successSwitch;
        }
    }
    public class GoalLookInDirection : Goal {
        public Vector2 dir;
        public GoalLookInDirection(GameObject g, Controllable c, Vector2 dir) : base(g, c) {
            goalThought = "I'm looking over there.";
            this.dir = dir;
            successCondition = new ConditionLookingInDirection(g, c, dir);
            routines.Add(new RoutineLookInDirection(g, c, dir));
        }
        public override void Update() {
            base.Update();
        }
    }
    public class GoalHoseDown : Goal {
        public Ref<GameObject> target;
        public new string goalThought {
            get { return "I've got to do something about that " + target.val.name + "."; }
        }
        public GoalHoseDown(GameObject g, Controllable c, Ref<GameObject> r) : base(g, c) {
            successCondition = new ConditionFail(g);
            RoutineUseObjectOnTarget hoseRoutine = new RoutineUseObjectOnTarget(g, c, r);
            RoutineWander wanderRoutine = new RoutineWander(g, c);
            hoseRoutine.timeLimit = 1;
            wanderRoutine.timeLimit = 1;
            routines.Add(hoseRoutine);
            routines.Add(wanderRoutine);
        }
    }
    public class GoalPunch : Goal {
        public Ref<GameObject> target;
        public new string goalThought {
            get { return "I've got to do something about that " + target.val.name + "."; }
        }
        public GoalPunch(GameObject g, Controllable c, Ref<GameObject> r, Personality.CombatProfficiency profficiency) : base(g, c) {
            Goal punchGoal = new Goal(gameObject, control);
            Routine routinePunch = new RoutinePunchAt(gameObject, control, r, profficiency);
            Routine wanderRoutine = new RoutineWander(g, c);
            switch (profficiency) {
                case Personality.CombatProfficiency.expert:
                    routinePunch.timeLimit = 1.2f;
                    wanderRoutine.timeLimit = 0.5f;
                    break;
                default:
                case Personality.CombatProfficiency.normal:
                    routinePunch.timeLimit = 1.0f;
                    wanderRoutine.timeLimit = 1f;
                    break;
                case Personality.CombatProfficiency.poor:
                    routinePunch.timeLimit = 1f;
                    wanderRoutine.timeLimit = 1.2f;
                    break;
            }

            routines.Add(routinePunch);
            routines.Add(wanderRoutine);
        }
    }
    public class GoalWander : Goal {
        public GoalWander(GameObject g, Controllable c) : base(g, c) {
            goalThought = "I'm doing nothing in particular.";
            successCondition = new ConditionFail(g);
            routines.Add(new RoutineWander(g, c));
        }
    }
    public class GoalPanic : Goal {
        public GoalPanic(GameObject g, Controllable c) : base(g, c) {
            goalThought = "Panic!";
            successCondition = new ConditionFail(g);
            routines.Add(new RoutinePanic(g, c));
        }
    }
    public class GoalInflateBalloons : Goal {
        float utteranceTimer = UnityEngine.Random.Range(10f, 20f);
        List<string> phrases;
        public GoalInflateBalloons(GameObject g, Controllable c) : base(g, c) {
            goalThought = "I'm inflating balloons.";
            successCondition = new ConditionFail(g);
            routines.Add(new RoutineWanderAndPressF(g, c));
            LoadPhrases();
        }
        public void LoadPhrases() {
            phrases = new List<string>();
            TextAsset textData = Resources.Load("data/dialogue/clown_phrases") as TextAsset;
            foreach (string line in textData.text.Split('\n')) {
                phrases.Add(line);
            }
        }
        public override void Update() {
            base.Update();
            utteranceTimer -= Time.deltaTime;
            if (utteranceTimer <= 0f) {
                EventData ed = new EventData(positive: 1);
                utteranceTimer = UnityEngine.Random.Range(10f, 20f);
                string phrase = phrases[UnityEngine.Random.Range(0, phrases.Count)];
                MessageSpeech message = new MessageSpeech(phrase, eventData: ed);
                Toolbox.Instance.SendMessage(gameObject, gameObject.transform, message);
            }
        }
    }

    public class GoalDukesUp : Goal {
        public GoalDukesUp(GameObject g, Controllable c, Inventory i) : base(g, c) {
            successCondition = new ConditionInFightMode(g, control);
            // TODO: add a routine for checking inventory
            routines.Add(new RoutineToggleFightMode(g, c));
            routines.Add(new RoutineToggleFightMode(g, c));
        }
    }
}
