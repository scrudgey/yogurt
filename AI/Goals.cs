using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    [System.Serializable]
    public class Ref<T> {
        public T val;
        public Ref(T t) {
            val = t;
        }
    }

    [System.Serializable]
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
        public Goal(GameObject g, Controllable c) {
            gameObject = g;
            control = c;
            slewTime = UnityEngine.Random.Range(0.1f, 0.5f);
        }
        public status Evaluate() {
            // if (successCondition.Evaluate() == status.success)
            //     return status.success;
            foreach (Goal requirement in requirements) {
                if (requirement.Evaluate() != status.success) {
                    return status.failure;
                }
            }
            return successCondition.Evaluate();
        }
        public virtual void Update() {
            // if i have any unmet requirements, my update goes to the first unmet one.
            foreach (Goal requirement in requirements) {
                if (requirement.Evaluate() != status.success) {
                    fulfillingRequirements = true;
                    requirement.Update();
                    return;
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
                    Debug.Log(this.ToString() + " fail: " + e.Message);
                    Debug.Log(e.TargetSite);
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
        public GoalWalkToObject(GameObject g, Controllable c, Ref<GameObject> t, float range = 0.2f) : base(g, c) {
            target = t;
            successCondition = new ConditionCloseToObject(g, target, range);
            routines.Add(new RoutineWalkToGameobject(g, c, target));
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
    public class GoalWalkToPoint : Goal {
        public Ref<Vector2> target;
        public GoalWalkToPoint(GameObject g, Controllable c, Ref<Vector2> target, float minDistance) : base(g, c) {
            goalThought = "I want to be over there.";
            this.target = target;
            ConditionLocation condition = new ConditionLocation(g, target);
            condition.minDistance = minDistance;
            successCondition = condition;

            routines.Add(new RoutineWalkToPoint(g, c, target, minDistance));
        }
        public GoalWalkToPoint(GameObject g, Controllable c, Ref<Vector2> target) : this(g, c, target, 0.2f) { }
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
    public class GoalRunFromObject : Goal {
        public GoalRunFromObject(GameObject g, Controllable c, Ref<GameObject> threat) : base(g, c) {
            goalThought = "I'm trying to avoid a bad thing.";
            // successCondition = new ConditionLocation(g, new Ref<Vector2>(Vector2.zero));
            successCondition = new ConditionLocation(g, new Ref<Vector2>(new Vector2(-999f, -999f)));
            routines.Add(new RoutineAvoidGameObject(g, c, threat));
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
