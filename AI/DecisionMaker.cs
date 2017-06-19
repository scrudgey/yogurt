﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using AI;

[System.Serializable]
public class Personality {
	public enum Bravery {neutral, cowardly, brave};
	public Bravery bravery;
	public enum Actor {no, yes};
	public Actor actor;
}

public class DecisionMaker : MonoBehaviour, IMessagable {
	public Controllable control;
	public GameObject thought;
	public Text thoughtText;
	public List<Priority> priorities;
	public Personality personality;
	public PriorityAttack priorityAttack;
	public PriorityRunAway priorityRunAway;
	public List<GameObject> initialAwareness;
	public GameObject possession;
	public Controllable.HitState hitState;
	void Start() {
		// make sure there's Awareness
		Awareness awareness = Toolbox.Instance.GetOrCreateComponent<Awareness>(gameObject);
		awareness.initialAwareness = initialAwareness;
		awareness.possession = possession;

		// initialize thought bubble
		thought = Instantiate(Resources.Load("UI/thoughtbubble"), gameObject.transform.position, Quaternion.identity) as GameObject;
		DistanceJoint2D dj = thought.GetComponent<DistanceJoint2D>();
		dj.connectedBody = GetComponent<Rigidbody2D>();
		dj.distance = 0.3f;
		thoughtText = thought.GetComponentInChildren<Text>();
		thoughtText.text = "";

		control = GetComponent<Controllable>();

		priorities = new List<Priority>();

		priorities.Add(new PriorityFightFire(gameObject, control));
		priorities.Add(new PriorityWander(gameObject, control));
		priorities.Add(new PriorityRunAway(gameObject, control));
		priorities.Add(new PriorityAttack(gameObject, control));
		priorities.Add(new PriorityReadScript(gameObject, control));
		priorities.Add(new PriorityProtectPossessions(gameObject, control));
	}
	public void ReceiveMessage(Message message){
		if (message is MessageHitstun){
			MessageHitstun hits = (MessageHitstun)message;
			hitState = hits.hitState;
		}
		if (hitState >= Controllable.HitState.unconscious)
			return;
		foreach(Priority priority in priorities){
			priority.ReceiveMessage(message);
		}
	}
	public void Update(){
		Priority activePriority = null;
		foreach(Priority priority in priorities){
			priority.Update();
			if (activePriority == null)
				activePriority = priority;
			if (activePriority.Urgency(personality) < priority.Urgency(personality))
				activePriority = priority;
			if (priority.urgency > priority.minimumUrgency)
				priority.urgency -= Time.deltaTime / 10f;
			if (priority.urgency < priority.minimumUrgency)
				priority.urgency += Time.deltaTime / 10f;
			priority.urgency = Mathf.Min(priority.urgency, Priority.urgencyMaximum);
		}
		if (hitState >= Controllable.HitState.unconscious)
			return;
		if (activePriority != null){
			// Debug.Log(activePriority.ToString() + " " + activePriority.Urgency(personality).ToString());
			activePriority.DoAct();
			// thoughtText = activePriority.goal.
			// Debug.Log(gameObject.name + " " + activePriority.GetType().ToString());
		}
	}
}
