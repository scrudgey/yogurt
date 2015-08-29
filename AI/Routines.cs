using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AI{

	public class Routine {
		public string routineThought = "I have no idea what I'm doing!";

		protected GameObject gameObject;
		protected Controllable control;
		public enum status{neutral,success,failure}
		public float timeLimit =-1;
		protected float runTime = 0;
		
		public virtual void Init(GameObject g, Controllable c){
			gameObject = g;
			control = c;
		}
		protected virtual status DoUpdate(){
			return status.neutral;
		}

		// update is the routine called each frame by goal.
		// this bit first checks for a timeout, then calls a routine
		// that a child class can modify.
		public status Update(){
			runTime += Time.deltaTime;
			if (timeLimit > 0 && runTime > timeLimit){
//				Debug.Log("Routine timeout");
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

		public RoutineWanderUntilFound(string t){
			target = t;
			routineThought = "I'm looking around for a "+t+".";
			mode = 0;
		}

		public override void Init (GameObject g, Controllable c){
			base.Init(g,c);
			awareness = gameObject.GetComponent<Awareness>();
			wander = new RoutineWander();
			wander.Init(g,c);
		}

		protected override status DoUpdate(){
			if (mode == 0){   // wander part
				if (checkInterval > 1.5f){
					checkInterval = 0f;
					List<GameObject> objs = awareness.FindObjectWithName(target);
					if (objs.Count > 0){
						mode = 1;
						getIt = new RoutineGetNamedFromEnvironment(target);
						getIt.Init(gameObject,control);
					}
				}
//				Debug.Log("wander update");
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

		public RoutineGetNamedFromEnvironment(string t){
			routineThought = "I'm going to pick up that "+t+".";
			targetName = t;
		}

		public override void Init (GameObject g, Controllable c)
		{
			base.Init (g, c);
			
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
				walkToRoutine = new RoutineWalkToGameobject(target);
				walkToRoutine.Init(gameObject,control);
			}
		}

		protected override status DoUpdate(){
			if (target){
				if (Vector2.Distance(gameObject.transform.position,target.transform.position) > 0.2f){
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

		public override void Init (GameObject g, Controllable c)
		{
			base.Init (g, c);
			inv = gameObject.GetComponent<Inventory>();
		}

		public RoutineRetrieveNamedFromInv(string names){
			routineThought = "I'm checking my pockets for a "+names+".";
			targetName = names;
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
		public enum direction {left,right,up,down,none}
		private direction dir;

		public RoutineWander(){
			routineThought = "I'm wandering around.";
		}
		
		protected override status DoUpdate ()
		{
			Controller.ResetInput(control);
//			Debug.Log("wandering");
			
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

		public RoutineWalkToPoint(Vector2 t){
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
		public GameObject target;

		public RoutineUseObjectOnTarget(GameObject g){
			routineThought = "I'm using this object on "+g.name+".";
			target = g;
		}

		protected override status DoUpdate(){
			if (target){
				control.SetDirection(Vector2.ClampMagnitude( target.transform.position - gameObject.transform.position, 1f ));
				control.shootHeldFlag = true;
				return status.neutral;
			} else {
				return status.failure;
			}
		}
	}

	public class RoutineWalkToGameobject : Routine {
		
		public GameObject target;

		public RoutineWalkToGameobject(GameObject g){
			routineThought = "I'm walking over to the "+g.name+".";
			target =g;
		}

		protected override status DoUpdate()
		{
			if (target){
				float distToTarget = Vector2.Distance(gameObject.transform.position,target.transform.position);
				control.leftFlag = control.rightFlag = control.upFlag = control.downFlag = false;
				
				if (distToTarget < 0.2f){
					return status.success;
				} else {
					
					if ( Math.Abs( gameObject.transform.position.x - target.transform.position.x) > 0.1f ){
						if (gameObject.transform.position.x < target.transform.position.x){
							control.rightFlag = true;
						} 
						if (gameObject.transform.position.x > target.transform.position.x){
							control.leftFlag = true;
						}
					}
					
					if ( Math.Abs( gameObject.transform.position.y - target.transform.position.y) > 0.1f ){
						if (gameObject.transform.position.y < target.transform.position.y){
							control.upFlag = true;
						}
						if (gameObject.transform.position.y > target.transform.position.y){
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
}