using UnityEngine;
// using System.Collections;
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
			successCondition.Init(g);
			routines[0].Init(g,c);
			slewTime = Random.Range(0.3f,1.4f);
		}
		
		public status Update(){
			status returnStatus = status.neutral;
			if (slewTime > 0){
				slewTime -= Time.deltaTime;
			} else {
				status routineStatus =	routines[index].Update();
				returnStatus = successCondition.Evaluate();
				if (routineStatus == status.failure){
					Controller.ResetInput(control);
					index ++;
					// get next routine, or fail.
					if (index < routines.Count){
						slewTime = Random.Range(0.8f, 1.4f);
						routines[index].Init(gameObject, control);
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

		public static Goal testGoal(){
			GameObject tom = GameObject.Find("Tom");
			// define a new test goal with three routines.
			Goal newGoal = new Goal();
			newGoal.goalThought = "I'm filled with ennui.";
			newGoal.successCondition = new ConditionLocation(new Vector2(-0.5f,1f));
			
			RoutineWalkToGameobject w = new RoutineWalkToGameobject(tom);
			w.timeLimit = 2f;
			newGoal.routines.Add( w );
			RoutineWalkToPoint w2 = new RoutineWalkToPoint(new Vector2(00.5f,1f));
			w2.timeLimit = 2f;
			newGoal.routines.Add ( w2 );
			RoutineWalkToGameobject	w3 = new RoutineWalkToGameobject(tom);
			newGoal.routines.Add(w3);
			return newGoal;
		}

		public static Goal GetItemGoal(string target){
			Goal newGoal = new Goal();
			newGoal.goalThought = "I need a "+target+".";
			newGoal.successCondition = new ConditionHoldingObjectWithName(target);

			RoutineRetrieveNamedFromInv w = new RoutineRetrieveNamedFromInv( target );
			newGoal.routines.Add(w);
			RoutineGetNamedFromEnvironment w2 = new RoutineGetNamedFromEnvironment(target);
			newGoal.routines.Add(w2);
			RoutineWanderUntilFound w3 = new RoutineWanderUntilFound(target);
			newGoal.routines.Add(w3);
			return newGoal;
		}

		public static Goal WalkTo(GameObject target){
			Goal newGoal = new Goal();
			newGoal.goalThought = "I'm going to check out that "+target.name+".";
			newGoal.successCondition = new ConditionCloseToObject(target,0.4f);

			RoutineWalkToGameobject w = new RoutineWalkToGameobject(target);
			newGoal.routines.Add(w);

			return newGoal;
		}

		public static Goal HoseDown(GameObject target){
			Goal newgoal = new Goal();
			newgoal.goalThought = "I've got to do something about that "+target.name+".";
			newgoal.successCondition = new ConditionLocation(Vector2.zero);

			RoutineUseObjectOnTarget w = new RoutineUseObjectOnTarget(target);
			w.timeLimit = 1.5f;
			newgoal.routines.Add(w);

			return newgoal;
		}

		public static Goal WanderGoal(){
			// initialize a new goal
			Goal newGoal = new Goal();
			newGoal.goalThought = "I'm doing nothing in particular.";
			// set success condition
			newGoal.successCondition = new ConditionLocation(Vector2.zero);
			 
			// create routine
			RoutineWander w = new RoutineWander();

			// set routine and return goal
			newGoal.routines.Add(w);
			return newGoal;
		}

	}



}
