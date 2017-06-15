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
		Ref<Vector2> target = new Ref<Vector2>(Vector2.zero);
		public ConditionLocation(GameObject g, Ref<Vector2> t): base(g) {
			conditionThought = "I need to be over there.";
			target = t;
		}
		public override status Evaluate(){
			if (Vector2.Distance(gameObject.transform.position, target.val) < 0.25f){
				return status.success;
			} else {
				return status.neutral;
			}
		}
	}
	public class ConditionBoolSwitch : Condition{
		public bool conditionMet;
		public ConditionBoolSwitch(GameObject g): base(g){}
		public override status Evaluate(){
			if (conditionMet){
				return status.success;
			} else {
				return status.neutral;
			}
		}
	}
	public class ConditionFail : Condition{
		public ConditionFail(GameObject g): base(g){}
		public override status Evaluate(){
			return status.failure;
		}
	}

	public class ConditionCloseToObject : Condition{
		public Ref<GameObject> target;
		float dist;
		public ConditionCloseToObject(GameObject g, Ref<GameObject> t, float d) : base(g) {
			// conditionThought = "I need to get close to that "+t.name;
			target = t;
			dist = d;
		}
		public ConditionCloseToObject(GameObject g, Ref<GameObject> t) : base(g) {
			target = t;
			dist = 0.25f;
		}
		public override status Evaluate(){
			if (target.val == null)
				return status.neutral;
			if (Vector2.Distance(gameObject.transform.position, target.val.transform.position) < dist){
				return status.success;
			} else {
				return status.neutral;
			}
		}
	}
	public class ConditionSawObjectRecently : Condition {
		public Ref<GameObject> target;
		private Awareness awareness;
		public ConditionSawObjectRecently(GameObject g, Controllable c, Ref<GameObject> target): base(g){
			this.target = target;
			awareness = g.GetComponent<Awareness>();
		}
		public override status Evaluate(){
			if (target.val == null)
				return status.failure;
			if (awareness){
				if (awareness.knowledgebase.ContainsKey(target.val)){
					if (Time.time - awareness.knowledgebase[target.val].lastSeenTime < 5){
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
	public class ConditionObjectInPlace : Condition {
		public Ref<GameObject> target;
		private Ref<Vector2> place;
		public ConditionObjectInPlace(GameObject g, Controllable c, Ref<GameObject> target,  Ref<Vector2> place): base(g){
			this.target = target;
			this.place = place;
		}
		public override status Evaluate(){
			if (target.val){
				if (Vector2.Distance(target.val.transform.position, place.val) < 0.1)
					return status.success;
				return status.failure;
			}
			return status.failure;
		}
	}
	public class ConditionLookingAtObject : Condition {
		public Ref<GameObject> target;
		private Controllable controllable;
		public ConditionLookingAtObject(GameObject g, Controllable c, Ref<GameObject> target): base(g) {
			this.target = target;
			controllable = c;
		}
		public override status Evaluate(){
			Vector2 dif = (Vector2)gameObject.transform.position - (Vector2)target.val.transform.position;
			float angle = Toolbox.Instance.ProperAngle(dif.x, dif.y);
			if (Mathf.Abs(angle - controllable.directionAngle) < 20){
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
			if (inv){
				if (inv.holding){
					string holdingName = Toolbox.Instance.CloneRemover(inv.holding.name);
					if (holdingName == name){
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
	public class ConditionHoldingSpecificObject : Condition{
		// string name;
		Ref<GameObject> target;
		Inventory inv;
		public ConditionHoldingSpecificObject(GameObject g, Ref<GameObject> target) : base(g) {
			// conditionThought = "I need a "+t;
			// name = t;
			this.target = target;
			inv = gameObject.GetComponent<Inventory>();
		}
		public override status Evaluate(){
			if (target.val == null)
				return status.failure;
			if (inv){
				if (inv.holding){
					// Debug.Log(inv.holding);
					// Debug.Log(target.val);
					if (inv.holding.gameObject == target.val){
						return status.success;
					}else{
						return status.failure;
					}
				} else {
					return status.failure;
				}
			} else {
				return status.failure;
			}
		}
	}
	public class ConditionInFightMode : Condition{
		Controllable control;
		public ConditionInFightMode(GameObject g, Controllable c) : base(g) {
			control = c;
		}
		public override status Evaluate(){
			if (control.fightMode){
				return status.success;
			} else {
				return status.neutral;
			}
		}
	}
	public class ConditionKnowAboutFire : Condition{
		Awareness awareness;
		public ConditionKnowAboutFire(GameObject g) : base(g){
			awareness = g.GetComponent<Awareness>();
		}
		public override status Evaluate(){
			if (awareness){
				if (awareness.nearestFire.val != null){
					return status.success;
				} else {
					return status.failure;
				}
			} else {
				return status.failure;
			}
		}
	}

}