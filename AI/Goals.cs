using UnityEngine;
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
		public string goalThought = "I'm just doing my thing.";
		public Goal(GameObject g, Controllable c){
			// Init(g, c);
			gameObject = g;
			control = c;
			slewTime = Random.Range(0.3f, 1.4f);
		}
		public status Evaluate(){
			foreach (Goal requirement in requirements){
				if (requirement.Evaluate() != status.success){
					return status.failure;
				}
			}
			return successCondition.Evaluate();
		}
		public void Update(){
			foreach (Goal requirement in requirements){
				if (requirement.Evaluate() != status.success){
					requirement.Update();
					return;
				}
			}
			if (slewTime > 0){
				slewTime -= Time.deltaTime;
				return;
			}
			try {
				status routineStatus = routines[index].Update();
				if (routineStatus == status.failure){
					Controller.ResetInput(control);
					Debug.Log(routines[index]);
					index ++;
					// get next routine, or fail.
					if (index < routines.Count){
						slewTime = Random.Range(0.8f, 1.4f);
					} else {
						// what do do? reset from the start maybe
						// index = routines.Count - 1;
						index = 0;
					}
				}
			} catch {
				Debug.Log(this);
			}
		}
	}

	public class GoalGetItem : Goal {
		public GoalGetItem (GameObject g, Controllable c, string target) : base(g, c){
				goalThought = "I need a "+target+".";
				successCondition = new ConditionHoldingObjectWithName(g, target);
				routines.Add(new RoutineRetrieveNamedFromInv(g, c, target));
				routines.Add(new RoutineGetNamedFromEnvironment(g, c, target));
				routines.Add(new RoutineWanderUntilFound(g, c, target));
		}
	}

	public class GoalWalkToObject : Goal {
		public Ref<GameObject> target;
		public new string goalThought{
			get {return "I'm going to check out that "+target.val.name+".";}
		}
		public GoalWalkToObject(GameObject g, Controllable c, Ref<GameObject> t) : base(g, c){
			target = t;
			successCondition = new ConditionCloseToObject(g, target, 0.75f);
			routines.Add(new RoutineWalkToGameobject(g, c, target));
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
