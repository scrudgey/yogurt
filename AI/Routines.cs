using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI{
	public enum status{neutral, success, failure}
	public class Routine {
		public string routineThought = "I have no idea what I'm doing!";
		protected GameObject gameObject;
		protected Controllable control;
		public float timeLimit = -1;
		protected float runTime = 0;
		Transform cachedTransform;
		public Transform transform
		{
			get
			{
				if( cachedTransform == null )
				{
					cachedTransform = gameObject.GetComponent<Transform>();
				}
				return cachedTransform;
			}
		}
		public Routine(GameObject g, Controllable c){
			gameObject = g;
			control = c;
		}
		protected virtual status DoUpdate(){
			return status.neutral;
		}
		public virtual void Configure(){

		}
		// update is the routine called each frame by goal.
		// this bit first checks for a timeout, then calls a routine
		// that is specific to the child class.
		public status Update(){
			runTime += Time.deltaTime;
			if (timeLimit > 0 && runTime > timeLimit){
				return status.failure;
			} else
				return DoUpdate();
		}
	}

	/* 
	 * Routines
	 * 
	 * */
	public class RoutineWanderUntilNamedFound: Routine {
		// TODO: routine's update mimics goal's requirements structure. replace this with a goal
		private string target;
		private Awareness awareness;
		private float checkInterval;
		private RoutineWander wander;
		private RoutineGetNamedFromEnvironment getIt;
		private int mode;
		public RoutineWanderUntilNamedFound(GameObject g, Controllable c, string t) : base(g, c){
			target = t;
			routineThought = "I'm looking around for a " + t + ".";
			mode = 0;
			awareness = gameObject.GetComponent<Awareness>();
			wander = new RoutineWander(gameObject, control);
		}
		protected override status DoUpdate(){
			if (mode == 0){   // wander part
				if (checkInterval > 1.5f){
					checkInterval = 0f;
					List<GameObject> objs = awareness.FindObjectWithName(target);
					if (objs.Count > 0){
						mode = 1;
						getIt = new RoutineGetNamedFromEnvironment(gameObject, control, target);
					}
				}
				checkInterval += Time.deltaTime;
				return wander.Update();
			} else {
				return getIt.Update();
			}
		}
	}
	
	public class RoutineUseTelephone: Routine {
		public Ref<GameObject> telRef;
		private Telephone telephone;
		private ConditionBoolSwitch condition;
		public RoutineUseTelephone(GameObject g, Controllable c, Ref<GameObject> telRef, ConditionBoolSwitch condition) : base(g, c){
			this.telRef = telRef;
			this.condition = condition;
			telephone = telRef.val.GetComponent<Telephone>();
		}
		protected override status DoUpdate(){
			if (telephone && !condition.conditionMet){
				telephone.FireButtonCallback();
				Debug.Log("called FD");
				condition.conditionMet = true;
				return status.success;
			} else {
				return status.failure;
			}
		}
	}

	public class RoutineGetNamedFromEnvironment: Routine {
		private Inventory inv;
		private Awareness awareness;
		private GameObject target;
		private string targetName;
		private RoutineWalkToGameobject walkToRoutine;
		public RoutineGetNamedFromEnvironment(GameObject g, Controllable c, string t) : base(g, c){
			routineThought = "I'm going to pick up that " + t + ".";
			targetName = t;
			inv = gameObject.GetComponent<Inventory>();
			awareness = gameObject.GetComponent<Awareness>();
			Configure();
		}
		public override void Configure(){
			List<GameObject> objs = new List<GameObject>();
			if (awareness){
				objs = awareness.FindObjectWithName(targetName);
			}
			if (objs.Count > 0){
				target = objs[0];
			}
			if (target && target.activeInHierarchy){
				walkToRoutine = new RoutineWalkToGameobject(gameObject, control, new Ref<GameObject>(target));
			}
		}
		protected override status DoUpdate(){
			if (target && target.activeInHierarchy){
				if (Vector2.Distance(transform.position, target.transform.position) > 0.2f){
					return walkToRoutine.Update();
				} else {
					// this bit is shakey
					inv.GetItem(target.GetComponent<Pickup>());
					return status.success;
				}
			} else {
				return status.failure;
			}
		}
	}
	public class RoutineGetRefFromEnvironment: Routine {
		private Inventory inv;
		// private Awareness awareness;
		private Ref<GameObject> target;
		private RoutineWalkToGameobject walkToRoutine;
		public RoutineGetRefFromEnvironment(GameObject g, Controllable c, Ref<GameObject> target) : base(g, c){
			// routineThought = "I'm going to pick up that " + t + ".";
			this.target = target;
			inv = gameObject.GetComponent<Inventory>();
			// awareness = gameObject.GetComponent<Awareness>();
			Configure();
		}
		public override void Configure(){
			// List<GameObject> objs = new List<GameObject>();
			if (target.val && target.val.activeInHierarchy){
				walkToRoutine = new RoutineWalkToGameobject(gameObject, control, target);
			}
		}
		protected override status DoUpdate(){
			if (target.val && target.val.activeInHierarchy){
				if (Vector2.Distance(transform.position, target.val.transform.position) > 0.2f){
					return walkToRoutine.Update();
				} else {
					// this bit is shakey
					inv.GetItem(target.val.GetComponent<Pickup>());
					return status.success;
				}
			} else {
				return status.failure;
			}
		}
	}

	public class RoutineRetrieveNamedFromInv : Routine {
		private Inventory inv;
		private string targetName;
		public RoutineRetrieveNamedFromInv(GameObject g, Controllable c, string names): base(g, c){
			routineThought = "I'm checking my pockets for a " + names + ".";
			targetName = names;
			inv = g.GetComponent<Inventory>();
		}
		protected override status DoUpdate(){
			if (inv){
				foreach (GameObject g in inv.items){
					if (targetName == g.name){
						inv.RetrieveItem(g.name);
						return status.success;
					}
				}
				return status.failure;
			}else {
				return status.failure;
			}
		}
	}
	public class RoutinePlaceObject : Routine {
		private Inventory inv;
		private Ref<Vector2> position;
		public RoutinePlaceObject(GameObject g, Controllable c, Ref<Vector2> position): base(g, c){
			this.position = position;
			inv = g.GetComponent<Inventory>();
		}
		protected override status DoUpdate(){
			if (inv){
				if (inv.holding){
					if (inv.PlaceItem(position.val)){
						return status.success;
					} else {
						return status.failure;
					}
				} 
				return status.failure;
			}else {
				return status.failure;
			}
		}
	}
	public class RoutineRetrieveRefFromInv : Routine {
		private Inventory inv;
		private Ref<GameObject> target;
		public RoutineRetrieveRefFromInv(GameObject g, Controllable c, Ref<GameObject> target): base(g, c){
			// routineThought = "I'm checking my pockets for a " + names + ".";
			this.target = target;
			inv = g.GetComponent<Inventory>();
		}
		protected override status DoUpdate(){
			if (inv){
				foreach (GameObject g in inv.items){
					if (target.val == g){
						inv.RetrieveItem(g.name);
						return status.success;
					}
				}
				return status.failure;
			}else {
				return status.failure;
			}
		}
	}
	
	public class RoutineWander : Routine {
		private float wanderTime = 0;
		public enum direction {left, right, up, down, none}
		private direction dir;
		public RoutineWander(GameObject g, Controllable c) : base(g, c) {
			routineThought = "I'm wandering around.";
		}
		protected override status DoUpdate ()
		{
			control.ResetInput();
			if (wanderTime > 0){
				switch(dir){
				case direction.down:
					control.downFlag = true;
					break;
				case direction.left:
					control.leftFlag = true;
					break;
				case direction.right:
					control.rightFlag = true;
					break;
				case direction.up:
					control.upFlag = true;
					break;
				}
			}
			if (wanderTime < -1f){
				wanderTime = UnityEngine.Random.Range(0, 2);
				dir = (direction)(UnityEngine.Random.Range(0, 4));
			} else {
				wanderTime -= Time.deltaTime;
			}
			return status.neutral;
		}
	}
	public class RoutineLookAtObject : Routine {
		public Ref<GameObject> target = new Ref<GameObject>(null);
		public RoutineLookAtObject(GameObject g, Controllable c, Ref<GameObject> target) : base(g, c){
			this.target = target;
		}
		protected override status DoUpdate (){
			control.ResetInput();
			Vector2 dif = (Vector2)gameObject.transform.position - (Vector2)target.val.transform.position;
			control.SetDirection(-1 * dif);
			return status.neutral;
		}
	}
	public class RoutineLookInDirection : Routine {
		public Vector2 dir;
		public RoutineLookInDirection(GameObject g, Controllable c, Vector2 dir) : base(g, c){
			this.dir = dir;
		}
		protected override status DoUpdate (){
			// Debug.Log("looking in direction "+dir.ToString());
			control.ResetInput();
			control.SetDirection(dir);
			return status.neutral;
		}
	}
	public class RoutineWalkToPoint : Routine {
		public Ref<Vector2> target = new Ref<Vector2>(Vector2.zero);
		public RoutineWalkToPoint(GameObject g, Controllable c, Ref<Vector2> t) : base(g, c){
			routineThought = "I'm walking to a spot.";
			target = t;
		}
		protected override status DoUpdate () 
		{
			float distToTarget = Vector2.Distance(gameObject.transform.position, target.val);
			control.ResetInput();
			if (distToTarget < 0.2f){
				return status.success;
			} else {
				if ( Math.Abs( gameObject.transform.position.x - target.val.x) > 0.1f ){
					if (gameObject.transform.position.x < target.val.x){
						control.rightFlag = true;
					} 
					if (gameObject.transform.position.x > target.val.x){
						control.leftFlag = true;
					}
				}
				if ( Math.Abs( gameObject.transform.position.y - target.val.y) > 0.1f ){
					if (gameObject.transform.position.y < target.val.y){
						control.upFlag = true;
					}
					if (gameObject.transform.position.y > target.val.y){
						control.downFlag = true;
					}
				}
				return status.neutral;
			}
		}
	}

	public class RoutineUseObjectOnTarget : Routine {
		public Ref<GameObject> target;
		public RoutineUseObjectOnTarget(GameObject g, Controllable c, Ref<GameObject> targetObject) : base(g, c){
			routineThought = "I'm using this object on " + g.name + ".";
			target = targetObject;
		}
		protected override status DoUpdate(){
			if (target.val != null){
				control.SetDirection(Vector2.ClampMagnitude(target.val.transform.position - gameObject.transform.position, 1f));
				control.shootHeldFlag = true;
				return status.neutral;
			} else {
				return status.failure;
			}
		}
	}

	public class RoutineWalkToGameobject : Routine {
		public Ref<GameObject> target;
		private Transform cachedTransform;
		private GameObject cachedGameObject;
		public float minDistance = 0.2f;
		public Transform targetTransform{
			get {
				if (cachedGameObject == target.val){
					if (cachedTransform != null){
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
		public RoutineWalkToGameobject(GameObject g, Controllable c, Ref<GameObject> targetObject) : base(g, c) {
			routineThought = "I'm walking over to the " + g.name + ".";
			target = targetObject;
		}
		protected override status DoUpdate()
		{
			if (target.val){
				float distToTarget = Vector2.Distance(transform.position, targetTransform.position);
				control.leftFlag = control.rightFlag = control.upFlag = control.downFlag = false;
				if (distToTarget < minDistance){
					return status.success;
				} else {
					if ( Math.Abs(transform.position.x - targetTransform.position.x) > 0.1f ){
						if (transform.position.x < targetTransform.position.x){
							control.rightFlag = true;
						} 
					if (transform.position.x > targetTransform.position.x){
							control.leftFlag = true;
						}
					}
					if ( Math.Abs(transform.position.y - targetTransform.position.y) > 0.1f ){
						if (transform.position.y < targetTransform.position.y){
							control.upFlag = true;
						}
						if (transform.position.y > targetTransform.position.y){
							control.downFlag = true;
						}
					}
					return status.neutral;
				}
			} else {
				return status.failure;
			}
		}
	}

	public class RoutineAvoidGameObject : Routine {
		public Ref<GameObject> threat;
		private Transform cachedTransform;
		private GameObject cachedGameObject;
		//TODO: make use of lastSeenPosition
		// private Vector3 lastSeenPosition;
		public Transform threatTransform{
			get {
				if (cachedGameObject == threat.val){
					if (cachedTransform != null){
						return cachedTransform;
					} else {
						cachedTransform = threat.val.transform;
						return cachedTransform;
					}
				} else {
					cachedGameObject = threat.val;
					cachedTransform = threat.val.transform;
					return cachedTransform;
				}
			}
		}
		public RoutineAvoidGameObject(GameObject g, Controllable c, Ref<GameObject> threatObject) : base(g, c) {
			routineThought = "Get me away from that "+g.name+" !";
			threat = threatObject;
		}
		protected override status DoUpdate()
		{
			if (threat.val){
				control.leftFlag = control.rightFlag = control.upFlag = control.downFlag = false;
				// lastSeenPosition = threatTransform.position;
				if (Math.Abs(transform.position.x - threatTransform.position.x) > 0.1f){
					if (transform.position.x < threatTransform.position.x){
						control.leftFlag = true;
					} 
					if (transform.position.x > threatTransform.position.x){
						control.rightFlag = true;
					}
				}
				if (Math.Abs(transform.position.y - threatTransform.position.y) > 0.1f){
					if (transform.position.y < threatTransform.position.y){
						control.downFlag = true;
					}
					if (transform.position.y > threatTransform.position.y){
						control.upFlag = true;
					}
				}
			}
			return status.neutral;
		}
	}
	public class RoutineToggleFightMode : Routine {
		public RoutineToggleFightMode(GameObject g, Controllable c) : base(g, c) {
			routineThought = "I need to prepare for battle!";
		}
		protected override status DoUpdate(){
			control.ToggleFightMode();
			return status.success;
		}
	}

	public class RoutinePunchAt : Routine {
		Ref<GameObject> target;
		float timer;
		public RoutinePunchAt(GameObject g, Controllable c, Ref<GameObject> t) : base(g, c){
			routineThought = "Fisticuffs!";
			target = t;
		}
		protected override status DoUpdate(){
			timer += Time.deltaTime;
			if (target.val != null){
				control.SetDirection(Vector2.ClampMagnitude(target.val.transform.position - gameObject.transform.position, 1f));
				if (timer > 0.5){
					timer = 0f;
					control.shootPressedFlag = true;
				} else {
					control.ResetInput();
				}
				return status.neutral;
			} else {
				return status.failure;
			}
		}
	}
}