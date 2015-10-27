using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class Container : Interactive, IExcludable {

	public List<Pickup> items = new List<Pickup>();
	public int maxNumber;
	public bool disableContents = true;
	private bool isQuitting;

	virtual protected void Start() {
		Interaction stasher = new Interaction(this, "Stash", "Store");
		stasher.displayVerb = "Stash in";
		stasher.validationFunction = true;
		interactions.Add(stasher);
		foreach (Pickup pickup in items){
			Pickup closurePickup = pickup;
			Action<Component> removeIt = (comp) => {
				Inventory i = comp as Inventory;
				Remove(i, closurePickup);
			};
			Interaction newInteraction = new Interaction(this, closurePickup.itemName, removeIt);
			newInteraction.actionDelegate = removeIt;
			newInteraction.parameterTypes = new List<Type>();
			newInteraction.parameterTypes.Add(typeof(Inventory));
			interactions.Add(newInteraction);
			PhysicalBootstrapper bs = pickup.gameObject.GetComponent<PhysicalBootstrapper>();
			if (bs){
				bs.doInit = false;
			}
		}
	}
	

	protected void RemoveRetrieveAction(Pickup pickup){
		Interaction removeThis = null;
		foreach (Interaction interaction in interactions)
		{
			if (interaction.parameters.Count == 2)
				if (interaction.parameters[1] == pickup)
					removeThis = interaction;
			if (interaction.actionName == pickup.itemName)
				    removeThis = interaction;
		}
		if (removeThis != null)
			interactions.Remove(removeThis);
	}

	public bool Store_Validation(Inventory inv){
		if (inv.holding){
			if (inv.holding.gameObject != gameObject){
				return true;
			}
			return false;
		}
		else{
			return false;
		}
	}

	virtual public void Store(Inventory inv){
		Pickup pickup = inv.holding;
		if (maxNumber == 0 || items.Count < maxNumber){
			inv.SoftDropItem();
			AddItem(pickup);
			Action<Component> removeIt = (comp) => {
				Inventory i = comp as Inventory;
				Remove(i,pickup);
			};
			Interaction newInteraction = new Interaction(this,pickup.itemName,removeIt);
			newInteraction.displayVerb = "Retreive "+pickup.itemName+" from";
			newInteraction.actionDelegate = removeIt;
			newInteraction.parameterTypes = new List<Type>();
			newInteraction.parameterTypes.Add(typeof(Inventory));
			interactions.Add(newInteraction);
		} else {
			inv.gameObject.SendMessage("Say","It's full.");
		}
	}

	public void AddItem(Pickup pickup){
		items.Add(pickup);
		Messenger.Instance.ClaimObject(pickup.gameObject,this);
		
		// place it behind me
		SpriteRenderer rend = pickup.GetComponent<SpriteRenderer>();
		if (rend)
			rend.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder - 1;
		
		// disable its physical
		PhysicalBootstrapper physical = pickup.GetComponent<PhysicalBootstrapper>();
		if(physical)
			physical.DestroyPhysical();
		
		//relocate the object
		Vector3 pos = transform.position;
		pickup.transform.position = pos;
		pickup.transform.parent = transform;
		
		// diable the collision
		pickup.GetComponent<Collider2D>().enabled = false;
		
		// disable the object if that's how we're playin it
		if (disableContents)
			pickup.gameObject.SetActive(false);
		
		// disable interactions
		Interactive[] interactives = pickup.GetComponents<Interactive>();
		foreach(Interactive interactive in interactives)
			interactive.disableInteractions = true;
		
		// make rigidbody kinematic
		if (pickup.GetComponent<Rigidbody2D>())
			pickup.GetComponent<Rigidbody2D>().isKinematic = true;
	}
	
	public void Remove(Inventory inv, Pickup pickup){
		Vector3 pos = inv.transform.position;
		pickup.transform.parent = null;
		pickup.GetComponent<Collider2D>().enabled = true;

		// enable interactions
		Interactive[] interactives = pickup.GetComponents<Interactive>();
		foreach(Interactive interactive in interactives)
			interactive.disableInteractions = false;

		// make rigidbody un kinematic
		if (pickup.GetComponent<Rigidbody2D>())
			pickup.GetComponent<Rigidbody2D>().isKinematic = false;

		pickup.transform.position = pos;
		inv.GetItem(pickup);

		if (disableContents)
			pickup.gameObject.SetActive(true);
		
		Messenger.Instance.DisclaimObject(pickup.gameObject, this);

		items.Remove(pickup);
		RemoveRetrieveAction(pickup);
		inv.UpdateActions();

	}

	public void Dump(Pickup pickup){
		Vector3 pos = transform.position;
		pickup.transform.parent = null;
		pickup.GetComponent<Collider2D>().enabled = true;
		
		// enable interactions
		Interactive[] interactives = pickup.GetComponents<Interactive>();
		foreach(Interactive interactive in interactives)
			interactive.disableInteractions = false;
		
		// make rigidbody un kinematic
		if (pickup.GetComponent<Rigidbody2D>())
			pickup.GetComponent<Rigidbody2D>().isKinematic = false;
		
		pickup.transform.position = pos;
		
		if (disableContents)
			pickup.gameObject.SetActive(true);

		
		PhysicalBootstrapper physical = pickup.GetComponent<PhysicalBootstrapper>();
		if(physical && physical.doInit)
			physical.InitPhysical(0.05f,Vector2.zero);


		Messenger.Instance.DisclaimObject(pickup.gameObject,this);
		items.Remove(pickup);
		RemoveRetrieveAction(pickup);
	}

	public void DropMessage(GameObject obj){
		Pickup pickup = obj.GetComponent<Pickup>();
		if (pickup)
			Dump(pickup);
	}

	public void WasDestroyed(GameObject obj){
		Pickup pickup = obj.GetComponent<Pickup>();
		LiquidContainer liquidContainer = obj.GetComponent<LiquidContainer>();
		LiquidContainer myLiquidContainer = GetComponent<LiquidContainer>();
		if (liquidContainer && myLiquidContainer){
			myLiquidContainer.FillFromContainer(liquidContainer);
		}
		if (pickup){
			items.Remove(pickup);
			RemoveRetrieveAction(pickup);
		}
	}

	void OnApplicationQuit()
	{
		isQuitting = true;
	}

	void OnDestroy(){
		if (!isQuitting && MySaver.saveState != MySaver.SaverState.Loading){
			while (items.Count > 0){
				foreach (MonoBehaviour component in items[0].GetComponents<MonoBehaviour>() )
					component.enabled = true;
				Dump(items[0]);
			}
		}
	}

}
