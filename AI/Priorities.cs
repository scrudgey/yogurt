using UnityEngine;

namespace AI{
	[System.Serializable]
	public class Priority : IMessagable {
		public float urgency;
		public string name;
		public AIBootstrapper bootstrapper;
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
			}
		}

		public void ReceiveMessage(Message m){

		}
	}

	public class PriorityFightFire: Priority{
		public PriorityFightFire(GameObject g, Controllable c) : base(g, c) {
			name = "extinguish";
			goal = GoalFactory.GetItemGoal("fire extinguisher");
		}
		public override void Update(){
			// get a fire extinguisher if I don't have one
			// walk to and hose down whatever flaming object I know about
			// if I don't know of any flaming objects, search for one

			// goalStack.Add(GoalFactory.WalkTo(g));
			// goalStack.Add(GoalFactory.HoseDown(g));
		}
	}

	public class PriorityWander: Priority{
		public PriorityWander(GameObject g, Controllable c) : base(g, c) {
			name = "wander";
			goal = GoalFactory.WanderGoal();
			goal.Init(gameObject, control);
		}
		public override void Update(){
			urgency = 1;
		}
	}

}