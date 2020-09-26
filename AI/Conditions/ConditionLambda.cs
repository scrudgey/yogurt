using UnityEngine;
using System;

namespace AI {
    public class ConditionLambda : Condition {
        protected Func<bool> conditionFunction;
        public ConditionLambda(GameObject g, Func<bool> function) : base(g) {
            this.gameObject = g;
            this.conditionFunction = function;
        }
        public override status Evaluate() {
            switch (conditionFunction()) {
                case true:
                    return status.success;
                default:
                case false:
                    return status.neutral;
            }
        }
    }
}