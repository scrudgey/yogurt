﻿using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class Inventory : Interactive, IExcludable, IMessagable, IDirectable, ISaveable {
	public List<GameObject> items;
	public GameObject initHolding;
	public bool strong;
	public Pickup holding{
		get {return _holding;}
		set {
			MessageAnimation anim = new MessageAnimation();
			MessageInventoryChanged invMessage = new MessageInventoryChanged();

			anim.type = MessageAnimation.AnimType.holding;
			anim.value = value != null;
				Toolbox.Instance.SendMessage(gameObject, this, anim);
			if (value == null){
				invMessage.dropped = _holding.gameObject;
			}
			_holding = value;
			if (value != null)
				invMessage.holding = value.gameObject;
			Toolbox.Instance.SendMessage(gameObject, this, invMessage);
			if (value != null)
				GameManager.Instance.CheckItemCollection(value.gameObject, gameObject);
			UpdateActions();
		}
	}
	private Pickup _holding;
	private Transform holdpoint;
	public float holdpoint_angle;
	public GameObject slasher;
	private string slashFlag;
	private List<Interaction> manualActionDictionary;
	private GameObject throwObject;
	public float dropHeight = 0.18f;
	public Vector2 direction = Vector2.right;
	private float directionAngle;
	private SortingGroup holdSortGroup;
	// private GameObject strengthFX;
	public bool normalPunchSounds = true;
	public List<AudioClip> punchSounds;
	private AudioSource audioSource;
	void Awake(){
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
		holdpoint = transform.Find("holdpoint");
		holdSortGroup = holdpoint.GetComponent<SortingGroup>();
		Interaction getAction = new Interaction(this, "Get", "GetItem", true, false);
		getAction.dontWipeInterface = false;
		getAction.otherOnPlayerConsent = false;
		getAction.playerOnOtherConsent = false;
		// getAction.hideInManualActions = true;
		interactions.Add(getAction);
		Interaction swingAction = new Interaction(this, "Swing", "SwingItem", false, true);
		swingAction.defaultPriority = 5;
		interactions.Add(swingAction);
		direction = Vector2.right;
		if (normalPunchSounds){
			AudioClip[] punches = Resources.LoadAll<AudioClip>("sounds/swoosh/");
			punchSounds = new List<AudioClip>(punches);
		}
	}
	public void Start(){
		if (initHolding){
			if (initHolding.activeInHierarchy){
				Pickup pickup = initHolding.GetComponent<Pickup>();
				GetItem(pickup);
			} else {
				GameObject instance = Instantiate(initHolding) as GameObject;
				Pickup pickup = instance.GetComponent<Pickup>();
				GetItem(pickup);
			}
		}
	}
	public void DirectionChange(Vector2 dir){
		direction = dir;
		directionAngle = Toolbox.Instance.ProperAngle(direction.x, direction.y);
		if (Toolbox.Instance.DirectionToString(dir) == "up"){
			holdSortGroup.sortingOrder = -1000;
		} else {
			holdSortGroup.sortingOrder = 1000;
		}
	}
	public void GetItem(Pickup pickup){
		//first check to see if we're already holding it.
		if (holding == pickup){
			StashItem(holding.gameObject);
		}else{
			// strength check to see if we can pick it up
			if (pickup.heavyObject && !strong){
				MessageSpeech message = new MessageSpeech("It's too heavy!");
				Toolbox.Instance.SendMessage(gameObject, this, message);
				return;
			} 
			if (holding){
				DropItem();
			}
			//make the object the current holding.
			ClaimsManager.Instance.ClaimObject(pickup.gameObject,this);
			holding = pickup;
			PhysicalBootstrapper phys = holding.GetComponent<PhysicalBootstrapper>();
			if (phys)
				phys.DestroyPhysical();
			holding.transform.position = holdpoint.position;
			holding.transform.SetParent(holdpoint, false);
			holding.transform.rotation = Quaternion.identity;
			holding.GetComponent<Rigidbody2D>().isKinematic = true;
			holding.GetComponent<Collider2D>().isTrigger = true;
			holding.holder = this;
			if (holding.pickupSounds.Length > 0)
				GetComponent<AudioSource>().PlayOneShot(holding.pickupSounds[Random.Range(0, holding.pickupSounds.Length)]);
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
	public bool PlaceItem(Vector2 place){
		ClaimsManager.Instance.DisclaimObject(holding.gameObject, this);
		holding.GetComponent<Rigidbody2D>().isKinematic = false;
		holding.GetComponent<Collider2D>().isTrigger = false;
		holding.transform.SetParent(null);
		holding.transform.position = place;
		PhysicalBootstrapper phys = holding.GetComponent<PhysicalBootstrapper>();
		if (phys){
			phys.InitPhysical(0.01f, Vector3.zero);
		}
		SpriteRenderer sprite = holding.GetComponent<SpriteRenderer>();
		sprite.sortingLayerName = "main";
		holding = null;
		return true;
	}
	public void SoftDropItem(){
		if (holding == null)
			return;
		ClaimsManager.Instance.DisclaimObject(holding.gameObject, this);
		holding.holder = null;
		PhysicalBootstrapper phys = holding.GetComponent<PhysicalBootstrapper>();
		if (phys)	
			phys.doInit = false;
		holding.GetComponent<Rigidbody2D>().isKinematic = false;
		holding.GetComponent<Collider2D>().isTrigger = false;
		holding = null;
	}
	public void DropItem(){
		if (holding == null)
			return;
		ClaimsManager.Instance.DisclaimObject(holding.gameObject, this);
		holding.holder = null;
		holding.GetComponent<Rigidbody2D>().isKinematic = false;
		holding.GetComponent<Collider2D>().isTrigger = false;
		PhysicalBootstrapper phys = holding.GetComponent<PhysicalBootstrapper>();
		if (phys){
			Vector2 initV = Vector2.ClampMagnitude(direction, 0.1f);
			initV = initV + GetComponent<Rigidbody2D>().velocity;
			float vx = initV.x;
			float vy = initV.y / 2;
			float vz = 0.1f;
			phys.InitPhysical(dropHeight, new Vector3(vx, vy, vz));
		} else {
			holding.transform.SetParent(null);
		}
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
				items[i].SetActive(true);
				items[i].transform.position = holdpoint.position;
				items[i].transform.SetParent(holdpoint);
				items[i].GetComponent<Rigidbody2D>().isKinematic = true;
				items[i].GetComponent<Collider2D>().isTrigger = true;
				ClaimsManager.Instance.ClaimObject(items[i].gameObject, this);
				holding = items[i].GetComponent<Pickup>();
				items.RemoveAt(i);
			}
		}
	}
	public void ThrowItem(){
		// set up the held object to be thrown on the next fixed update
		MessageAnimation anim = new MessageAnimation(MessageAnimation.AnimType.throwing, true);
		Toolbox.Instance.SendMessage(gameObject, this, anim);
	}
	public void ActivateThrow(){
		if (holding){
			throwObject = holding.gameObject;
			holding = null;
		}
	}
	private void DoThrow(){
		ClaimsManager.Instance.DisclaimObject(throwObject, this);
		PhysicalBootstrapper phys = throwObject.GetComponent<PhysicalBootstrapper>();
		if (phys){
			phys.thrownBy = gameObject;
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
			float vx = direction.x * 2.5f;
			float vy = direction.y * 2.5f;
			float vz = 0f;
			if (myBody){
				vx = vx + myBody.velocity.x;
				vy = vy + myBody.velocity.y;
			}
			phys.Set3MotionImmediate(new Vector3(vx, vy, vz));
			foreach(Collider2D collider in GetComponentsInChildren<Collider2D>()){
				if (!collider.isTrigger){
					Physics2D.IgnoreCollision(collider, phys.physical.objectCollider);
					Physics2D.IgnoreCollision(collider, phys.physical.horizonCollider);
					phys.physical.temporaryDisabledColliders.Add(collider);
				}
			}
		}
		MessageAnimation anim = new MessageAnimation(MessageAnimation.AnimType.throwing, false);
		Toolbox.Instance.SendMessage(gameObject, this, anim);
		GetComponent<AudioSource>().PlayOneShot(Resources.Load("sounds/8bit_throw", typeof(AudioClip)) as AudioClip);
		EventData data = Toolbox.Instance.DataFlag(gameObject, chaos:1);
		data.noun = "throwing";
		data.whatHappened = Toolbox.Instance.GetName(gameObject)+" threw a "+ Toolbox.Instance.GetName(throwObject);
		throwObject = null;
	}
	void FixedUpdate(){
		// do the throwing action in fixed update if we have gotten the command to throw
		if (throwObject){
			DoThrow();
		}
	}
	void Update(){
		if (holding){
			if(directionAngle > 45 && directionAngle < 135){
				holding.GetComponent<Renderer>().sortingOrder = GetComponent<Renderer>().sortingOrder - 1;
			} else {
				holding.GetComponent<Renderer>().sortingOrder = GetComponent<Renderer>().sortingOrder + 2;
			}
			holding.transform.position = holdpoint.transform.position;
			if (holdpoint_angle != 0){
				string dirString = Toolbox.Instance.DirectionToString(direction);
				if (dirString == "left" || dirString == "right"){
					holdpoint.rotation = Quaternion.AngleAxis(holdpoint_angle * transform.localScale.x, new Vector3(0, 0, 1f));
				}
			}
		}
	}
	public void SwingItem(MeleeWeapon weapon){
		MessageAnimation anim = new MessageAnimation(MessageAnimation.AnimType.swinging, true);
		Toolbox.Instance.SendMessage(gameObject, this, anim);

		if (weapon.swingSounds.Length > 0){
			GetComponent<AudioSource>().PlayOneShot(weapon.swingSounds[Random.Range(0, weapon.swingSounds.Length)]);
		}
		slashFlag = "right";
		if (directionAngle > 45 && directionAngle < 135){
			slashFlag = "up";
		}
		if (directionAngle > 225 && directionAngle < 315){
			slashFlag = "down";
		}
	}
	public string SwingItem_desc(MeleeWeapon weapon){
		string weaponname = Toolbox.Instance.GetName(weapon.gameObject);
		return "Swing "+weaponname;
	}
	void EndSwing(){
		MessageAnimation anim = new MessageAnimation(MessageAnimation.AnimType.swinging, false);
		Toolbox.Instance.SendMessage(gameObject, this, anim);

		holding.GetComponent<Renderer>().sortingLayerName = "main";
		holding.GetComponent<Renderer>().sortingOrder = GetComponent<Renderer>().sortingOrder - 1;
	}
	void StartSwing(){
		if (holding == null)
			return;
		Dictionary<BuffType, Buff> netBuffs = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(gameObject).NetBuffs();

		GameObject slash = Instantiate(Resources.Load("Slash2"), transform.position, transform.rotation) as GameObject;
		MeleeWeapon weapon = holding.GetComponent<MeleeWeapon>();
		// Intrinsics intrins = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(gameObject);

		slash.transform.SetParent(transform);
		slash.transform.localScale = Vector3.one;
		holding.GetComponent<Renderer>().sortingLayerName = "main";
		holding.GetComponent<Renderer>().sortingOrder = GetComponent<Renderer>().sortingOrder + 1;
		slash.GetComponent<Animator>().SetBool(slashFlag, true);
		Slasher s = slash.GetComponent<Slasher>();

		MessageDamage message = new MessageDamage(weapon.damage, damageType.physical);
		if (weapon.impactSounds.Length > 0){
			message.impactSounds = weapon.impactSounds;
		}
		message.force = new Vector2(direction.x * weapon.damage / 100f, direction.y * weapon.damage / 100f);
		message.responsibleParty = gameObject;
		message.strength = netBuffs[BuffType.strength].boolValue;
		message.type = weapon.damageType;
		message.amount = weapon.damage;
		s.message = message;
	}
	public void DropMessage(GameObject obj){
		SoftDropItem();
	}
	public void WasDestroyed(GameObject obj){
		if (obj == holding.gameObject){
			holding = null;
		}
	}
	public void StartPunch(){
		MessageAnimation anim = new MessageAnimation(MessageAnimation.AnimType.punching, true);
		Toolbox.Instance.SendMessage(gameObject, this, anim);
		if (punchSounds.Count > 0){
			audioSource.PlayOneShot(punchSounds[Random.Range(0, punchSounds.Count)]);
		}
	}
	public void PunchImpact(){
		Dictionary<BuffType, Buff> netBuffs = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(gameObject).NetBuffs();

		Vector3 startPoint = transform.position;
		startPoint.x += direction.normalized.x / 6f;
		startPoint.y += direction.normalized.y / 6f + 0.02f;
		GameObject slash = Instantiate(Resources.Load("PhysicalImpact"), startPoint, holdpoint.rotation) as GameObject;
		PhysicalImpact impact = slash.GetComponent<PhysicalImpact>();
		// Intrinsics intrins = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(gameObject);
		MessageDamage message = new MessageDamage(10f, damageType.physical);
		message.force = new Vector2(direction.x * 0.2f, direction.y * 0.2f);
		message.responsibleParty = gameObject;
		message.strength = netBuffs[BuffType.strength].boolValue;
		message.type = damageType.physical;
		message.amount = 20f;
		message.responsibleParty = gameObject;
		impact.message = message;
		Collider2D slashCollider = slash.GetComponent<Collider2D>();
		foreach (Collider2D tomCollider in GetComponentsInChildren<Collider2D>()){
			Physics2D.IgnoreCollision(tomCollider, slashCollider, true);
		}
	}
	public void EndPunch(){
		MessageAnimation anim = new MessageAnimation(MessageAnimation.AnimType.punching, false);
		Toolbox.Instance.SendMessage(gameObject, this, anim);
	}
	public void ClearInventory(){
		foreach(GameObject item in items){
			Destroy(item);
		}
		items = new List<GameObject>();
	}
	public void ReceiveMessage(Message m){
		if (m is MessageAnimation){
			MessageAnimation message = (MessageAnimation)m;
			if (message.type == MessageAnimation.AnimType.fighting && message.value == true){
				if (holding)
					DropItem();
			}
		}
		if (m is MessageHitstun){
			MessageHitstun message = (MessageHitstun)m;
			if (message.doubledOver || message.knockedDown){
				if (holding)
					DropItem();
			}
		}
		if (m is MessagePunch){
			if (holding){
				MeleeWeapon weapon = holding.GetComponent<MeleeWeapon>();
				if (weapon != null){
					SwingItem(weapon);
				}
			} else{
				StartPunch();
			}
		}
		if (m is MessageNetIntrinsic){
			MessageNetIntrinsic intrins = (MessageNetIntrinsic)m;
			strong = intrins.netBuffs[BuffType.strength].boolValue;
			// if (strong){
			// 	if (strengthFX == null){
			// 		strengthFX = Instantiate(Resources.Load("particles/strength_particles")) as GameObject;
			// 		strengthFX.transform.SetParent(transform, false);
			// 	}
			// } else {
			// 	if (strengthFX != null){
			// 		Destroy(strengthFX);
			// 	}
			// }
		}
	}
	public void UpdateActions(){
		Controllable controllable = GetComponent<Controllable>();
		if (holding) {
			List<Interaction> manualActions = Interactor.ReportManualActions(holding.gameObject, gameObject);
			controllable.defaultInteraction = Interactor.GetDefaultAction(manualActions);
			if (GameManager.Instance.playerObject == gameObject)
				UINew.Instance.CreateActionButtons(manualActions, controllable.defaultInteraction);
		} else {
			controllable.defaultInteraction = null;
			if (GameManager.Instance.playerObject == gameObject){
				UINew.Instance.ClearActionButtons();
				UINew.Instance.ClearWorldButtons();
			}
		}
	}
	public void SaveData(PersistentComponent data){
		if (holding != null){
			MySaver.UpdateGameObjectReference(holding.gameObject, data, "holdingID");
			MySaver.AddToReferenceTree(data.id, holding.gameObject);
		} else {
			data.ints["holdingID"] = -1;
		}
		data.ints["itemCount"] = items.Count;
		if (items.Count > 0){
			for (int i = 0; i < items.Count; i++){
				MySaver.UpdateGameObjectReference(items[i], data, "item"+i.ToString());
				MySaver.AddToReferenceTree(data.id, items[i]);
			}
		} 
		data.vectors["direction"] = direction;
	}
	public void LoadData(PersistentComponent data){
		direction = data.vectors["direction"];
		if (data.ints["holdingID"] != -1){
			GameObject go = MySaver.IDToGameObject(data.ints["holdingID"]);
			if (go != null){
				GetItem(go.GetComponent<Pickup>());
			} else {
				Debug.Log("tried to get loadedobject " + data.ints["holdingID"].ToString() + " but was not found!");
			}
		}
		// note: trying to reference a key in data.ints that didn't exist here caused a hard crash at runtime
		// so PROTECT YA NECK!!!
		if (data.ints["itemCount"] > 0){
			for (int i = 0; i < data.ints["itemCount"]; i++){
				GameObject theItem = MySaver.IDToGameObject(data.ints["item"+i.ToString()]);
				if (theItem == null)
					continue;
				items.Add(theItem);
				theItem.SetActive(false);
				PhysicalBootstrapper testBoot = theItem.GetComponent<PhysicalBootstrapper>();
				if (testBoot){
					testBoot.DestroyPhysical();
				}
			}
		}
		initHolding = null;
	}
}
