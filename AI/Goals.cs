using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
	public class Ref<T> {
		public T val;
		public Ref(T t){
			val = t;
		}
	}

	public class Goal{
		public List<Routine> routines = new List<Routine>();
		public int index = 0;
		public List<Goal> requirements = new List<Goal>();
		public Condition successCondition;
		public GameObject gameObject;
		public Controllable control;
		public float slewTime;
		private bool fulfillingRequirements = true;
		public string goalThought = "I'm just doing my thing.";
		public Goal(GameObject g, Controllable c){
			gameObject = g;
			control = c;
			slewTime = UnityEngine.Random.Range(0.1f, 0.5f);
		}
		public status Evaluate(){
			foreach (Goal requirement in requirements){
				if (requirement.Evaluate() != status.success){
					return status.failure;
				}
			}
			return successCondition.Evaluate();
		}
		public virtual void Update(){
			// if i have any unmet requirements, my update goes to the first unmet one.
			foreach (Goal requirement in requirements){
				if (requirement.Evaluate() != status.success){
					fulfillingRequirements = true;
					requirement.Update();
					return;
				}
			}
			if (fulfillingRequirements){
				// Debug.Log(control.gameObject.name + " " + this.ToString() + " requirements met");;
				control.ResetInput();
			}
			fulfillingRequirements = false;
			if (slewTime > 0){
				slewTime -= Time.deltaTime;
				return;
			}
			try {
				status routineStatus = routines[index].Update();
				if (routineStatus == status.failure){
					control.ResetInput();
					index ++;
					// get next routine, or fail.
					if (index < routines.Count){
						slewTime = UnityEngine.Random.Range(0.1f, 0.5f);
					} else {
						// what do do? reset from the start maybe
						// index = routines.Count - 1;
						index = 0;
					}
					routines[index].Configure();
				}
			} catch (Exception e) {
				Debug.Log(this.ToString() + " fail: " + e.Message);
				Debug.Log(e.TargetSite);
			}
		}
	}
	public class GoalUsePhone : Goal {
		public bool phoneCalled;
		private RoutineUseTelephone telRoutine;
		public GoalUsePhone(GameObject g, Controllable c): base(g, c){
			Telephone phoneObject = GameObject.FindObjectOfType<Telephone>();
			if (phoneObject){
				Ref<GameObject> phoneRef = new Ref<GameObject>(phoneObject.gameObject);
				successCondition = new ConditionBoolSwitch(g);
				telRoutine = new RoutineUseTelephone(g, c, phoneRef, (ConditionBoolSwitch)successCondition);
				routines.Add(telRoutine);
			}
		}
		public override void Update(){
			base.Update();
			if (successCondition.Evaluate() == status.success && !phoneCalled){
				phoneCalled = true;
			}
		}
	}
	public class GoalReturnObject : Goal {
		public GoalReturnObject(GameObject g, Controllable c, GameObject target) : base(g,c ){
			
		}
	}
	public class GoalGetItem : Goal {
		public bool findingFail;
		public GoalGetItem (GameObject g, Controllable c, string target) : base(g, c){
				goalThought = "I need a "+target+".";
				successCondition = new ConditionHoldingObjectWithName(g, target);
				routines.Add(new RoutineRetrieveNamedFromInv(g, c, target));
				routines.Add(new RoutineGetNamedFromEnvironment(g, c, target));
				routines.Add(new RoutineWanderUntilFound(g, c, target));
		}
		public GoalGetItem (GameObject g, Controllable c, GameObject target) : base(g, c){
				goalThought = "I need a "+target+".";
				successCondition = new ConditionHoldingObjectWithName(g, target.name);
				routines.Add(new RoutineRetrieveNamedFromInv(g, c, target.name));
				routines.Add(new RoutineGetNamedFromEnvironment(g, c, target.name));
				routines.Add(new RoutineWanderUntilFound(g, c, target.name));
		}
		public override void Update(){
			base.Update();
			if (index == 2 && !findingFail){
				findingFail = true;
				Debug.Log("finding fail");
			}
		}
	}
	public class GoalWalkToObject : Goal {
		public Ref<GameObject> target;
		public new string goalThought{
			get {return "I'm going to check out that "+target.val.name+".";}
		}
		public GoalWalkToObject(GameObject g, Controllable c, Ref<GameObject> t) : base(g, c){
			target = t;
			successCondition = new ConditionCloseToObject(g, target, 0.55f);
			routines.Add(new RoutineWalkToGameobject(g, c, target));
		}
		public GoalWalkToObject(GameObject g, Controllable c, Type objType) : base(g, c){
			// GameObject targetObject = GameObject.FindObjectOfType<typeof(objType)>();
			UnityEngine.Object obj = GameObject.FindObjectOfType(objType);
			Component targetComponent = (Component)obj;
			if (targetComponent != null){
				GameObject targetObject = targetComponent.gameObject;
				target = new Ref<GameObject>(targetObject);
				routines.Add(new RoutineWalkToGameobject(g, c, target));
				successCondition = new ConditionCloseToObject(g, target, 0.25f);
			}
		}
	}
	public class GoalLookAtObject: Goal {
		public Ref<GameObject> target;
		public GoalLookAtObject(GameObject g, Controllable c, Ref<GameObject> target) : base(g, c){
			this.target = target;
			successCondition = new ConditionLookingAtObject(g, c, target);
			routines.Add(new RoutineLookAtObject(g, c, target));
		}
	}
	public class GoalWalkToPoint : Goal {
		public Ref<Vector2> target;
		public GoalWalkToPoint(GameObject g, Controllable c, Ref<Vector2> target) : base(g, c){
			this.target = target;
			successCondition = new ConditionLocation(g, target);
			routines.Add(new RoutineWalkToPoint(g, c, target));
		}
	}

	public class GoalHoseDown : Goal {
		public Ref<GameObject> target;
		public new string goalThought{
			get {return "I've got to do something about that "+target.val.name+".";}
		}
		public GoalHoseDown(GameObject g, Controllable c, Ref<GameObject> r) : base(g, c){
			successCondition = new ConditionFail(g);
			routines.Add(new RoutineUseObjectOnTarget(g, c, r));
		}
	}

	public class GoalWander : Goal {
		public GoalWander(GameObject g, Controllable c) : base(g, c){
			goalThought = "I'm doing nothing in particular.";
			successCondition = new ConditionLocation(g, new Ref<Vector2>(Vector2.zero));
			routines.Add(new RoutineWander(g, c));
		}
	}

	public class GoalRunFromObject : Goal {
		public GoalRunFromObject(GameObject g, Controllable c, Ref<GameObject> threat) : base(g, c){
			goalThought = "I'm trying to avoid a bad thing.";
			successCondition = new ConditionLocation(g, new Ref<Vector2>(Vector2.zero));
			routines.Add(new RoutineAvoidGameObject(g, c, threat));
		}
	}

	public class GoalDukesUp : Goal {
		public GoalDukesUp(GameObject g, Controllable c, Inventory i) : base(g, c){
			successCondition = new ConditionInFightMode(g, control);
			routines.Add(new RoutineToggleFightMode(g, c));
			routines.Add(new RoutineToggleFightMode(g, c));
		}
	}
}
