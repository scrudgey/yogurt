using UnityEngine;
// using System.Collections;
// using System.Xml.Serialization;
// using System.IO;

public class MonoLiquid : MonoBehaviour {

	public Liquid liquid;
	public Edible edible;

	void Awake(){
		edible = GetComponent<Edible>();
		if (!edible)
			edible = gameObject.AddComponent<Edible>();
	}

	 void Start () {
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer && liquid != null){
			spriteRenderer.color = liquid.color;
		}
	}

	public void LoadLiquid(string type){
		// make this fancier: retrieve liquid in buffer, only change variabel
		// if liquid was returned
		liquid = LiquidCollection.LoadLiquid(type);

		// add things here for changing edible properties according to liquid properties
	}

	void OnGroundImpact(Physical phys){
		GameObject puddle = Instantiate(Resources.Load("Puddle"),transform.position,Quaternion.identity) as GameObject;
		puddle.layer = 8;
		PhysicalBootstrapper pb = GetComponent<PhysicalBootstrapper>();
		pb.DestroyPhysical();
		LiquidCollection.MonoLiquidify(puddle,liquid);
		puddle.GetComponent<Edible>().offal = true;

		Destroy(gameObject);
	}

}
