﻿using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public enum status { neutral, success, failure }
    [System.Serializable]
    public class Routine {
        public string routineThought = "I have no idea what I'm doing!";
        protected GameObject gameObject;
        protected Controllable control;
        public float timeLimit = -1;
        protected float runTime = 0;
        Transform cachedTransform;
        public Transform transform {
            get {
                if (cachedTransform == null) {
                    cachedTransform = gameObject.GetComponent<Transform>();
                }
                return cachedTransform;
            }
        }
        public Routine(GameObject g, Controllable c) {
            gameObject = g;
            control = c;
        }
        protected virtual status DoUpdate() {
            return status.neutral;
        }
        public virtual void Configure() {

        }
        // update is the routine called each frame by goal.
        // this bit first checks for a timeout, then calls a routine
        // that is specific to the child class.
        public status Update() {
            // Debug.Log("")
            runTime += Time.deltaTime;
            if (timeLimit > 0 && runTime > timeLimit) {
                runTime = 0;
                return status.failure;
            } else
                return DoUpdate();
        }
    }

    /* 
	 * Routines
	 * 
	 * */
    public class RoutineWanderUntilNamedFound : Routine {
        // TODO: routine's update mimics goal's requirements structure. replace this with a goal
        private string target;
        private Awareness awareness;
        private float checkInterval;
        private RoutineWander wander;
        private RoutineGetNamedFromEnvironment getIt;
        private int mode;
        public RoutineWanderUntilNamedFound(GameObject g, Controllable c, string t) : base(g, c) {
            target = t;
            routineThought = "I'm looking around for a " + t + ".";
            mode = 0;
            awareness = gameObject.GetComponent<Awareness>();
            wander = new RoutineWander(gameObject, control);
        }
        protected override status DoUpdate() {
            if (mode == 0) {   // wander part
                if (checkInterval > 1.5f) {
                    checkInterval = 0f;
                    List<GameObject> objs = awareness.FindObjectWithName(target);
                    if (objs.Count > 0) {
                        mode = 1;
                        getIt = new RoutineGetNamedFromEnvironment(gameObject, control, target);
                    }
                }
                checkInterval += Time.deltaTime;
                return wander.Update();
            } else {
                return getIt.Update();
            }
        }
    }

    public class RoutineUseTelephone : Routine {
        public Ref<GameObject> telRef;
        private Telephone telephone;
        private ConditionBoolSwitch condition;
        public RoutineUseTelephone(GameObject g, Controllable c, Ref<GameObject> telRef, ConditionBoolSwitch condition) : base(g, c) {
            this.telRef = telRef;
            this.condition = condition;
            telephone = telRef.val.GetComponent<Telephone>();
        }
        protected override status DoUpdate() {
            if (telephone && !condition.conditionMet) {
                telephone.FireButtonCallback();
                condition.conditionMet = true;
                return status.success;
            } else {
                return status.failure;
            }
        }
    }
    public class RoutineTalkToPerson : Routine {
        public Ref<GameObject> target;
        private ConditionBoolSwitch condition;
        public Awareness awareness;
        public RoutineTalkToPerson(GameObject g, Controllable c, Ref<GameObject> target, ConditionBoolSwitch condition, Awareness awareness) : base(g, c) {
            this.target = target;
            this.condition = condition;
            this.awareness = awareness;
        }
        protected override status DoUpdate() {
            if (target.val == null)
                return status.neutral;
            if (awareness && !condition.conditionMet) {
                awareness.ReactToPerson(target.val);
                condition.conditionMet = true;
                awareness.socializationTimer = -30;
                return status.success;
            } else {
                return status.failure;
            }
        }
    }
    public class RoutineSpeechWithPerson : Routine {
        public Ref<GameObject> target;
        private ConditionBoolSwitch condition;
        private Speech speech;
        public RoutineSpeechWithPerson(GameObject g, Controllable c, Ref<GameObject> target, ConditionBoolSwitch condition) : base(g, c) {
            this.target = target;
            this.condition = condition;
            this.speech = g.GetComponent<Speech>();
        }
        protected override status DoUpdate() {
            if (target.val == null)
                return status.neutral;
            if (!condition.conditionMet) {
                condition.conditionMet = true;
                speech.SpeakWith();
                Inventory myInv = gameObject.GetComponent<Inventory>();
                Inventory theirInv = target.val.GetComponent<Inventory>();
                if (myInv.holding != null) {
                    theirInv.GetItem(myInv.holding);
                }
                return status.success;
            } else {
                return status.failure;
            }
        }
    }
    public class RoutineSayLine : Routine {
        public MessageSpeech message;
        private ConditionBoolSwitch condition;
        public RoutineSayLine(GameObject g, Controllable c, MessageSpeech message, ConditionBoolSwitch condition) : base(g, c) {
            this.message = message;
            this.condition = condition;
        }
        protected override status DoUpdate() {
            if (message == null)
                return status.neutral;
            if (!condition.conditionMet) {
                Toolbox.Instance.SendMessage(gameObject, control, message);
                condition.conditionMet = true;
                return status.success;
            } else {
                return status.failure;
            }
        }
    }
    public class RoutineGetNamedFromEnvironment : Routine {
        private Inventory inv;
        private Awareness awareness;
        private GameObject target;
        private string targetName;
        private RoutineWalkToGameobject walkToRoutine;
        public RoutineGetNamedFromEnvironment(GameObject g, Controllable c, string t) : base(g, c) {
            routineThought = "I'm going to pick up that " + t + ".";
            targetName = t;
            inv = gameObject.GetComponent<Inventory>();
            awareness = gameObject.GetComponent<Awareness>();
            Configure();
        }
        public override void Configure() {
            List<GameObject> objs = new List<GameObject>();
            if (awareness) {
                objs = awareness.FindObjectWithName(targetName);
            }
            if (objs.Count > 0) {
                target = objs[0];
            }
            if (target && target.activeInHierarchy) {
                walkToRoutine = new RoutineWalkToGameobject(gameObject, control, new Ref<GameObject>(target));
            }
        }
        protected override status DoUpdate() {
            if (target && target.activeInHierarchy) {
                if (Vector2.Distance(transform.position, target.transform.position) > 0.2f) {
                    return walkToRoutine.Update();
                } else {
                    // this bit is shakey
                    inv.GetItem(target.GetComponent<Pickup>());
                    return status.success;
                }
            } else {
                return status.failure;
            }
        }
    }
    public class RoutineGetRefFromEnvironment : Routine {
        private Inventory inv;
        private Ref<GameObject> target = new Ref<GameObject>(null);
        private RoutineWalkToGameobject walkToRoutine;
        public RoutineGetRefFromEnvironment(GameObject g, Controllable c, Ref<GameObject> target) : base(g, c) {
            // routineThought = "I'm going to pick up that " + t + ".";
            this.target = target;
            inv = gameObject.GetComponent<Inventory>();
            Configure();
        }
        public override void Configure() {
            walkToRoutine = new RoutineWalkToGameobject(gameObject, control, target);
        }
        protected override status DoUpdate() {
            if (target.val && target.val.activeInHierarchy) {
                if (Vector2.Distance(transform.position, target.val.transform.position) > 0.2f) {
                    return walkToRoutine.Update();
                } else {
                    // this bit is shakey
                    inv.GetItem(target.val.GetComponent<Pickup>());
                    return status.success;
                }
            } else {
                return status.failure;
            }
        }
    }

    public class RoutineRetrieveNamedFromInv : Routine {
        private Inventory inv;
        private string targetName;
        public RoutineRetrieveNamedFromInv(GameObject g, Controllable c, string names) : base(g, c) {
            routineThought = "I'm checking my pockets for a " + names + ".";
            targetName = names;
            inv = g.GetComponent<Inventory>();
        }
        protected override status DoUpdate() {
            if (inv) {
                foreach (GameObject g in inv.items) {
                    if (targetName == g.name) {
                        inv.RetrieveItem(g.name);
                        return status.success;
                    }
                }
                return status.failure;
            } else {
                return status.failure;
            }
        }
    }
    public class RoutinePlaceObject : Routine {
        private Inventory inv;
        private Ref<Vector2> position;
        public RoutinePlaceObject(GameObject g, Controllable c, Ref<Vector2> position) : base(g, c) {
            this.position = position;
            inv = g.GetComponent<Inventory>();
        }
        protected override status DoUpdate() {
            if (inv) {
                if (inv.holding) {
                    if (inv.PlaceItem(position.val)) {
                        return status.success;
                    } else {
                        return status.failure;
                    }
                }
                return status.failure;
            } else {
                return status.failure;
            }
        }
    }
    public class RoutineRetrieveRefFromInv : Routine {
        private Inventory inv;
        private Ref<GameObject> target;
        public RoutineRetrieveRefFromInv(GameObject g, Controllable c, Ref<GameObject> target) : base(g, c) {
            // routineThought = "I'm checking my pockets for a " + names + ".";
            this.target = target;
            inv = g.GetComponent<Inventory>();
        }
        protected override status DoUpdate() {
            if (inv) {
                foreach (GameObject g in inv.items) {
                    if (target.val == g) {
                        inv.RetrieveItem(g.name);
                        return status.success;
                    }
                }
                return status.failure;
            } else {
                return status.failure;
            }
        }
    }
    public class RoutinePanic : Routine {
        private float wanderTime = 0;
        public enum direction { left, right, up, down, none }
        private direction dir;
        public RoutinePanic(GameObject g, Controllable c) : base(g, c) {
            routineThought = "Panic!!!!";
        }
        public override void Configure() {
            wanderTime = UnityEngine.Random.Range(0, 2);
            dir = (direction)(UnityEngine.Random.Range(0, 4));
        }
        protected override status DoUpdate() {
            control.ResetInput();
            if (wanderTime > 0) {
                switch (dir) {
                    case direction.down:
                        control.downFlag = true;
                        break;
                    case direction.left:
                        control.leftFlag = true;
                        break;
                    case direction.right:
                        control.rightFlag = true;
                        break;
                    case direction.up:
                        control.upFlag = true;
                        break;
                }
            }
            if (wanderTime < 0f) {
                wanderTime = UnityEngine.Random.Range(0, 2);
                dir = (direction)(UnityEngine.Random.Range(0, 4));
            } else {
                wanderTime -= Time.deltaTime;
            }
            return status.neutral;
        }
    }
    public class RoutineWander : Routine {
        private float wanderTime = 0;
        public enum direction { left, right, up, down, none }
        private direction dir;
        public RoutineWander(GameObject g, Controllable c) : base(g, c) {
            routineThought = "I'm wandering around.";
        }
        public override void Configure() {
            wanderTime = UnityEngine.Random.Range(0, 2);
            dir = (direction)(UnityEngine.Random.Range(0, 4));
        }
        protected override status DoUpdate() {
            control.ResetInput();
            if (wanderTime > 0) {
                switch (dir) {
                    case direction.down:
                        control.downFlag = true;
                        break;
                    case direction.left:
                        control.leftFlag = true;
                        break;
                    case direction.right:
                        control.rightFlag = true;
                        break;
                    case direction.up:
                        control.upFlag = true;
                        break;
                }
            }
            if (wanderTime < -1f) {
                wanderTime = UnityEngine.Random.Range(0, 2);
                dir = (direction)(UnityEngine.Random.Range(0, 4));
            } else {
                wanderTime -= Time.deltaTime;
            }
            return status.neutral;
        }
    }
    public class RoutinePressF : Routine {
        public ConditionBoolSwitch boolSwitch;
        public int count;
        public float interval;
        public float timer;
        public int numberTimesPressed;
        public RoutinePressF(GameObject g, Controllable c, ConditionBoolSwitch boolSwitch, int count = 1, float interval = 1f) : base(g, c) {
            this.boolSwitch = boolSwitch;
            this.count = count;
            this.interval = interval;
            this.timer = interval;
        }
        protected override status DoUpdate() {
            timer += Time.deltaTime;
            if (timer > interval) {
                timer = 0;
                if (numberTimesPressed < count) {
                    numberTimesPressed += 1;
                    control.ShootPressed();
                    return status.neutral;
                } else {
                    boolSwitch.conditionMet = true;
                    return status.success;
                }
            }
            return status.neutral;
        }
    }
    public class RoutineWanderAndPressF : RoutineWander {
        public float timeSinceF;
        public RoutineWanderAndPressF(GameObject g, Controllable c) : base(g, c) {
            routineThought = "I'm a clown.";
            timeSinceF = UnityEngine.Random.Range(1.0f, 1.5f);
        }
        protected override status DoUpdate() {
            timeSinceF -= Time.deltaTime;
            if (timeSinceF <= 0) {
                timeSinceF = UnityEngine.Random.Range(1.5f, 10.5f);
                control.ShootPressed();
            }
            return base.DoUpdate();
        }
    }
    public class RoutineLookAtObject : Routine {
        public Ref<GameObject> target = new Ref<GameObject>(null);
        public RoutineLookAtObject(GameObject g, Controllable c, Ref<GameObject> target) : base(g, c) {
            this.target = target;
        }
        protected override status DoUpdate() {
            control.ResetInput();
            control.LookAtPoint(target.val.transform.position);
            return status.neutral;
        }
    }
    public class RoutineLookInDirection : Routine {
        public Vector2 dir;
        public RoutineLookInDirection(GameObject g, Controllable c, Vector2 dir) : base(g, c) {
            this.dir = dir;
        }
        protected override status DoUpdate() {
            control.ResetInput();
            control.SetDirection(dir);
            return status.neutral;
        }
    }
    public class RoutineWalkToPoint : Routine {
        public bool invert;
        public float minDistance;
        public Ref<Vector2> target = new Ref<Vector2>(Vector2.zero);
        public RoutineWalkToPoint(GameObject g, Controllable c, Ref<Vector2> t, float minDistance, bool invert = false) : base(g, c) {
            routineThought = "I'm walking to a spot.";
            target = t;
            this.minDistance = minDistance;
            this.invert = invert;
        }
        public RoutineWalkToPoint(GameObject g, Controllable c, Ref<Vector2> t) : this(g, c, t, 0.1f) { }
        protected override status DoUpdate() {
            float distToTarget = Vector2.Distance(gameObject.transform.position, target.val);
            control.ResetInput();
            if (distToTarget < minDistance) {
                return status.success;
            } else {
                Vector2 comparator = Vector2.zero;

                if (invert) {
                    comparator = target.val - (Vector2)transform.position;
                } else {
                    comparator = (Vector2)transform.position - target.val;
                }

                if (comparator.x > 0) {
                    control.leftFlag = true;
                } else if (comparator.x < 0) {
                    control.rightFlag = true;
                }

                if (comparator.y > 0) {
                    control.downFlag = true;
                } else if (comparator.y < 0) {
                    control.upFlag = true;
                }
                return status.neutral;
            }
        }
    }

    public class RoutineUseObjectOnTarget : Routine {
        public Ref<GameObject> target;
        public RoutineUseObjectOnTarget(GameObject g, Controllable c, Ref<GameObject> targetObject) : base(g, c) {
            routineThought = "I'm using this object on " + g.name + ".";
            target = targetObject;
        }
        protected override status DoUpdate() {
            if (target.val != null) {
                control.SetDirection(Vector2.ClampMagnitude(target.val.transform.position - gameObject.transform.position, 1f));
                control.ShootHeld();
                return status.neutral;
            } else {
                return status.failure;
            }
        }
    }

    public class RoutineWalkToGameobject : Routine {
        public bool invert;
        public Ref<GameObject> target;
        private Transform cachedTransform;
        private GameObject cachedGameObject;
        public float minDistance = 0.2f;
        public Vector2 localOffset;
        public Transform targetTransform {
            get {
                if (cachedGameObject == target.val) {
                    if (cachedTransform != null) {
                        return cachedTransform;
                    } else {
                        cachedTransform = target.val.transform;
                        return cachedTransform;
                    }
                } else {
                    cachedGameObject = target.val;
                    cachedTransform = target.val.transform;
                    return cachedTransform;
                }
            }
        }
        public RoutineWalkToGameobject(GameObject g, Controllable c, Ref<GameObject> targetObject, bool invert = false, Vector2 localOffset = new Vector2()) : base(g, c) {
            routineThought = "I'm walking over to the " + g.name + ".";
            this.target = targetObject;
            this.invert = invert;
            this.localOffset = localOffset;
        }
        protected override status DoUpdate() {
            if (target.val != null) {
                Vector2 localizedOffset = new Vector2(targetTransform.lossyScale.x * localOffset.x, targetTransform.lossyScale.y * localOffset.y);

                Vector2 targetPosition = (Vector2)targetTransform.position + localizedOffset;
                float distToTarget = Vector2.Distance(transform.position, targetPosition);
                control.leftFlag = control.rightFlag = control.upFlag = control.downFlag = false;
                if (distToTarget <= minDistance) {
                    return status.success;
                } else {
                    Vector2 comparator = Vector2.zero;

                    if (invert) {
                        comparator = targetPosition - (Vector2)transform.position;
                    } else {
                        comparator = (Vector2)transform.position - targetPosition;
                    }
                    if (comparator.x > 0) {
                        control.leftFlag = true;
                    } else if (comparator.x < 0) {
                        control.rightFlag = true;
                    }

                    if (comparator.y > 0) {
                        control.downFlag = true;
                    } else if (comparator.y < 0) {
                        control.upFlag = true;
                    }

                    return status.neutral;
                }
            } else {
                // Debug.Log("target val is null");
                return status.failure;
            }
        }
    }
    public class RoutineToggleFightMode : Routine {
        public RoutineToggleFightMode(GameObject g, Controllable c) : base(g, c) {
            routineThought = "I need to prepare for battle!";
        }
        protected override status DoUpdate() {
            control.ToggleFightMode();
            return status.success;
        }
    }
    public class RoutineTrapdoor : Routine {
        public Speech vampireSpeech;
        public ConditionBoolSwitch sw;
        public RoutineTrapdoor(GameObject g, Controllable c, Speech vampireSpeech, ConditionBoolSwitch sw) : base(g, c) {
            this.vampireSpeech = vampireSpeech;
            this.sw = sw;
        }
        protected override status DoUpdate() {
            if (!sw.conditionMet) {
                vampireSpeech.SpeakWith();
                sw.conditionMet = true;
                return status.success;
            } else
                return status.neutral;
        }
    }
    public class RoutinePunchAt : Routine {
        Ref<GameObject> target;
        float timer;
        Personality.CombatProfficiency proficiency;
        public RoutinePunchAt(GameObject g, Controllable c, Ref<GameObject> t, Personality.CombatProfficiency proficiency) : base(g, c) {
            routineThought = "Fisticuffs!";
            target = t;
            this.proficiency = proficiency;
        }
        protected override status DoUpdate() {
            timer -= Time.deltaTime;
            if (target.val != null) {
                control.SetDirection(Vector2.ClampMagnitude(target.val.transform.position - gameObject.transform.position, 1f));
                if (timer < 0) {
                    if (proficiency == Personality.CombatProfficiency.normal) {
                        timer = 0.65f + UnityEngine.Random.Range(0, 0.25f);
                    } else if (proficiency == Personality.CombatProfficiency.expert) {
                        timer = 0.5f + UnityEngine.Random.Range(0, 0.2f);
                    } else if (proficiency == Personality.CombatProfficiency.poor) {
                        timer = 0.8f + UnityEngine.Random.Range(0, 0.25f);
                    }
                    control.ShootPressed();
                } else {
                    control.ResetInput();
                }
                return status.neutral;
            } else {
                return status.failure;
            }
        }
    }
}