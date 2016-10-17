using UnityEngine;

namespace AI{
	[System.Serializable]
	public class Priority : IMessagable {
		public float urgency;
		public Awareness awareness;
		public Controllable control;
		public GameObject gameObject;
		public Goal goal;
		public Priority(GameObject g, Controllable c){
			InitReferences(g, c);
		}
		public void InitReferences(GameObject g, Controllable c){
			gameObject = g;
			control = c;
			awareness = g.GetComponent<Awareness>();
		}
		public virtual void Update(){}
		public void DoAct(){
			if (goal != null){
				goal.Update();
			}
		}
		public virtual float Urgency(Personality personality){
			return urgency;
		}
		public virtual void ReceiveMessage(Message m){}
	}
	public class PriorityFightFire: Priority{
		public Ref<GameObject> flamingObject = new Ref<GameObject>(null);
		public float updateInterval;
		public PriorityFightFire(GameObject g, Controllable c): base(g, c) {
			Goal getExt = new GoalGetItem(gameObject, control, "fire_extinguisher");

			Goal wander = new GoalWander(gameObject, control);
			wander.successCondition = new ConditionKnowAboutFire(gameObject);
			wander.requirements.Add(getExt);

			Goal approach = new GoalWalkToObject(gameObject, control, flamingObject);
			approach.requirements.Add(wander);

			goal = new GoalHoseDown(gameObject, control, flamingObject);
			goal.requirements.Add(approach);
		}
		public override void Update(){
			if (updateInterval > 0){
				updateInterval -= Time.deltaTime;
				return;
			}
			updateInterval = 0.5f;
			flamingObject.val = awareness.nearestFire();
			if (flamingObject.val != null)
				urgency = 10;
		}
	}

	public class PriorityWander: Priority{
		public PriorityWander(GameObject g, Controllable c): base(g, c) {
			goal = new GoalWander(g, c);
		}
		public override void Update(){
			urgency = 1;
		}
	}

	public class PriorityRunAway: Priority{
		Ref<GameObject> threat = new Ref<GameObject>(null);
		public PriorityRunAway(GameObject g, Controllable c): base(g, c){
			goal = new GoalWander(g, c);
		}
		public override void ReceiveMessage(Message incoming){
			if (incoming is MessageDamage){
				MessageDamage dam = (MessageDamage)incoming;
				urgency += 1;
				threat.val = dam.responsibleParty[0];
				goal = new GoalRunFromObject(gameObject, control, threat);
			}
		}
	}

	public class PriorityAttack: Priority{
		public Ref<GameObject> closestEnemy = new Ref<GameObject>(null);
		private float updateInterval;
		private Inventory inventory;
		public PriorityAttack(GameObject g, Controllable c): base(g, c){
			inventory = gameObject.GetComponent<Inventory>();

			Goal dukesUp = new GoalDukesUp(gameObject, control, inventory);
			dukesUp.successCondition = new ConditionInFightMode(g, control);

			Goal fightGoal = new GoalWalkToObject(gameObject, control, closestEnemy);
			fightGoal.successCondition = new ConditionCloseToObject(gameObject, closestEnemy);
			fightGoal.requirements.Add(dukesUp);

			goal = fightGoal;
		}
		public override void ReceiveMessage(Message incoming){
			if (incoming is MessageDamage){
				// MessageDamage dam = (MessageDamage)incoming;
				urgency = 5;
			}
		}
		public override void Update(){
			if (updateInterval > 0){
				updateInterval -= Time.deltaTime;
				return;
			}
			updateInterval = 0.5f;
			closestEnemy.val = awareness.nearestEnemy();
		}
	}

	public class PriorityReadScript: Priority {
		// ScriptDirector
		public PriorityReadScript(GameObject g, Controllable c): base(g, c){

		}
		public override float Urgency(Personality personality){
			if (personality.actor == Personality.Actor.yes){
				return -1;
			} else {
				return -1;
			}
		}
		public override void ReceiveMessage(Message incoming){
			if (incoming is MessageScript){
				MessageScript message = (MessageScript)incoming;
				ScriptDirector director = (ScriptDirector)incoming.messenger;

				Vector3 dif = director.transform.position - gameObject.transform.position;
				Vector2 direction = (Vector2)dif;
				control.direction = direction;
				control.SetDirection(direction);

				// say my line
				Toolbox.Instance.SendMessage(gameObject, director, new MessageSpeech(message.coStarLine));
			}
		}
	}
}