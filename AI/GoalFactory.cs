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
		
		public status Update(){
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
						// routines[index].Init(gameObject, control);
					} else {
						returnStatus = status.failure;
						slewTime = Random.Range(0.3f, 1.4f);
					}
				}
			}
			return returnStatus;
		}
		
	}

	public class GoalFactory  {

		public static Goal testGoal(GameObject g, Controllable c){
			GameObject tom = GameObject.Find("Tom");
			Goal newGoal = new Goal();
			newGoal.goalThought = "I'm filled with ennui.";
			newGoal.successCondition = new ConditionLocation(g, new Vector2(-0.5f,1f));
			RoutineWalkToGameobject w = new RoutineWalkToGameobject(g, c, tom);
			w.timeLimit = 2f;
			newGoal.routines.Add( w );
			RoutineWalkToPoint w2 = new RoutineWalkToPoint(g, c, new Vector2(00.5f,1f));
			w2.timeLimit = 2f;
			newGoal.routines.Add ( w2 );
			newGoal.routines.Add(new RoutineWalkToGameobject(g, c, tom));
			return newGoal;
		}

		public static Goal GetItemGoal(GameObject g, Controllable c, string target){
			Goal newGoal = new Goal();
			newGoal.goalThought = "I need a "+target+".";
			newGoal.successCondition = new ConditionHoldingObjectWithName(g, target);
			newGoal.routines.Add(new RoutineRetrieveNamedFromInv(g, c, target));
			newGoal.routines.Add(new RoutineGetNamedFromEnvironment(g, c, target));
			newGoal.routines.Add(new RoutineWanderUntilFound(g, c, target));
			return newGoal;
		}

		public static Goal WalkTo(GameObject g, Controllable c, GameObject target){
			Goal newGoal = new Goal();
			newGoal.goalThought = "I'm going to check out that "+target.name+".";
			newGoal.successCondition = new ConditionCloseToObject(g, target, 0.4f);
			newGoal.routines.Add(new RoutineWalkToGameobject(g, c, target));
			return newGoal;
		}

		public static Goal HoseDown(GameObject g, Controllable c, GameObject target){
			Goal newgoal = new Goal();
			newgoal.goalThought = "I've got to do something about that "+target.name+".";
			newgoal.successCondition = new ConditionLocation(g, Vector2.zero);
			RoutineUseObjectOnTarget w = new RoutineUseObjectOnTarget(g, c, target);
			w.timeLimit = 1.5f;
			newgoal.routines.Add(w);
			return newgoal;
		}

		public static Goal WanderGoal(GameObject g, Controllable c){
			Goal newGoal = new Goal();
			newGoal.goalThought = "I'm doing nothing in particular.";
			newGoal.successCondition = new ConditionLocation(g, Vector2.zero);
			newGoal.routines.Add(new RoutineWander(g, c));
			return newGoal;
		}

		public static Goal RunFromObject(GameObject g, Controllable c, GameObject threat){
			Goal newGoal = new Goal();
			newGoal.goalThought = "I'm trying to avoid a bad thing.";
			newGoal.successCondition = new ConditionLocation(g, Vector2.zero);
			newGoal.routines.Add(new RoutineAvoidGameObject(g, c, threat));
			return newGoal;
		}
	}
}
