using UnityEngine;

public class Package : Interactive {
	public string contents;
	void Start () {
		if (contents == "none")
			return;
		Gibs gibs = gameObject.AddComponent<Gibs>();
		gibs.particle = Resources.Load("prefabs/"+contents) as GameObject;
		gibs.number = 1;
		gibs.damageCondition = damageType.any;
		Interaction openAct = new Interaction(this, "Open", "Open");
		interactions.Add(openAct);
	}
	public void Open(){
		Destructible destructo = GetComponent<Destructible>();
		destructo.Die();
	}
	public string Open_desc(){
		return "Open package";
	}
}
