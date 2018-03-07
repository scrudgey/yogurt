using UnityEngine;
public class MonoLiquid : MonoBehaviour {
    public Liquid liquid;
    public Edible edible;
    public PhysicalBootstrapper physB;
    public bool ignoreCollisions;
    static GameObject CreateStain(GameObject target, Vector3 location) {
        GameObject stainObject = Instantiate(Resources.Load("prefabs/stain")) as GameObject;
        stainObject.transform.position = location;
        Stain stain = stainObject.GetComponent<Stain>();
        stain.ConfigureParentObject(target);
        return stainObject;
    }
    void Awake() {
        edible = GetComponent<Edible>();
        if (!edible)
            edible = gameObject.AddComponent<Edible>();
        physB = GetComponent<PhysicalBootstrapper>();
    }
    void Start() {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer && liquid != null) {
            spriteRenderer.color = liquid.color;
        }
    }
    //TODO: figure out why splashes are happening instead of puddles for vomit
    void OnCollisionEnter2D(Collision2D coll) {
        if (ignoreCollisions)
            return;
        if (physB == null)
            return;
        if (physB.physical == null)
            return;
        if (coll.collider.gameObject.layer == LayerMask.NameToLayer("main"))
            if (physB.physical.currentMode == Physical.mode.fly) {
                GameObject splash = Instantiate(Resources.Load("prefabs/splash"), transform.position, Quaternion.identity) as GameObject;
                SpriteRenderer splashSprite = splash.GetComponent<SpriteRenderer>();
                splashSprite.color = liquid.color;
                Transform rectTransform = splash.GetComponent<Transform>();
                rectTransform.Rotate(new Vector3(0, 0, Random.Range(0f, 360f)));

                GameObject stain = CreateStain(coll.gameObject, transform.position);
                SpriteRenderer stainRenderer = stain.GetComponent<SpriteRenderer>();
                stainRenderer.color = liquid.color;
                Liquid.MonoLiquidify(stain, liquid);
                ClaimsManager.Instance.WasDestroyed(gameObject);
                Destroy(gameObject);
            }
    }
    public void LoadLiquid(string type) {
        // TODO: check for null return
        liquid = Liquid.LoadLiquid(type);
        // add things here for changing edible properties according to liquid properties (?)
    }
    void GroundModeStart(Physical phys) {
        EventData data = Toolbox.Instance.DataFlag(gameObject, chaos: 1, disgusting: 1);
        data.whatHappened = liquid.name + " was spilled";
        data.noun = "spilling";

        GameObject puddle = Instantiate(Resources.Load("Puddle"), transform.position, Quaternion.identity) as GameObject;
        puddle.layer = 9;
        PhysicalBootstrapper pb = GetComponent<PhysicalBootstrapper>();
        pb.DestroyPhysical();
        Liquid.MonoLiquidify(puddle, liquid);
        Edible puddleEdible = puddle.GetComponent<Edible>();
        Edible edible = GetComponent<Edible>();
        if (edible) {
            puddleEdible.human = edible.human;
            puddleEdible.offal = true;
        }
        Destroy(gameObject);
        ClaimsManager.Instance.WasDestroyed(gameObject);
    }
}
