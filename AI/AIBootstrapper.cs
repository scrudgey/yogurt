using UnityEngine;

public class AIBootstrapper : MonoBehaviour {
	public Awareness awareness;
	public DecisionMaker decisionMaker;
	public Controllable controllable;

	void Start(){
		controllable = GetComponent<Controllable>();
		awareness = Toolbox.Instance.GetOrCreateComponent<Awareness>(gameObject);
		decisionMaker = Toolbox.Instance.GetOrCreateComponent<DecisionMaker>(gameObject);
		decisionMaker.control = controllable;
	}

}
