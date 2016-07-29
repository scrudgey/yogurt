using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
// using System;

namespace AI{

	public class Condition{
		public string conditionThought = "I have no clear motivation!";

		protected GameObject gameObject;
		public virtual void Init(GameObject g){
			gameObject = g;
		}
		public virtual status Evaluate(){
			return status.neutral;
		}
	}

	/* 
	 * Success conditions
	 * 
	 * */

	public class ConditionLocation : Condition{
		Vector2 target;
		
		public ConditionLocation(Vector2 t){
			conditionThought = "I need to be over there.";
			target = t;
		}
		
		public override status Evaluate(){
			if ( Vector2.Distance(gameObject.transform.position,target) < 0.25f){
				return status.success;
			} else {
				return status.neutral;
			}
		}
	}

	public class ConditionCloseToObject : Condition{
		GameObject target;
		float dist;

		public ConditionCloseToObject(GameObject t, float d){
			conditionThought = "I need to get close to that "+t.name;
			target = t;
			dist = d;
		}

		public ConditionCloseToObject(GameObject t){
			target = t;
			dist = 0.25f;
		}
		
		public override status Evaluate(){
			if (Vector2.Distance(gameObject.transform.position, target.transform.position) < dist ){
				return status.success;
			} else {
				return status.neutral;
			}
		}
	}

	// this success condition is going to be pretty update intensive, getting a component on each frame??
	// find a better way to do this
	public class ConditionHoldingObjectOfType : Condition{
		string type;
		Inventory inv;
		
		public override void Init (GameObject g)
		{
			base.Init (g);
			inv = gameObject.GetComponent<Inventory>();
		}
		
		public ConditionHoldingObjectOfType(string t){
			conditionThought = "I need a "+t;
			type = t;
		}
		
		public override status Evaluate(){
			if (inv){
				if (inv.holding){
					if (inv.holding.GetComponent(type) ){
						return status.success;
					}else{
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

	public class ConditionHoldingObjectWithName : Condition{
		string name;
		Inventory inv;
		
		public override void Init (GameObject g)
		{
			base.Init (g);
			
			inv = gameObject.GetComponent<Inventory>();
		}
		
		public ConditionHoldingObjectWithName(string t){
			conditionThought = "I need a "+t;
			name = t;
		}
		
		public override status Evaluate(){
			if (inv ){
				if (inv.holding){
					if (inv.holding.name == name ){
						return status.success;
					}else{
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

}