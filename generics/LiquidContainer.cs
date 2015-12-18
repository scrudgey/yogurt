using UnityEngine;
// using System.Collections;

public class LiquidContainer : Interactive {
//	public static AudioClip pourSound;
	private SpriteRenderer liquidSprite;
	public Liquid liquid;
	private float _amount;
	public float amount{
		get { 
			return _amount;
		}
		set {
			_amount = value;
			CheckLiquid();
		}
	}
	public float fillCapacity;
	private float spillTimeout = 0.075f;
	private bool empty;
	public bool lid;
	private bool LoadInitialized = false;
	private bool doSpill = false;
	public string initLiquid;

	void Update(){
		if (spillTimeout > 0){
			spillTimeout -= Time.deltaTime;
		}
		if (Vector3.Dot(transform.up, Vector3.up) < 0.05 && spillTimeout <= 0 && !lid){
			Spill();
		}
	}

	void Start () {
		interactions.Add(new Interaction(this, "Fill", "FillFromReservoir"));
		interactions.Add(new Interaction(this, "Fill", "FillFromContainer"));
		empty = true;
		if (!LoadInitialized)
			LoadInit();
	}

	public void LoadInit(){
		Transform child = transform.FindChild("liquidSprite");
		if (child){
			liquidSprite = child.GetComponent<SpriteRenderer>();
			if (liquidSprite) 
				liquidSprite.enabled = false;
		}
		if (initLiquid != ""){
			FillByLoad(initLiquid);
		}
		LoadInitialized = true;
	}

	public void FillFromReservoir(LiquidResevoir l){
		FillWithLiquid(l.liquid);
	}

	public void FillFromContainer(LiquidContainer l){
		if( l.amount > 0){
			FillWithLiquid(l.liquid);
			float fill = Mathf.Min(l.amount, fillCapacity);
			l.amount -= fill;
			amount = fill;
		}
	}

	public void FillWithLiquid(Liquid l){
		if (amount > 0){
			l = Liquid.MixLiquids(liquid, l);
		}
		liquid = l;
		amount = fillCapacity;
		CheckLiquid();
	}

	public void FillByLoad(string type){
		Liquid l = LiquidCollection.LoadLiquid(type);
		liquid = l;
		amount = fillCapacity;
		CheckLiquid();
	}

	private void CheckLiquid(){
		if (liquid != null && amount > 0 && liquidSprite != null){

			if (liquidSprite != null){
				liquidSprite.enabled = true;
				liquidSprite.color = liquid.color;
			}

		}
		if (amount <= 0 ){
			if (liquidSprite)
				liquidSprite.enabled = false;
		}
		if (empty && amount > 0){
			empty = false;
			interactions.Add( new Interaction(this, "Drink", "Drink"));
			SendMessageUpwards("UpdateActions", SendMessageOptions.DontRequireReceiver);
		}
		if (!empty && amount <= 0){
			empty = true;
			Interaction removeThis = null;
			foreach (Interaction interaction in interactions)
				if (interaction.actionName == "Drink")
					removeThis = interaction;
			if (removeThis != null){
				interactions.Remove(removeThis);
				SendMessageUpwards("UpdateActions", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void Spill(){
		doSpill = true;
	}

	void FixedUpdate(){
		if (doSpill){
			doSpill = false;
			if (amount > 0 && spillTimeout <= 0){
				Vector3 initialVelocity = Vector2.zero;
				Vector3 randomVelocity = Vector2.zero;
				randomVelocity = transform.right * Random.Range(-0.2f, 0.2f);
				initialVelocity.x = transform.up.x * Random.Range(0.8f, 1.3f);
				// initialVelocity.z = Random.Range(0.2f, 0.4f);
				initialVelocity.z = Random.Range(1.0f, 1.2f);
				// Rigidbody2D parentBody = GetComponent<Rigidbody2D>();
				// if (parentBody){
				// 	initialVelocity.x += parentBody.velocity.x;
				// 	initialVelocity.y += parentBody.velocity.y;
				// }
				initialVelocity.x += randomVelocity.x;
				initialVelocity.z += randomVelocity.y;
				GameObject droplet = Instantiate(Resources.Load("droplet"), transform.position, Quaternion.identity) as GameObject;
				PhysicalBootstrapper phys = droplet.GetComponent<PhysicalBootstrapper>();
				Vector2 initpos = droplet.transform.position;
				float initHeight = 0.01f;
				phys.ignoreCollisions = true;
				Physical pb = GetComponentInParent<Physical>();
				if (pb != null){ 
					initHeight = pb.height; 
				}
				initpos.y += initHeight;
				droplet.transform.position = initpos;
				phys.doInit = false;
				phys.Start();
				phys.InitPhysical(initHeight, initialVelocity);
				Physics2D.IgnoreCollision(GetComponent<Collider2D>(), droplet.GetComponent<Collider2D>(), true);
				LiquidCollection.MonoLiquidify(droplet, liquid);
				amount -= 0.25f;
				spillTimeout = 0.075f;
			}
		}
	}

	public void Drink(Eater eater){

		if (eater){
			GameObject sip = new GameObject();
			sip.AddComponent<MonoLiquid>();
			LiquidCollection.MonoLiquidify(sip,liquid);
			eater.Eat( sip.GetComponent<Edible>() );
			amount -= 1f;
		}
	}
	void OnGroundImpact(Physical phys){
		if (!lid)
			Spill();
	}

}
