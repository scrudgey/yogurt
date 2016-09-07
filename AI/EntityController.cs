using UnityEngine;
// using System.Collections;
using System.Collections.Generic;
// using System;
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
	}

	public void Wander(){
		name = "wander";
		goalStack.Add(GoalFactory.WanderGoal());
	}

	public void Stand(){
		name = "stand";
		goalStack = new List<Goal>();
	}
}

public class EntityController : MonoBehaviour {
	public AIBootstrapper bootstrapper;
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

		if (priority == null)	
			priority = new Priority();

		// init the thought bubble
		thought = Instantiate(Resources.Load("UI/thoughtbubble"), gameObject.transform.position, Quaternion.identity) as GameObject;
		DistanceJoint2D dj = thought.GetComponent<DistanceJoint2D>();
		dj.connectedBody = GetComponent<Rigidbody2D>();
		dj.distance = 0.3f;
		thoughtText = thought.GetComponentInChildren<Text>();
		thoughtText.text = "";
	}

	public void CheckPriority(){
		if (priority.goalStack.Count > 0){
			goal = priority.goalStack[0];
			priority.goalStack.RemoveAt(0);
			goal.Init(gameObject, control);
			slewTime = UnityEngine.Random.Range(0.5f,1.0f);
		}
	}

	void Update () {
		if (slewTime <= 0){
			// pop the next goal if we need one
			if (goal == null && priority.goalStack.Count > 0){
				goal = priority.goalStack[0];
				priority.goalStack.RemoveAt(0);
				goal.Init(gameObject, control);
				slewTime = UnityEngine.Random.Range(0.5f, 1.0f);
			}

			// could this be where the decision maker comes in?
			if (goal == null && priority.goalStack.Count == 0){
				// priority.Wander();
				bootstrapper.InitializeController();
			}
			
			// tell goal to update
			if (goal != null){
				//			thoughtText.text = goal.goalThought+" "+goal.routines[goal.index].routineThought+" "+goal.successCondition.conditionThought;
				thoughtText.text = goal.goalThought+" "+goal.routines[goal.index].routineThought;
				status goalStatus = goal.Update();
				if (goalStatus == status.success){
					//new goal
					goal = null;
					Controller.ResetInput(control);
				}
				if (goalStatus == status.failure){
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