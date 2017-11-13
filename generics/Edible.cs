using UnityEngine;

public class Edible : Interactive {
	public float nutrition;
	public bool vegetable;
	public bool meat;
	public bool immoral;
	public bool offal;
    // public bool poison;
    public bool vomit;
	public bool blendable;
	public string blend_liquid_name;
	public bool human;
	public Color pureeColor;
	public AudioClip eatSound;
	virtual public void BeEaten(){
		PhysicalBootstrapper pb = GetComponent<PhysicalBootstrapper>();
		if (pb){
			pb.DestroyPhysical();
		}
		ClaimsManager.Instance.WasDestroyed(gameObject);
		Destroy(gameObject);
	}
	public Liquid Liquify(){
		Liquid returnLiquid = null;
		if (blend_liquid_name != ""){
			returnLiquid = Liquid.LoadLiquid(blend_liquid_name);
		} else {
			returnLiquid = Liquid.LoadLiquid("juice");
			returnLiquid.vegetable = vegetable;
			returnLiquid.meat = meat;
			returnLiquid.immoral = immoral;
			returnLiquid.nutrition = nutrition / 10;
			returnLiquid.color = pureeColor;
			returnLiquid.name = gameObject.name + " juice";
			// returnLiquid.human = human;
		}
		return returnLiquid;
	}
}
