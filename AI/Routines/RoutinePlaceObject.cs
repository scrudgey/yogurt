using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutinePlaceObject : Routine {
        private Inventory inv;
        private Ref<Vector2> position;
        public RoutinePlaceObject(GameObject g, Controller c, Ref<Vector2> position) : base(g, c) {
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
}