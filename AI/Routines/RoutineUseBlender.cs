using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
    public class RoutineUseBlender : Routine {
        private Blender blender;
        private Inventory inventory;
        private ConditionBoolSwitch condition;
        private float waitTimer = 0f;
        public RoutineUseBlender(GameObject g, Controller c, Blender blender, Inventory inventory, ConditionBoolSwitch condition) : base(g, c) {
            this.condition = condition;
            this.inventory = inventory;
            this.blender = blender;
        }
        protected override status DoUpdate() {
            // Debug.Log($"{blender} {waitTimer} {condition.conditionMet}");
            if (blender && waitTimer == 0 && !condition.conditionMet) {
                LiquidContainer cauldronContainer = blender.GetComponent<LiquidContainer>();
                cauldronContainer.liquid = null;
                cauldronContainer.amount = 0;

                blender.Store(inventory);
                blender.Power();
                waitTimer += Time.deltaTime;
                return status.neutral;
            } else if (waitTimer > 0 && !condition.conditionMet) {
                waitTimer += Time.deltaTime;
                if (waitTimer > 5f) {
                    // Debug.Log("meeting condition");
                    condition.conditionMet = true;
                    blender.Power();
                    Inventory inv = gameObject.GetComponent<Inventory>();
                    GameObject smoothie = GameObject.Instantiate(Resources.Load("prefabs/slushie"), gameObject.transform.position, Quaternion.identity) as GameObject;
                    LiquidContainer smoothieContainer = smoothie.GetComponent<LiquidContainer>();
                    LiquidContainer cauldronContainer = blender.GetComponent<LiquidContainer>();
                    Pickup smoothiePickup = smoothie.GetComponent<Pickup>();
                    smoothieContainer.FillFromContainer(cauldronContainer);
                    inv.GetItem(smoothiePickup);
                    return status.success;
                }
            } else if (condition.conditionMet) {
                return status.success;
            }
            return status.neutral;
        }
        public void Reset() {
            waitTimer = 0;
            condition.conditionMet = false;
        }
    }
}