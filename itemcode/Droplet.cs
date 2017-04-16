using UnityEngine;
// using System.Collections;

public class Droplet : MonoBehaviour {
	public Physical physical;
	void Awake(){
		physical = GetComponent<Physical>();
	}
	void OnGroundImpact(Physical phys){
		MonoLiquid monoLiquid = GetComponent<MonoLiquid>();
		GameObject puddle = Instantiate(Resources.Load("Puddle"), transform.position, Quaternion.identity) as GameObject;
		puddle.layer = 9;
		PhysicalBootstrapper pb = GetComponent<PhysicalBootstrapper>();
		pb.DestroyPhysical();
		LiquidCollection.MonoLiquidify(puddle, monoLiquid.liquid);
	}
	void OnCollisionEnter2D(Collision2D coll){
		if (physical.currentMode == Physical.mode.fly){
			Debug.Log("droplet fly collision");
		}
	}
}
