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
				goal.Update();
			}
		}
		public virtual void ReceiveMessage(Message m){}
	}
	public class PriorityFightFire: Priority{
		public GameObject flamingObject;
		public PriorityFightFire(GameObject g, Controllable c): base(g, c) {
			goal = new GoalGetItem(g, c, "fire extinguisher");
		}
		Goal Extinguish(){
			Goal getExt = new GoalGetItem(gameObject, control, "fire extinguisher");
			getExt.successCondition = new ConditionHoldingObjectWithName(gameObject, "fire extinguisher");

			Goal approach = new GoalWalkToObject(gameObject, control, flamingObject);
			// approach.successCondition = new ConditionCloseToObject(gameObject, ref flamingObject);
			approach.requirements.Add(getExt);

			Goal goal = new GoalHoseDown(gameObject, control, ref flamingObject);
			goal.requirements.Add(approach);

			return goal;
		}
	}

	public class PriorityWander: Priority{
		public PriorityWander(GameObject g, Controllable c): base(g, c) {
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
			goal = new GoalWander(g, c);
		}
		public override void ReceiveMessage(Message incoming){
			if (incoming is MessageDamage){
				MessageDamage dam = (MessageDamage)incoming;
				// urgency += 1;
				threat = dam.responsibleParty[0];
				goal = new GoalRunFromObject(gameObject, control, threat);
			}
		}
	}

	[System.Serializable]
	public class PriorityAttack: Priority{
		public HashSet<GameObject> enemies = new HashSet<GameObject>();
		public GameObject closestEnemy{
			get {
				return _closestEnemy;
				}
			set {
				_closestEnemy = value;
				fightGoal.ChangeTarget(value);
			}
		}
		private GameObject _closestEnemy;
		private float updateInterval;
		private Inventory inventory;
		public GoalWalkToObject fightGoal;
		public PriorityAttack(GameObject g, Controllable c): base(g, c){
			// goal = new GoalWander(g, c);
			inventory = gameObject.GetComponent<Inventory>();

			Goal dukesUp = new GoalDukesUp(gameObject, control, inventory);
			dukesUp.successCondition = new ConditionInFightMode(g, inventory);

			fightGoal = new GoalWalkToObject(gameObject, control, closestEnemy);
			fightGoal.successCondition = new ConditionCloseToObject(gameObject, closestEnemy);
			fightGoal.requirements.Add(dukesUp);

			goal = fightGoal;
		}
		public override void ReceiveMessage(Message incoming){
			if (incoming is MessageDamage){
				MessageDamage dam = (MessageDamage)incoming;
				urgency = 5;
				enemies.Add(dam.responsibleParty[0]);
				// goal = AttackEnemy();
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
			Debug.Log(closestEnemy);
		}
	}
}