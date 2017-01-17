using UnityEngine;

namespace AI{
	[System.Serializable]
	public class Priority : IMessagable {
		public const float urgencyMinor = 1f;
		public const float urgencySmall = 2.5f;
		public const float urgencyLarge = 5f;
		public const float urgencyPressing = 10f;
		public float urgency;
		public float minimumUrgency = 0;
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
		public virtual void DoAct(){
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
		public float updateInterval;
		public PriorityFightFire(GameObject g, Controllable c): base(g, c) {
			Goal getExt = new GoalGetItem(gameObject, control, "fire_extinguisher");

			Goal wander = new GoalWander(gameObject, control);
			wander.successCondition = new ConditionKnowAboutFire(gameObject);
			wander.requirements.Add(getExt);

			Goal approach = new GoalWalkToObject(gameObject, control, awareness.nearestFire);
			approach.requirements.Add(wander);

			goal = new GoalHoseDown(gameObject, control, awareness.nearestFire);
			goal.requirements.Add(approach);
		}
		public override void Update(){
			if (awareness.nearestFire.val != null)
				urgency = Priority.urgencyPressing;
		}
	}

	public class PriorityWander: Priority{
		public PriorityWander(GameObject g, Controllable c): base(g, c) {
			goal = new GoalWander(g, c);
		}
		public override float Urgency(Personality personality){
			if (personality.actor == Personality.Actor.yes){
				return -1;
			} else {
				return Priority.urgencyMinor;
			}
		}
	}

	public class PriorityRunAway: Priority{
		public PriorityRunAway(GameObject g, Controllable c): base(g, c){
			goal = new GoalRunFromObject(gameObject, control, awareness.nearestEnemy);
		}
		public override void ReceiveMessage(Message incoming){
			if (incoming is MessageDamage){
				// MessageDamage dam = (MessageDamage)incoming;
				urgency += Priority.urgencyMinor;
			}
			if (incoming is MessageInsult){
				urgency += Priority.urgencyMinor;
			}
			if (incoming is MessageThreaten){
				urgency += Priority.urgencySmall;
			}
		}
		public override float Urgency(Personality personality){
			if (personality.bravery == Personality.Bravery.brave)
				return urgency / 2f;
			if (personality.bravery == Personality.Bravery.cowardly)
				return urgency * 2f;
			return urgency;
		}
		public override void Update(){
			if (awareness.nearestEnemy.val == null)
				urgency -= Time.deltaTime / 10f;
		}
	}

	public class PriorityAttack: Priority{
		private Inventory inventory;
		private float updateInterval;
		public PriorityAttack(GameObject g, Controllable c): base(g, c){
			inventory = gameObject.GetComponent<Inventory>();

			Goal dukesUp = new GoalDukesUp(gameObject, control, inventory);
			dukesUp.successCondition = new ConditionInFightMode(g, control);

			Goal approachGoal = new GoalWalkToObject(gameObject, control, awareness.nearestEnemy);
			approachGoal.successCondition = new ConditionCloseToObject(gameObject, awareness.nearestEnemy);
			approachGoal.requirements.Add(dukesUp);

			Goal punchGoal = new Goal(gameObject, control);
			punchGoal.routines.Add(new RoutinePunchAt(gameObject, control, awareness.nearestEnemy));
			punchGoal.requirements.Add(approachGoal);

			goal = punchGoal;
		}
		public override void ReceiveMessage(Message incoming){
			if (incoming is MessageDamage){
				urgency += Priority.urgencyLarge;
			}
			if (incoming is MessageInsult){
				urgency += Priority.urgencySmall;
			}
			if (incoming is MessageThreaten){
				urgency += Priority.urgencyMinor;
			}
		}
		public override void Update(){
			if (awareness.nearestEnemy.val == null)
				urgency -= Time.deltaTime / 10f;
		}
		public override float Urgency(Personality personality){
			if (personality.bravery == Personality.Bravery.brave)
				return urgency * 2f;
			if (personality.bravery == Personality.Bravery.cowardly)
				return urgency / 2f;
			return urgency;
		}
	}

	public class PriorityReadScript: Priority {
		string nextLine;
		ScriptDirector director;
		public PriorityReadScript(GameObject g, Controllable c): base(g, c){
			GameObject video = GameObject.FindObjectOfType<VideoCamera>().gameObject;
			Goal goalWalkTo = new GoalWalkToPoint(g, c, new Ref<Vector2>(new Vector2(0.186f, 0.812f)));
			Goal lookGoal = new GoalLookAtObject(g, c, new Ref<GameObject>(video));
			lookGoal.requirements.Add(goalWalkTo);
			goal = lookGoal;
		}
		public override float Urgency(Personality personality){
			if (personality.actor == Personality.Actor.yes){
				if (nextLine != null){
					return Priority.urgencyPressing;
				} else {
					return 0.1f;
				}
			} else {
				return -1;
			}
		}
		public override void ReceiveMessage(Message incoming){
			if (incoming is MessageScript){
				MessageScript message = (MessageScript)incoming;
				director = (ScriptDirector)incoming.messenger;
				nextLine = message.coStarLine;
			}
		}
		public override void DoAct(){
			if (goal != null){
				goal.Update();
			}
			if (nextLine != null){
				Vector3 dif = director.transform.position - gameObject.transform.position;
				Vector2 direction = (Vector2)dif;
				control.direction = direction;
				control.SetDirection(direction);

				Toolbox.Instance.SendMessage(gameObject, control, new MessageSpeech(nextLine));
				nextLine = null;
			}
		}
	}
}