using UnityEngine;

namespace AI {
    public class Condition {
        public string conditionThought = "I have no clear motivation!";
        protected GameObject gameObject;
        public Condition(GameObject g) {
            Init(g);
        }
        private void Init(GameObject g) {
            gameObject = g;
        }
        public virtual status Evaluate() {
            return status.neutral;
        }
    }

    /* 
	 * Success conditions
	 * 
	 * */
    public class ConditionLocation : Condition {
        Ref<Vector2> target = new Ref<Vector2>(Vector2.zero);
        public float minDistance = 0.2f;
        public ConditionLocation(GameObject g, Ref<Vector2> t) : base(g) {
            conditionThought = "I need to be over there.";
            target = t;
        }
        public override status Evaluate() {
            if (Vector2.Distance(gameObject.transform.position, target.val) < minDistance) {
                return status.success;
            } else {
                return status.neutral;
            }
        }
    }
    public class ConditionBoolSwitch : Condition {
        public bool conditionMet;
        public ConditionBoolSwitch(GameObject g) : base(g) { }
        public override status Evaluate() {
            if (conditionMet) {
                return status.success;
            } else {
                return status.neutral;
            }
        }
    }
    public class ConditionFail : Condition {
        public ConditionFail(GameObject g) : base(g) { }
        public override status Evaluate() {
            return status.failure;
        }
    }
    public class ConditionCloseToObject : Condition {
        public Ref<GameObject> target;
        float dist;
        public Vector2 localOffset;
        private Transform cachedTransform;
        private GameObject cachedGameObject;
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
        public ConditionCloseToObject(GameObject g, Ref<GameObject> t, float d, Vector2 localOffset = new Vector2()) : base(g) {
            // conditionThought = "I need to get close to that "+t.name;
            target = t;
            dist = d;
            this.localOffset = localOffset;
        }
        public ConditionCloseToObject(GameObject g, Ref<GameObject> t, Vector2 localOffset = new Vector2()) : base(g) {
            target = t;
            dist = 0.25f;
            this.localOffset = localOffset;
        }
        public override status Evaluate() {
            if (target.val == null) {
                return status.failure;
            }
            Vector2 localizedOffset = new Vector2(targetTransform.lossyScale.x * localOffset.x, targetTransform.lossyScale.y * localOffset.y);
            float d = Vector2.Distance(gameObject.transform.position, (Vector2)targetTransform.position + localizedOffset);
            if (d < dist) {
                // Debug.Log("close to object "+target.val.name+" at dist "+dist.ToString());
                return status.success;
            } else {
                return status.failure;
            }
        }
    }
    public class ConditionSawObjectRecently : Condition {
        public Ref<GameObject> target;
        private Awareness awareness;
        public ConditionSawObjectRecently(GameObject g, Controllable c, Ref<GameObject> target) : base(g) {
            this.target = target;
            awareness = g.GetComponent<Awareness>();
        }
        public override status Evaluate() {
            if (target.val == null)
                return status.failure;
            if (awareness) {
                if (awareness.knowledgebase.ContainsKey(target.val)) {
                    if (Time.time - awareness.knowledgebase[target.val].lastSeenTime < 5) {
                        return status.success;
                    } else {
                        return status.failure;
                    }
                } else {
                    return status.failure;
                }
            }
            return status.failure;
        }
    }
    public class ConditionObjectInPlace : Condition {
        public Ref<GameObject> target;
        private Ref<Vector2> place;
        public ConditionObjectInPlace(GameObject g, Controllable c, Ref<GameObject> target, Ref<Vector2> place) : base(g) {
            this.target = target;
            this.place = place;
        }
        public override status Evaluate() {
            if (target.val) {
                if (Vector2.Distance(target.val.transform.position, place.val) < 0.10) {
                    return status.success;
                }
                return status.failure;
            }
            return status.failure;
        }
    }
    public class ConditionLookingAtObject : Condition {
        public Ref<GameObject> target;
        private Controllable controllable;
        public ConditionLookingAtObject(GameObject g, Controllable c, Ref<GameObject> target) : base(g) {
            this.target = target;
            controllable = c;
        }
        public override status Evaluate() {
            float angledif = Vector2.Angle(controllable.direction, (Vector2)target.val.transform.position - (Vector2)gameObject.transform.position);
            if (angledif < 20) {
                return status.success;
            } else {
                return status.neutral;
            }
        }
    }
    public class ConditionLookingInDirection : Condition {
        public Vector2 dir;
        public Controllable controllable;
        public ConditionLookingInDirection(GameObject g, Controllable c, Vector2 dir) : base(g) {
            this.dir = dir;
            controllable = c;
        }
        public override status Evaluate() {
            if (Vector2.Angle(controllable.direction, dir) < 20) {
                return status.success;
            } else {
                return status.neutral;
            }
        }
    }
    // this success condition is going to be pretty update intensive, getting a component on each frame??
    // find a better way to do this
    public class ConditionHoldingObjectOfType : Condition {
        string type;
        Inventory inv;
        public ConditionHoldingObjectOfType(GameObject g, string t) : base(g) {
            conditionThought = "I need a " + t;
            type = t;
            inv = gameObject.GetComponent<Inventory>();
        }
        public override status Evaluate() {
            if (inv) {
                if (inv.holding) {
                    if (inv.holding.GetComponent(type)) {
                        return status.success;
                    } else {
                        return status.neutral;
                    }
                } else {
                    return status.neutral;
                }
            } else {
                return status.failure;
            }
        }
    }
    public class ConditionHoldingObjectWithName : Condition {
        string name;
        Inventory inv;
        public ConditionHoldingObjectWithName(GameObject g, string t) : base(g) {
            conditionThought = "I need a " + t;
            name = t;
            inv = gameObject.GetComponent<Inventory>();
        }
        public override status Evaluate() {
            if (inv) {
                if (inv.holding) {
                    string holdingName = Toolbox.Instance.CloneRemover(inv.holding.name);
                    if (holdingName == name) {
                        return status.success;
                    } else {
                        return status.neutral;
                    }
                } else {
                    return status.neutral;
                }
            } else {
                return status.failure;
            }
        }
    }
    public class ConditionHoldingSpecificObject : Condition {
        Ref<GameObject> target;
        Inventory inv;
        public ConditionHoldingSpecificObject(GameObject g, Ref<GameObject> target) : base(g) {
            // conditionThought = "I need a "+t;
            // name = t;
            this.target = target;
            inv = gameObject.GetComponent<Inventory>();
        }
        public override status Evaluate() {
            if (target.val == null)
                return status.failure;
            if (inv) {
                if (inv.holding) {
                    if (inv.holding.gameObject == target.val) {
                        return status.success;
                    } else {
                        return status.failure;
                    }
                } else {
                    return status.failure;
                }
            } else {
                return status.failure;
            }
        }
    }
    public class ConditionInFightMode : Condition {
        Controllable control;
        Inventory inv;
        public ConditionInFightMode(GameObject g, Controllable c) : base(g) {
            control = c;
            inv = g.GetComponent<Inventory>();
        }
        public override status Evaluate() {
            if (inv != null) {
                if (inv.holding) {
                    MeleeWeapon weapon = inv.holding.GetComponent<MeleeWeapon>();
                    if (weapon) {
                        return status.success;
                    }
                }
            }
            if (control.fightMode) {
                return status.success;
            } else {
                return status.neutral;
            }
        }
    }
    public class ConditionKnowAboutFire : Condition {
        Awareness awareness;
        public ConditionKnowAboutFire(GameObject g) : base(g) {
            awareness = g.GetComponent<Awareness>();
        }
        public override status Evaluate() {
            if (awareness) {
                if (awareness.nearestFire.val != null) {
                    return status.success;
                } else {
                    return status.failure;
                }
            } else {
                return status.failure;
            }
        }
    }
}