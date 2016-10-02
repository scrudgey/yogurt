using UnityEngine;
using System.Collections.Generic;

namespace AI{
	[System.Serializable]
	public class Priority : IMessagable {
		public float urgency;
		public Controllable control;
		public GameObject gameObject;
		public Goal goal;
		public Priority(GameObject g, Controllable c){
			InitReferences(g, c);
		}
		public void InitReferences(GameObject g, Controllable c){
			gameObject = g;
			control = c;
		}
		public virtual void Update(){}
		public void DoAct(){
			if (goal != null){
				status goalStatus = goal.Update();
				if (goalStatus != status.neutral)
					GoalFinished(goalStatus);
			}
		}
		public virtual void ReceiveMessage(Message m){}
		public virtual void GoalFinished(status goalStatus){}
	}

	public class PriorityFightFire: Priority{
		public PriorityFightFire(GameObject g, Controllable c): base(g, c) {
			// goal = GoalFactory.GetItemGoal(g, c, "fire extinguisher");
			goal = new GoalGetItem(g, c, "fire extinguisher");
		}
		// get a fire extinguisher if I don't have one
		// walk to and hose down whatever flaming object I know about
		// if I don't know of any flaming objects, search for one

		// goalStack.Add(GoalFactory.WalkTo(g));
		// goalStack.Add(GoalFactory.HoseDown(g));
		public override void Update(){
		}
		public override void GoalFinished(status goalStatus){
		}
	}

	public class PriorityWander: Priority{
		public PriorityWander(GameObject g, Controllable c): base(g, c) {
			// goal = GoalFactory.WanderGoal(g, c);
			goal = new GoalWander(g, c);
			goal.Init(gameObject, control);
		}
		public override void Update(){
			urgency = 1;
		}
	}

	public class PriorityRunAway: Priority{
		GameObject threat;
		public PriorityRunAway(GameObject g, Controllable c): base(g, c){
			// goal = GoalFactory.WanderGoal(g, c);
			goal = new GoalWander(g, c);
		}
		public override void ReceiveMessage(Message incoming){
			if (incoming is MessageDamage){
				MessageDamage dam = (MessageDamage)incoming;
				urgency += 1;
				threat = dam.responsibleParty[0];
				// goal = GoalFactory.RunFromObject(gameObject, control, threat);
				goal = new GoalRunFromObject(gameObject, control, threat);
			}
		}
	}

	public class PriorityAttack: Priority{
		public HashSet<GameObject> enemies = new HashSet<GameObject>();
		public GameObject closestEnemy;
		private float updateInterval;
		private Inventory inventory;
		public PriorityAttack(GameObject g, Controllable c): base(g, c){
			goal = new GoalWander(g, c);
			inventory = gameObject.GetComponent<Inventory>();
		}
		public override void ReceiveMessage(Message incoming){
			if (incoming is MessageDamage){
				MessageDamage dam = (MessageDamage)incoming;
				urgency += 1;
				enemies.Add(dam.responsibleParty[0]);
				goal = new GoalDukesUp(gameObject, control, inventory);
			}
		}

		public override void GoalFinished(status goalStatus){
			if (!inventory.fightMode){

			}
			if (goal is GoalDukesUp){

			}
		}
		public override void Update(){
			if (updateInterval > 0){
				updateInterval -= Time.deltaTime;
				return;
			}
			updateInterval = 0.5f;
			float closestDist = 999f;
			if (closestEnemy != null){
				closestDist = Vector2.Distance(gameObject.transform.position, closestEnemy.transform.position);
			}
			foreach (GameObject enemy in enemies){
				float dist = Vector2.Distance(gameObject.transform.position, enemy.transform.position);
				if (closestDist > dist){
					closestEnemy = enemy;
					closestDist = dist;
				}
			}
		}
	}
}