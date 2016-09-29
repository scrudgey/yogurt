using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using AI;

public class DecisionMaker : MonoBehaviour, IMessagable {

	public Controllable control;
	public GameObject thought;
	public Text thoughtText;

	public List<Priority> priorities;


	void Start() {
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
	}

	public void ReceiveMessage(Message message){
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
			if (activePriority.urgency < priority.urgency)
				activePriority = priority;
		}
		if (activePriority != null)
			activePriority.DoAct();
	}
}
