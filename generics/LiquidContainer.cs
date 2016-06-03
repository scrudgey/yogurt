﻿using UnityEngine;
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
		if(l.amount > 0){
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
	void OnGroundImpact(Physical phys){
		if (!lid)
			Spill();
	}

}
