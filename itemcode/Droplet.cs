using UnityEngine;

public class Droplet : MonoBehaviour {
    public Physical physical;
    void Awake() {
        physical = GetComponent<Physical>();
    }
    void OnGroundImpact(Physical phys) {
        MonoLiquid monoLiquid = GetComponent<MonoLiquid>();
        GameObject puddle = Instantiate(Resources.Load("prefabs/Puddle"), transform.position, Quaternion.identity) as GameObject;
        PhysicalBootstrapper pb = GetComponent<PhysicalBootstrapper>();
        pb.DestroyPhysical();
        Liquid.MonoLiquidify(puddle, monoLiquid.liquid);
        Edible edible = GetComponent<Edible>();
        if (edible) {
            Edible puddleEdible = puddle.GetComponent<Edible>();
            puddleEdible.human = edible.human;
        }
    }
    void OnCollisionEnter2D(Collision2D coll) {
        if (physical.currentMode == Physical.mode.fly) {
            Debug.Log("droplet fly collision");
        }
    }
}
