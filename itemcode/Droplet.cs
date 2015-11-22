using UnityEngine;
// using System.Collections;

public class Droplet : MonoBehaviour {

	public Edible edible;
	private MonoLiquid monoLiquid;
	
	void Start () {
		monoLiquid = gameObject.AddComponent<MonoLiquid>();
		LiquidCollection.LoadMonoLiquid("Test Liquid 2",monoLiquid);
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer){
			spriteRenderer.color = monoLiquid.liquid.color;
		}
	}
	
	public void LoadLiquid(string type){
		// make this fancier: retrieve liquid in buffer, only change variabel
		// if liquid was returned
		monoLiquid.liquid = LiquidCollection.LoadLiquid(type);
		
		// add things here for changing edible properties according to liquid properties
	}
	
	void OnGroundImpact(Physical phys){
		GameObject puddle = Instantiate(Resources.Load("Puddle"),transform.position,Quaternion.identity) as GameObject;
		puddle.layer = 8;
		PhysicalBootstrapper pb = GetComponent<PhysicalBootstrapper>();
		pb.DestroyPhysical();
		LiquidCollection.MonoLiquidify(puddle,monoLiquid.liquid);
	}
}
