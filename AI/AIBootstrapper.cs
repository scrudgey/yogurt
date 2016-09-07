using UnityEngine;
// using System.Collections;

public class AIBootstrapper : MonoBehaviour {
	public Awareness awareness;
	public EntityController controller;

	public enum InitialState{stand, wander}
	public InitialState initialState; 

	void Start(){
		awareness = Toolbox.Instance.GetOrCreateComponent<Awareness>(gameObject);
		controller = Toolbox.Instance.GetOrCreateComponent<EntityController>(gameObject);

		awareness.controller = controller;
		controller.bootstrapper = this;
		InitializeController();
	}

	public void InitializeController(){
		if (controller.priority == null)
			controller.priority = new Priority();
			
		switch(initialState){
			case InitialState.stand:
			break;
			case InitialState.wander:
			controller.priority.Wander();
			break;
			default:
			break;
		}
	}
}
