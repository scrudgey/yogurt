using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
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
        public RoutineWalkToGameobject(GameObject g, Controller c, Ref<GameObject> targetObject, bool invert = false, Vector2 localOffset = new Vector2()) : base(g, c) {
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

}