using UnityEngine;
using System.Collections;


public class Edible : Interactive {



	public float nutrition;
	public bool vegetable;
	public bool meat;
	public bool immoral;
	public bool offal;
	public bool blendable;
	public Color pureeColor;

	virtual public void BeEaten(){

		PhysicalBootstrapper pb = GetComponent<PhysicalBootstrapper>();
		if (pb){
			pb.DestroyPhysical();
		}
		Destroy(gameObject);
	}

	public Liquid Liquify(){
		Liquid returnLiquid = LiquidCollection.LoadLiquid("Juice");
		returnLiquid.vegetable = vegetable;
		returnLiquid.meat = meat;
		returnLiquid.immoral = immoral;
		returnLiquid.nutrition = nutrition / 10;
		returnLiquid.color = pureeColor;
		returnLiquid.name = gameObject.name + " Juice";

		return returnLiquid;
	}

}
