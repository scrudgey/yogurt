using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class Goal{
		public List<Routine> routines = new List<Routine>();
		public int index;
		public Condition successCondition;
		public GameObject gameObject;
		public Controllable control;
		public float slewTime;
		public string goalThought = "I'm just doing my thing.";
		public Goal(GameObject g, Controllable c){
			Init(g, c);
		}
		public Routine getRoutine(){
			if (index <= routines.Count -1)
				return routines[index];
			else 
				return null;
		}
		public void Init(GameObject g, Controllable c){
			// associate this goal with the relevant object. also associate 
			// success conditions, and the first routine.
			gameObject = g;
			control = c;
			slewTime = Random.Range(0.3f,1.4f);
		}
		
		public virtual status Update(){
			status returnStatus = status.neutral;
			if (slewTime > 0){
				slewTime -= Time.deltaTime;
			} else {
				status routineStatus = routines[index].Update();
				returnStatus = successCondition.Evaluate();
				if (routineStatus == status.failure){
					Controller.ResetInput(control);
					index ++;
					// get next routine, or fail.
					if (index < routines.Count){
						slewTime = Random.Range(0.8f, 1.4f);
					} else {
						returnStatus = status.failure;
						slewTime = Random.Range(0.3f, 1.4f);
					}
				}
			}
			return returnStatus;
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

	public class GoalWalkTo : Goal {
		public GoalWalkTo(GameObject g, Controllable c, GameObject target) : base(g, c){
			goalThought = "I'm going to check out that "+target.name+".";
			successCondition = new ConditionCloseToObject(g, target, 0.4f);
			routines.Add(new RoutineWalkToGameobject(g, c, target));
		}
	}

	public class GoalHoseDown : Goal {
		public GoalHoseDown(GameObject g, Controllable c, GameObject target) : base(g, c){
			goalThought = "I've got to do something about that "+target.name+".";
			successCondition = new ConditionLocation(g, Vector2.zero);
			RoutineUseObjectOnTarget w = new RoutineUseObjectOnTarget(g, c, target);
			w.timeLimit = 1.5f;
			routines.Add(w);
		}
	}

	public class GoalWander : Goal {
		public GoalWander(GameObject g, Controllable c) : base(g, c){
			goalThought = "I'm doing nothing in particular.";
			successCondition = new ConditionLocation(g, Vector2.zero);
			routines.Add(new RoutineWander(g, c));
		}
	}

	public class GoalRunFromObject : Goal {
		public GoalRunFromObject(GameObject g, Controllable c, GameObject threat) : base(g, c){
			goalThought = "I'm trying to avoid a bad thing.";
			successCondition = new ConditionLocation(g, Vector2.zero);
			routines.Add(new RoutineAvoidGameObject(g, c, threat));
		}
	}

	public class GoalDukesUp : Goal {
		private Inventory inv;
		public GoalDukesUp(GameObject g, Controllable c, Inventory i) : base(g, c){
			inv = i;
			successCondition = new ConditionInFightMode(g, i);
			routines.Add(new RoutineToggleFightMode(g, c, i));
			routines.Add(new RoutineToggleFightMode(g, c, i));
		}
	}
}
