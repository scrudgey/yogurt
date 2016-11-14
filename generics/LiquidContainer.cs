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
    private float spillSeverity;
	public string initLiquid;
    public string containerName;
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
		Interaction fillContainer = new Interaction(this, "Fill", "FillFromContainer");
		Interaction drinker = new Interaction(this, "Drink", "Drink");
		drinker.validationFunction = true;
		interactions.Add(drinker);
		fillContainer.validationFunction = true;
		interactions.Add(fillContainer);
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
	public string FillFromReservoir_desc(LiquidResevoir l){
		string myname = Toolbox.Instance.GetName(gameObject);
		string resname = Toolbox.Instance.GetName(l.gameObject);
		return "Fill "+myname+" with "+l.liquid.name+" from "+resname;
	}
	public void FillFromContainer(LiquidContainer l){
		if(l.amount > 0){
			FillWithLiquid(l.liquid);
			float fill = Mathf.Min(l.amount, fillCapacity);
			l.amount -= fill;
			amount = fill;
		}
	}
	public bool FillFromContainer_Validation(LiquidContainer l){
		if (l.amount > 0){
			return true;
		} else {
			return false;
		}
	}
	public string FillFromContainer_desc(LiquidContainer l){
		string myname = Toolbox.Instance.GetName(gameObject);
		string resname = Toolbox.Instance.GetName(l.gameObject);
		return "Fill "+myname+" with "+l.liquid.name+" from "+resname;
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
	    liquid = LiquidCollection.LoadLiquid(type);
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
			// TODO: only update actions if i'm being held : message
			// UINew.Instance.UpdateActionButtons();
		}
		if (!empty && amount <= 0){
			empty = true;
		}
	}

    public void Spill(){
        Spill(0.2f);
    }
	public void Spill(float severity){
		doSpill = true;
        spillSeverity = severity;
	}

	void FixedUpdate(){
		if (doSpill){
			doSpill = false;
			if (amount > 0 && spillTimeout <= 0){
                Toolbox.Instance.SpawnDroplet(liquid, spillSeverity, gameObject);
				amount -= 0.25f;
				spillTimeout = 0.075f;
			}
		}
	}
	public void Drink(Eater eater){
		if (eater){
			GameObject sip = new GameObject();
			sip.AddComponent<MonoLiquid>();
			LiquidCollection.MonoLiquidify(sip, liquid);
			eater.Eat(sip.GetComponent<Edible>());
			amount -= 1f;
		}
	}
	public bool Drink_Validation(Eater eater){
		return amount > 0;
	}
	public string Drink_desc(Eater eater){
		string myname = Toolbox.Instance.GetName(gameObject);
		return "Drink "+liquid.name+" from "+myname;
	}
	void OnGroundImpact(Physical phys){
		if (!lid)
			Spill();
	}

}
