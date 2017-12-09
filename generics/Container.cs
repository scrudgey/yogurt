using UnityEngine;
using System.Collections.Generic;
using System;

public class Container : Interactive, IExcludable, ISaveable {
	public List<Pickup> items = new List<Pickup>();
	public int maxNumber;
	public bool disableContents = true;
	private bool isQuitting;

	virtual protected void Start() {
		Interaction stasher = new Interaction(this, "Stash", "Store");
		stasher.validationFunction = true;
		interactions.Add(stasher);
		// good example of loop closure here
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
			newInteraction.descString = "Retrieve "+Toolbox.Instance.GetName(closurePickup.gameObject);
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
			if (interaction.parameters == null){
				removeThis = interaction;
			} else {
				if (interaction.parameters.Count == 2)
					if ((Pickup)interaction.parameters[1] == pickup)
						removeThis = interaction;
				if (interaction.actionName == pickup.itemName)
					    removeThis = interaction;
			}
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
				Remove(i, pickup);
			};
			Interaction newInteraction = new Interaction(this, pickup.itemName, removeIt);
			newInteraction.actionDelegate = removeIt;
			newInteraction.parameterTypes = new List<Type>();
			newInteraction.parameterTypes.Add(typeof(Inventory));
			newInteraction.descString = "Retrieve "+Toolbox.Instance.GetName(pickup.gameObject)+" from "+Toolbox.Instance.GetName(gameObject);
			interactions.Add(newInteraction);
		} else {
			Toolbox.Instance.SendMessage(inv.gameObject, this, new MessageSpeech("It's full.") as Message);
		}
	}
	public string Store_desc(Inventory inv){
		if (inv.holding){
			string itemname = Toolbox.Instance.GetName(inv.holding.gameObject);
			string myname = Toolbox.Instance.GetName(gameObject);
			return "Put "+itemname+" in "+myname;
		} else {
			return "";
		}
	}
	public void AddItem(Pickup pickup){
		items.Add(pickup);
		ClaimsManager.Instance.ClaimObject(pickup.gameObject,this);
		
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
		ClaimsManager.Instance.DisclaimObject(pickup.gameObject, this);
		items.Remove(pickup);
		RemoveRetrieveAction(pickup);
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

		ClaimsManager.Instance.DisclaimObject(pickup.gameObject,this);
		items.Remove(pickup);
		RemoveRetrieveAction(pickup);
	}
	public void DropMessage(GameObject obj){
		Pickup pickup = obj.GetComponent<Pickup>();
		if (pickup != null)
			Dump(pickup);
	}
	public virtual void WasDestroyed(GameObject obj){
		Pickup pickup = obj.GetComponent<Pickup>();
		LiquidContainer liquidContainer = obj.GetComponent<LiquidContainer>();
		LiquidContainer myLiquidContainer = GetComponent<LiquidContainer>();
		if (liquidContainer && myLiquidContainer){
			myLiquidContainer.FillFromContainer(liquidContainer);
		}
		if (pickup != null){
			if (items.Contains(pickup)){
				items.Remove(pickup);
				RemoveRetrieveAction(pickup);
			}
		}
	}
	void OnApplicationQuit(){
		isQuitting = true;
	}
	void OnDestroy(){
		if (isQuitting)
			return;
		while (items.Count > 0){
			foreach (MonoBehaviour component in items[0].GetComponents<MonoBehaviour>() )
				component.enabled = true;
			Dump(items[0]);
		}
	}
	public virtual void SaveData(PersistentComponent data){
		data.ints["maxItems"] = maxNumber;
		data.bools["disableContents"] = disableContents;
		data.ints["itemCount"] = items.Count;
		if (items.Count > 0){
			for (int i = 0; i < items.Count; i++){
				// data.ints["item"+i.ToString()] = MySaver.GameObjectToID(instance.items[i].gameObject);
				MySaver.UpdateGameObjectReference(items[i].gameObject, data, "item"+i.ToString());
				MySaver.AddToReferenceTree(data.id, items[i].gameObject);
			}
		}
	}
	public virtual void LoadData(PersistentComponent data){
		maxNumber = data.ints["maxItems"];
		disableContents = data.bools["disableContents"];
		if (data.ints["itemCount"] > 0){
			for (int i = 0; i < data.ints["itemCount"]; i++){
				GameObject go = MySaver.IDToGameObject(data.ints["item"+i.ToString()]);
				if (go != null){
					AddItem(go.GetComponent<Pickup>());
					PhysicalBootstrapper phys = go.GetComponent<PhysicalBootstrapper>();
					if (phys)
						phys.doInit = false;
				}
				// Debug.Log("container containing "+MySaver.loadedObjects[data.ints["item"+i.ToString()]].name);
			}
		}
	}
}
