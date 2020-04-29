using UnityEngine;

namespace AI {
    public class ConditionSawObjectRecently : Condition {
        public Ref<GameObject> target;
        private Awareness awareness;
        public ConditionSawObjectRecently(GameObject g, Ref<GameObject> target) : base(g) {
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
}