using UnityEngine;
// using System.Collections;
using System.Collections.Generic;
// using AI;

namespace AI{

	public class Priority{
		private GameObject _gameObject;
		public GameObject gameObject{
			get{return _gameObject;}
			set{_gameObject = value;
				GameObjectInit();}
		}
		public Controllable control;
		public List<Goal> goalStack = new List<Goal>();
		public Goal goal;
		public string name;
		public float priority;
		public float slewTime;

		public virtual void Update(){
			// pop the next goal if we need one
			if (goal == null && goalStack.Count > 0){
				goal = goalStack[0];
				goalStack.RemoveAt(0);
				goal.Init(gameObject,control);
				slewTime = UnityEngine.Random.Range(0.5f,1.0f);
			}
			
			// tell goal to update
			if (goal != null){
				Routine.status goalStatus = goal.Update();
				if (goalStatus == Routine.status.success){
					goal = null;
					Controller.ResetInput(control);
				}
				if (goalStatus == Routine.status.failure){
					goal = null;
					Controller.ResetInput(control);
				}
			}
		}

		protected virtual void GameObjectInit(){
			// here we can init references to other components on the gameobject
		}

		public virtual void UpdatePriority(){
			// this code is called intermittently to decide which priority takes priority
			// this function call can also include culling behavior, if the priority drops too far
		}

	}



	public class PriorityFightFire: Priority{

		public PriorityFightFire(GameObject g){
			name = "extinguish";
			goalStack.Add(GoalFactory.GetItemGoal("fire extinguisher"));
			goalStack.Add(GoalFactory.WalkTo(g));
			goalStack.Add(GoalFactory.HoseDown(g));
		}

	}

	public class PriorityWander: Priority{
		public PriorityWander(){
			name = "wander";
			goalStack.Add(GoalFactory.WanderGoal());
		}
	}

}