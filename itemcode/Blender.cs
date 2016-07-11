﻿using UnityEngine;
// using System.Collections;

public class Blender : Container {
//	public bool lidOn;
	public bool power;
	private bool vibrate;
	public Sprite[] spriteSheet;
	private SpriteRenderer spriteRenderer;
	public AudioClip blendStart;
	public AudioClip blendNoise;
	public AudioClip blendStop;
	private LiquidContainer liquidContainer;
	private bool LoadInitialized = false;
	private AudioSource audioSource;
	protected override void Start () {
		base.Start();
		if (!LoadInitialized)
			LoadInit();
	}
	public void LoadInit(){
		spriteRenderer = GetComponent<SpriteRenderer>();
		liquidContainer = GetComponent<LiquidContainer>();
		interactions.Add( new Interaction(this, "Power", "Power"));
		interactions.Add( new Interaction(this, "Lid", "Lid"));
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
		LoadInitialized = true;
	}
	void Update () {

		if(power){
			if(!audioSource.isPlaying){
				audioSource.clip = blendNoise;
				audioSource.Play();
			}
			//vibrate the blender
			Vector3 pos = transform.position;
			vibrate = !vibrate;
			if(vibrate){
				pos.y = pos.y + 0.01f;
			} else{
				pos.y = pos.y - 0.01f;
			}
			transform.position = pos;
			//blend contained item
			if(items.Count > 0){
				Destructible d = items[0].GetComponent<Destructible>();
				Edible edible = items[0].GetComponent<Edible>();
				if (edible && edible.blendable){
					liquidContainer.FillWithLiquid(edible.Liquify());
					RemoveRetrieveAction(items[0]);
					Destroy(items[0].gameObject);
					items.RemoveAt(0);
				}
				if (d){
					d.TakeDamage(Destructible.damageType.physical, Time.deltaTime * 10);
				}
			}
			if(liquidContainer.amount > 0 && !liquidContainer.lid){
				liquidContainer.Spill(1f);
			}
		}
	}
	public void Power(){
		power = !power;
		if (power){
			GetComponent<AudioSource>().clip = blendStart;
			GetComponent<AudioSource>().Play();
		} else{
			GetComponent<AudioSource>().clip = blendStop;
			GetComponent<AudioSource>().Play();
		}
	}
	public string Power_desc(){
		if (power){
			return "Turn off power";
		} else {
			return "Turn on power";
		}
	}

	public void Lid(){
		liquidContainer.lid = !liquidContainer.lid;
		if(liquidContainer.lid){
			spriteRenderer.sprite = spriteSheet[0];
		}else{
			spriteRenderer.sprite = spriteSheet[1];
		}
	}
	public string Lid_desc(){
		if (liquidContainer.lid){
			return "Remove lid";
		} else {
			return "Put on lid";
		}
	}
	public override void Store(Inventory inv){
		if (liquidContainer.lid){
			inv.gameObject.SendMessage("Say", "The lid is on!");
		} else {
			base.Store (inv);
		}
	}
}
