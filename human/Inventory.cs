﻿using UnityEngine;
// using System.Collections;
using System.Collections.Generic;

public class Inventory : Interactive, IExcludable {
	public List<GameObject> items;
	public float strength;
	public Pickup holding{
		get {return _holding;}
		set {
			if (value != null){
				MonoBehaviour[] list = value.gameObject.GetComponents<MonoBehaviour>();
				foreach(MonoBehaviour mb in list)
				{
					if (mb is IDirectable){
						IDirectable directable = (IDirectable)mb;
						if (value == null)
							controllable.directable = null;
						if (value != null)
							controllable.directable = directable;
					}
				}
				if (animator != null){
					animator.Holding(true);
				}
			} else {
				if (animator != null){
					animator.Holding(false);
				}
			}
			_holding = value;
			UINew.Instance.InventoryCallback(this);
			// GameManager.Instance.CheckItemCollection(this, gameObject);
			if (value != null)
				GameManager.Instance.CheckItemCollection(value.gameObject, gameObject);
		}
	}
	private Pickup _holding;
	private Transform holdpoint;
	public GameObject slasher;
	private string slashFlag;
	private Controllable controllable;
	private List<Interaction> manualActionDictionary;
	private bool LoadInitialized = false;
	private GameObject throwObject;
	private float dropHeight = 0.20f;
	private AdvancedAnimation animator;

	public bool fightMode;
	
	void Start(){
		if (!LoadInitialized)
			LoadInit();
	}

	public void LoadInit(){
		controllable = GetComponent<Controllable>();
		animator = GetComponent<AdvancedAnimation>();
		holdpoint = transform.Find("holdpoint");
		Interaction getAction = new Interaction(this, "Get", "GetItem", true, false);
		getAction.dontWipeInterface = false;
		interactions.Add(getAction);

		Interaction swingAction = new Interaction(this, "Swing", "SwingItem", false,true);
		swingAction.defaultPriority = 5;
		interactions.Add(swingAction);
		LoadInitialized = true;
	}

	public void GetItem(Pickup pickup){
		//first check to see if we're already holding it.
		if (holding == pickup){
			StashItem(holding.gameObject);
		}else{
			// strength check to see if we can pick it up
			if (strength < pickup.weight){
				this.SendMessage("Say", "It's too heavy!");
			} else {
				if (holding){
					DropItem();
				}
				//make the object the current holding.
				Messenger.Instance.ClaimObject(pickup.gameObject,this);
				holding = pickup;
				PhysicalBootstrapper phys = holding.GetComponent<PhysicalBootstrapper>();
				if (phys)
					phys.DestroyPhysical();
				OrderByY yorder = holding.GetComponent<OrderByY>();
				if (yorder)
					yorder.enabled = false;
				holding.transform.position = holdpoint.position;
				holdpoint.localScale = Vector3.one;
				transform.localScale = Vector3.one;
				holding.transform.parent = holdpoint;
				holding.transform.rotation = Quaternion.identity;
				holding.GetComponent<Rigidbody2D>().isKinematic = true;
				holding.GetComponent<Collider2D>().isTrigger = true;
				if (holding.pickupSounds.Length > 0)
					GetComponent<AudioSource>().PlayOneShot(holding.pickupSounds[Random.Range(0, holding.pickupSounds.Length)]);
			}
		}
	}
	public string GetItem_desc(Pickup pickup){
		string itemname = Toolbox.Instance.GetName(pickup.gameObject);
		return "Pick up "+itemname;
	}
	
	public void StashItem(GameObject item){
		items.Add(item);
		if (item == holding.gameObject)
			SoftDropItem();
		item.SetActive(false);
	}

	public void SoftDropItem(){
		Messenger.Instance.DisclaimObject(holding.gameObject,this);
		PhysicalBootstrapper phys = holding.GetComponent<PhysicalBootstrapper>();
		if (phys)	
			phys.doInit = false;
		OrderByY yorder = holding.GetComponent<OrderByY>();
		if (yorder)
			yorder.enabled = true;
		holding.GetComponent<Rigidbody2D>().isKinematic = false;
		holding.GetComponent<Collider2D>().isTrigger = false;
		holding = null;
	}

