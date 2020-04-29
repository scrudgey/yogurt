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
}