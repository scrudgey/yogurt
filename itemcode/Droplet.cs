using UnityEngine;
// using System.Collections;

public class Droplet : MonoBehaviour {
	void OnGroundImpact(Physical phys){
		MonoLiquid monoLiquid = GetComponent<MonoLiquid>();
		GameObject puddle = Instantiate(Resources.Load("Puddle"), transform.position, Quaternion.identity) as GameObject;
		puddle.layer = 8;
		PhysicalBootstrapper pb = GetComponent<PhysicalBootstrapper>();
		pb.DestroyPhysical();
		LiquidCollection.MonoLiquidify(puddle, monoLiquid.liquid);
	}
}
