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
			Init(g, c);
		}
		protected void Init(GameObject g, Controllable c){
			gameObject = g;
			control = c;
		}
		protected virtual status DoUpdate(){
			return status.neutral;
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

	public class RoutineWanderUntilFound: Routine {
		private string target;
		private Awareness awareness;
		private float checkInterval;
		private RoutineWander wander;
		private RoutineGetNamedFromEnvironment getIt;
		private int mode;
		public RoutineWanderUntilFound(GameObject g, Controllable c, string t) : base(g, c){
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
			List<GameObject> objs = new List<GameObject>();
			if (awareness){
				objs = awareness.FindObjectWithName(targetName);
			}
			if (objs.Count > 0){
				target = objs[0];
			}
			if (target){
				walkToRoutine = new RoutineWalkToGameobject(gameObject, control, new Ref<GameObject>(target));
			}
		}
		protected override status DoUpdate(){
			if (target){
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
	
	public class RoutineWander : Routine {
		private float wanderTime = 0;
		public enum direction {left, right, up, down, none}
		private direction dir;
		public RoutineWander(GameObject g, Controllable c) : base(g, c) {
			routineThought = "I'm wandering around.";
		}
		protected override status DoUpdate ()
		{
			Controller.ResetInput(control);
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
				wanderTime = UnityEngine.Random.Range(0,2);
				dir = (direction)(UnityEngine.Random.Range(0,4));
			} else {
				wanderTime -= Time.deltaTime;
			}
			return status.neutral;
		}
	}

	public class RoutineWalkToPoint : Routine {
		public Vector2 target = new Vector2(0,0);
		public RoutineWalkToPoint(GameObject g, Controllable c, Vector2 t) : base(g, c){
			routineThought = "I'm walking to a spot.";
			target = t;
		}
		protected override status DoUpdate () 
		{
			float distToTarget = Vector2.Distance(gameObject.transform.position,target);
			Controller.ResetInput(control);
			if (distToTarget < 0.2f){
				return status.success;
			} else {
				if ( Math.Abs( gameObject.transform.position.x - target.x) > 0.1f ){
					if (gameObject.transform.position.x < target.x){
						control.rightFlag = true;
					} 
					if (gameObject.transform.position.x > target.x){
						control.leftFlag = true;
					}
				}
				if ( Math.Abs( gameObject.transform.position.y - target.y) > 0.1f ){
					if (gameObject.transform.position.y < target.y){
						control.upFlag = true;
					}
					if (gameObject.transform.position.y > target.y){
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
			if (target.val){
				control.SetDirection(Vector2.ClampMagnitude(target.val.transform.position - gameObject.transform.position, 1f ));
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
				if (distToTarget < 0.2f){
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
				Debug.Log(target);
				return status.failure;
			}
		}
	}

	public class RoutineAvoidGameObject : Routine {
		public Ref<GameObject> threat;
		private Transform cachedTransform;
		private GameObject cachedGameObject;
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
				if ( Math.Abs(transform.position.x - threatTransform.position.x) > 0.1f ){
					if (transform.position.x < threatTransform.position.x){
						control.leftFlag = true;
					} 
					if (transform.position.x > threatTransform.position.x){
						control.rightFlag = true;
					}
				}
				if ( Math.Abs(transform.position.y - threatTransform.position.y) > 0.1f ){
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
		public Inventory inv;
		public RoutineToggleFightMode(GameObject g, Controllable c, Inventory i) : base(g, c) {
			routineThought = "I need to prepare for battle!";
			inv = i;
		}
		protected override status DoUpdate(){
			inv.ToggleFightMode();
			return status.success;
		}
	}
}