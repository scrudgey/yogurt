using UnityEngine;

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
		public virtual void Update () {

		}
		public virtual void GoalFinished(status goalStatus){

		}
		public void DoAct(){
			// the code to actually enact the current goal
			if (goal != null){
				status goalStatus = goal.Update();
				if (goalStatus != status.neutral)
					GoalFinished(goalStatus);
			}
		}
		public virtual void ReceiveMessage(Message m){

		}
	}

	public class PriorityFightFire: Priority{
		public PriorityFightFire(GameObject g, Controllable c): base(g, c) {
			goal = GoalFactory.GetItemGoal(g, c, "fire extinguisher");
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
			goal = GoalFactory.WanderGoal(g, c);
			goal.Init(gameObject, control);
		}
		public override void Update(){
			urgency = 1;
		}
	}

	public class PriorityRunAway: Priority{
		GameObject threat;
		public PriorityRunAway(GameObject g, Controllable c): base(g, c){
			goal = GoalFactory.WanderGoal(g, c);
		}
		public override void ReceiveMessage(Message incoming){
			if (incoming is MessageDamage){
				MessageDamage dam = (MessageDamage)incoming;
				urgency += 1;
				threat = dam.responsibleParty[0];
				goal = GoalFactory.RunFromObject(gameObject, control, threat);
			}
		}

	}

}