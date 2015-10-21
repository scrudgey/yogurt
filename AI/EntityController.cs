using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using AI;
using UnityEngine.UI;

public class Priority{
	public List<Goal> goalStack = new List<Goal>();
	public string name;

	public void ExtinguishObject(GameObject g){
		name = "extinguish";
		goalStack.Add(GoalFactory.GetItemGoal("fire extinguisher"));
		goalStack.Add(GoalFactory.WalkTo(g));
		goalStack.Add(GoalFactory.HoseDown(g));
		Debug.Log("switching priority to extinguish object");
	}

	public void Wander(){
		name = "wander";
		goalStack.Add(GoalFactory.WanderGoal());
	}
}

public class EntityController : MonoBehaviour {
	
	private Controllable control;
	public Routine routine;
	public Condition condition;
	public Goal goal;
//	public List<Goal> goalStack = new List<Goal>();
	public Priority priority;
	public GameObject thought;
	public Text thoughtText;
	private float slewTime;
	
	void Start () {

		// init controllable 
		control = GetComponent<Controllable>();	

		priority = new Priority();
		priority.Wander();

		// init the thought bubble
		thought = Instantiate( Resources.Load("UI/thoughtbubble"),gameObject.transform.position,Quaternion.identity ) as GameObject;
		DistanceJoint2D dj = thought.GetComponent<DistanceJoint2D>();
		dj.connectedBody = GetComponent<Rigidbody2D>();
		dj.distance = 0.3f;
		thoughtText = thought.GetComponentInChildren<Text>();
		thoughtText.text = "";
//		thought.transform.SetParent(transform);
//		thought.transform.
	}

	public void CheckPriority(){
		Debug.Log("checking priorty");
		if (priority.goalStack.Count > 0){
			goal = priority.goalStack[0];
			priority.goalStack.RemoveAt(0);
			goal.Init(gameObject,control);
			slewTime = UnityEngine.Random.Range(0.5f,1.0f);
		}
	}

	void Update () {
		if (slewTime <= 0){
			// pop the next goal if we need one
			if (goal == null && priority.goalStack.Count > 0){
				goal = priority.goalStack[0];
				priority.goalStack.RemoveAt(0);
				goal.Init(gameObject,control);
				slewTime = UnityEngine.Random.Range(0.5f,1.0f);
			}

			if (goal == null && priority.goalStack.Count == 0){
				priority.Wander();
			}

			// tell goal to update
			if (goal != null){
				//			thoughtText.text = goal.goalThought+" "+goal.routines[goal.index].routineThought+" "+goal.successCondition.conditionThought;
				thoughtText.text = goal.goalThought+" "+goal.routines[goal.index].routineThought;
				Routine.status goalStatus = goal.Update();
				if (goalStatus == Routine.status.success){
					//new goal
					goal = null;
					Controller.ResetInput(control);
				}
				if (goalStatus == Routine.status.failure){
					goal = null;
					Controller.ResetInput(control);
					// run and tell that
				}
			}
		} else {
			slewTime -= Time.deltaTime;
		}
	}
}