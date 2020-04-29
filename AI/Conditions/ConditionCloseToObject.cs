using UnityEngine;

namespace AI {
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
}