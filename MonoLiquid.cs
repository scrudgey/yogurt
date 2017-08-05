using UnityEngine;
public class MonoLiquid : MonoBehaviour {
	public Liquid liquid;
	public Edible edible;
	public PhysicalBootstrapper physB;
	static GameObject CreateStain(GameObject target, Vector3 location){
		SpriteMask parentMask = Toolbox.Instance.GetOrCreateComponent<SpriteMask>(target);
		SpriteRenderer parentRenderer = target.GetComponent<SpriteRenderer>();
		GameObject stainObject = Instantiate(Resources.Load("stain")) as GameObject;
		Stain stain = stainObject.GetComponent<Stain>();
		stain.parentMask = parentMask;
		stain.parentRenderer = parentRenderer;
		stainObject.transform.position = location;
		stainObject.transform.SetParent(target.transform, true);
		SpriteRenderer stainRenderer = stainObject.GetComponent<SpriteRenderer>();
		stainRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
		return stainObject;
	}
	void Awake(){
		edible = GetComponent<Edible>();
		if (!edible)
			edible = gameObject.AddComponent<Edible>();
		physB = GetComponent<PhysicalBootstrapper>();
	}
	 void Start () {
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer && liquid != null){
			spriteRenderer.color = liquid.color;
		}
	}
	//TODO: figure out why splashes are happening instead of puddles for vomit
	void OnCollisionEnter2D(Collision2D coll){
		if (physB == null)
			return;
		if (physB.physical == null)
			return;
		if (physB.physical.currentMode == Physical.mode.fly){
			GameObject splash = Instantiate(Resources.Load("prefabs/splash"), transform.position, Quaternion.identity) as GameObject;
			SpriteRenderer splashSprite = splash.GetComponent<SpriteRenderer>();
			splashSprite.color = liquid.color;
			Transform rectTransform = splash.GetComponent<Transform>();
 			rectTransform.Rotate(new Vector3(0, 0, Random.Range(0f, 360f)));

			GameObject stain = CreateStain(coll.gameObject, transform.position);
			SpriteRenderer stainRenderer = stain.GetComponent<SpriteRenderer>();
			stainRenderer.color = liquid.color;
			LiquidCollection.MonoLiquidify(stain, liquid);
			Destroy(gameObject);
		}
	}
	public void LoadLiquid(string type){
		// TODO: make this fancier: retrieve liquid in buffer, only change variable
		// if liquid was returned
		liquid = LiquidCollection.LoadLiquid(type);
		// add things here for changing edible properties according to liquid properties
	}
	void OnGroundImpact(Physical phys){
		Toolbox.Instance.DataFlag(gameObject, 1f, 1f, 0f, 0f, 0f);
		GameObject puddle = Instantiate(Resources.Load("Puddle"), transform.position, Quaternion.identity) as GameObject;
		puddle.layer = 9;
		PhysicalBootstrapper pb = GetComponent<PhysicalBootstrapper>();
		pb.DestroyPhysical();
		LiquidCollection.MonoLiquidify(puddle, liquid);
		puddle.GetComponent<Edible>().offal = true;
		Destroy(gameObject);
	}
}