	public void DropItem(){
		Messenger.Instance.DisclaimObject(holding.gameObject,this);
		holding.GetComponent<Rigidbody2D>().isKinematic = false;
		holding.GetComponent<Collider2D>().isTrigger = false;
		PhysicalBootstrapper phys = holding.GetComponent<PhysicalBootstrapper>();
		if (phys){
			Vector2 initV = Vector2.ClampMagnitude(controllable.direction, 0.5f);
			initV = initV + GetComponent<Rigidbody2D>().velocity;
			float vx = initV.x;
			float vy = initV.y / 2;
			float vz = 0.5f;
			phys.InitPhysical(dropHeight, new Vector3(vx, vy, vz));
		} else {
			holding.transform.parent = null;
		}
		OrderByY yorder = holding.GetComponent<OrderByY>();
		if (yorder)
			yorder.enabled = true;
		SpriteRenderer sprite = holding.GetComponent<SpriteRenderer>();
		sprite.sortingLayerName = "main";
		holding = null;
	}

	public void RetrieveItem(string itemName){
		for(int i=0; i < items.Count; i++){
			//retrieve the item from the items
			if(items[i].GetComponent<Item>().itemName == itemName){
				if (holding)
					DropItem();
				OrderByY yorder = items[i].GetComponent<OrderByY>();
				if (yorder)
					yorder.enabled = false;
				items[i].SetActive(true);
				items[i].transform.position = holdpoint.position;
				items[i].transform.parent = holdpoint;
				items[i].GetComponent<Rigidbody2D>().isKinematic = true;
				items[i].GetComponent<Collider2D>().isTrigger = true;
				Messenger.Instance.ClaimObject(items[i].gameObject, this);
				holding = items[i].GetComponent<Pickup>();
				items.RemoveAt(i);
			}
		}
	}

	public void ThrowItem(){
		// set up the held object to be thrown on the next fixed update
		if (animator){
			animator.Throwing(true);
		}
	}
	
	public void ActivateThrow(){
		if (holding){
			throwObject = holding.gameObject;
			OrderByY yorder = holding.GetComponent<OrderByY>();
			if (yorder)
				yorder.enabled = false;
			holding = null;
		}
	}

	private void DoThrow(){
		Messenger.Instance.DisclaimObject(throwObject, this);
		PhysicalBootstrapper phys = throwObject.GetComponent<PhysicalBootstrapper>();
		if (phys){
			Rigidbody2D myBody = GetComponent<Rigidbody2D>();
			phys.doInit = false;
			phys.initHeight = 0f;
			phys.InitPhysical(dropHeight, Vector2.zero);
			SpriteRenderer sprite = throwObject.GetComponent<SpriteRenderer>();
			sprite.sortingLayerName = "main";
			// these two commands have nothing to do with physical -- they're part of the drop code that sets physics
			// back to normal.
			throwObject.GetComponent<Rigidbody2D>().isKinematic = false;
			throwObject.GetComponent<Collider2D>().isTrigger = false;
			phys.physical.StartZipMode();
			float vx = controllable.direction.x * 2.5f;
			float vy = controllable.direction.y * 2.5f;
			float vz = 0f;
			if (myBody){
				vx = vx + myBody.velocity.x;
				vy = vy + myBody.velocity.y;
			}
			phys.Set3MotionImmediate(new Vector3(vx, vy, vz));
			foreach(Collider2D collider in GetComponentsInChildren<Collider2D>()){
				if (!collider.isTrigger){
					Physics2D.IgnoreCollision(collider, phys.physical.objectCollider);
					Physics2D.IgnoreCollision(collider, phys.physical.groundCollider);
				}
			}
		}
		throwObject = null;
		if (animator){
			animator.Throwing(false);
		}
		// Resources.Load("sounds/8bit_impact1", typeof(AudioClip)) as AudioClip;
		GetComponent<AudioSource>().PlayOneShot(Resources.Load("sounds/8bit_throw", typeof(AudioClip)) as AudioClip);
		Toolbox.Instance.DataFlag(gameObject, 50, 0, 0, 0, 0);
	}

