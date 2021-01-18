using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
    public class GoalGetSmoothieIngredient : Goal {
        Condition ectoplasmCondition;
        Condition pizzaCondition;
        Condition liverCondition;
        Condition failCondition;
        public Ref<int> smoothieIngredient;
        public Inventory inventory;
        public GoalGetSmoothieIngredient(GameObject g, Controller c, Ref<int> smoothieIngredient) : base(g, c) {
            ectoplasmCondition = new ConditionHoldingObjectWithName(g, "ectoplasm");
            pizzaCondition = new ConditionHoldingObjectWithName(g, "pizza");
            liverCondition = new ConditionHoldingObjectWithName(g, "liver");
            failCondition = new ConditionFail(g);
            this.smoothieIngredient = smoothieIngredient;
            this.successCondition = failCondition;
            this.inventory = g.GetComponent<Inventory>();
        }
        public override void Update() {
            base.Update();
            GameObject ingredient = null;
            string prefabPath = "";
            switch (smoothieIngredient.val) {
                default:
                case -1:
                    successCondition = failCondition;
                    break;
                case 1:
                    successCondition = ectoplasmCondition;
                    prefabPath = "prefabs/ectoplasm";
                    break;
                case 2:
                    successCondition = pizzaCondition;
                    prefabPath = "prefabs/pizza";
                    break;
                case 3:
                    successCondition = liverCondition;
                    prefabPath = "prefabs/liver";
                    break;
            }
            if (inventory.holding == null && smoothieIngredient.val != -1) {
                ingredient = GameObject.Instantiate(Resources.Load(prefabPath), gameObject.transform.position, Quaternion.identity) as GameObject;
                Pickup pickup = ingredient.GetComponent<Pickup>();
                inventory.GetItem(pickup);
            }
        }
    }
}
