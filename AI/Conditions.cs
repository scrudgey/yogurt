using UnityEngine;

namespace AI{
	public class Condition{
		public string conditionThought = "I have no clear motivation!";
		protected GameObject gameObject;
		public Condition(GameObject g){
			Init(g);
		}
		private void Init(GameObject g){
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
		public ConditionLocation(GameObject g, Vector2 t): base(g) {
			conditionThought = "I need to be over there.";
			target = t;
		}
		public override status Evaluate(){
			if (Vector2.Distance(gameObject.transform.position, target) < 0.25f){
				return status.success;
			} else {
				return status.neutral;
			}
		}
	}

	public class ConditionCloseToObject : Condition{
		GameObject target;
		float dist;
		public ConditionCloseToObject(GameObject g, GameObject t, float d) : base(g) {
			// conditionThought = "I need to get close to that "+t.name;
			target = t;
			dist = d;
		}
		public ConditionCloseToObject(GameObject g, GameObject t) : base(g) {
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
		public ConditionHoldingObjectOfType(GameObject g, string t) : base(g) {
			conditionThought = "I need a "+t;
			type = t;
			inv = gameObject.GetComponent<Inventory>();
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
		public ConditionHoldingObjectWithName(GameObject g, string t) : base(g) {
			conditionThought = "I need a "+t;
			name = t;
			inv = gameObject.GetComponent<Inventory>();
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

	public class ConditionInFightMode : Condition{
		Inventory inv;
		public ConditionInFightMode(GameObject g, Inventory i) : base(g) {
			inv = i;
		}
		public override status Evaluate(){
			if (inv){
				if (inv.fightMode){
					return status.success;
				} else {
					return status.neutral;
				}
			} else {
				return status.failure;
			}
		}
	}

}