	void FixedUpdate(){
		// do the throwing action in fixed update if we have gotten the command to throw
		if (throwObject){
			DoThrow();
		}
	}

	void Update(){
		if (holding ){
			if( controllable.directionAngle > 45 && controllable.directionAngle < 135){
				holding.GetComponent<Renderer>().sortingOrder = GetComponent<Renderer>().sortingOrder - 1;
			} else {
				holding.GetComponent<Renderer>().sortingOrder = GetComponent<Renderer>().sortingOrder + 2;
			}
			holding.transform.position = holdpoint.transform.position;
		}
	}

	public void SwingItem(MeleeWeapon weapon){
		if (animator)
			animator.Swinging(true);
		if (weapon.swingSounds.Length > 0){
			GetComponent<AudioSource>().PlayOneShot(weapon.swingSounds[Random.Range(0, weapon.swingSounds.Length)]);
		}
		slashFlag = "right";
		if (controllable.directionAngle > 45 && controllable.directionAngle < 135){
			slashFlag = "up";
		}
		if (controllable.directionAngle > 225 && controllable.directionAngle < 315){
			slashFlag = "down";
		}
	}
	public string SwingItem_desc(MeleeWeapon weapon){
		string weaponname = Toolbox.Instance.GetName(weapon.gameObject);
		return "Swing "+weaponname;
	}

	void EndSwing(){
		if (animator)
			animator.Swinging(false);
		holding.GetComponent<Renderer>().sortingLayerName="main";
		holding.GetComponent<Renderer>().sortingOrder = GetComponent<Renderer>().sortingOrder - 1;
	}

	void StartSwing(){
		GameObject slash = Instantiate(Resources.Load("Slash2"), transform.position, transform.rotation) as GameObject;
		slash.transform.parent = transform;
		slash.transform.localScale = Vector3.one;
		holding.GetComponent<Renderer>().sortingLayerName = "main";
		holding.GetComponent<Renderer>().sortingOrder = GetComponent<Renderer>().sortingOrder + 1;
		slash.GetComponent<Animator>().SetBool(slashFlag, true);
		Collider2D slashCollider = slash.GetComponent<Collider2D>();
		Physics2D.IgnoreCollision(holding.GetComponent<Collider2D>(), slashCollider, true);
		Slasher s = slash.GetComponent<Slasher>();
		s.impactSounds = holding.GetComponent<MeleeWeapon>().impactSounds;
		s.direction = controllable.direction;
	}

	public void DropMessage(GameObject obj){
		SoftDropItem();
	}

	public void WasDestroyed(GameObject obj){
		if (obj == holding.gameObject){
			holding = null;
		}
	}

	public void ToggleFightMode(){
		fightMode = !fightMode;
		if (fightMode){
			if (holding){
				DropItem();
			}
			if (animator){
				animator.Fighting(true);
			}
		} else {
			if (animator){
				animator.Fighting(false);
			}
		}
	}

	public void StartPunch(){
		if (animator)
			animator.Punching(true);
	}
	public void PunchImpact(){
		GameObject slash = Instantiate(Resources.Load("PhysicalImpact"), holdpoint.position, holdpoint.rotation) as GameObject;
		PhysicalImpact impact = slash.GetComponent<PhysicalImpact>();
		impact.direction = controllable.direction;
		Collider2D slashCollider = slash.GetComponent<Collider2D>();
		foreach (Collider2D tomCollider in GetComponentsInChildren<Collider2D>()){
			Physics2D.IgnoreCollision(tomCollider, slashCollider, true);
		}
	}
	public void EndPunch(){
		if (animator)
			animator.Punching(false);
	}

}
