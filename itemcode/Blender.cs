﻿using UnityEngine;

public class Blender : Container, ISaveable {
	public bool power;
	private bool vibrate;
	public Sprite[] spriteSheet;
	private SpriteRenderer spriteRenderer;
	public AudioClip blendStart;
	public AudioClip blendNoise;
	public AudioClip blendStop;
	public AudioClip lidOn;
	public AudioClip lidOff;
	private LiquidContainer liquidContainer;
	private AudioSource audioSource;
	protected override void Awake(){
		base.Awake();
		spriteRenderer = GetComponent<SpriteRenderer>();
		liquidContainer = GetComponent<LiquidContainer>();
		interactions.Add(new Interaction(this, "Power", "Power"));
		interactions.Add(new Interaction(this, "Lid", "Lid"));
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
	}
	void Update (){
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
				MessageDamage message = new MessageDamage();
				message.amount = Time.deltaTime * 10;
				message.force = Vector2.zero;
				message.type = damageType.physical;
				Toolbox.Instance.SendMessage(items[0].gameObject, this, message, false);
				
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
			audioSource.PlayOneShot(lidOn);
		}else{
			spriteRenderer.sprite = spriteSheet[1];
			audioSource.PlayOneShot(lidOff);
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
			Toolbox.Instance.SendMessage(inv.gameObject, this, new MessageSpeech("The lid is on!"));
		} else {
			base.Store(inv);
		}
	}
	public override void WasDestroyed(GameObject obj){
		base.WasDestroyed(obj);
		Edible edible = obj.GetComponent<Edible>();
		if (edible && edible.blendable){
			liquidContainer.FillWithLiquid(edible.Liquify());
		}
	}
	public override void SaveData(PersistentComponent data){
		base.SaveData(data);
		data.bools["power"] = power;
		data.bools["lid"] = liquidContainer.lid;
	}
	public override void LoadData(PersistentComponent data){
		base.LoadData(data);
		power = data.bools["power"];
		if (data.bools["lid"]){
			spriteRenderer.sprite = spriteSheet[0];
		}else{
			spriteRenderer.sprite = spriteSheet[1];
		}
	}
}
