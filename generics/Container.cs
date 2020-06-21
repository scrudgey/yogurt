using UnityEngine;
using System.Collections.Generic;
using System;

public class Container : MayorLock, IExcludable, ISaveable {
    public List<Pickup> items = new List<Pickup>();
    public int maxNumber;
    public bool disableContents = true;
    private bool isQuitting;
    public GameObject lockObject;
    public Sprite openSprite;
    public List<Pickup> initItems = new List<Pickup>();
    public Dictionary<Pickup, Interaction> retrieveActions = new Dictionary<Pickup, Interaction>();
    virtual protected void Awake() {
        PopulateContentActions();
    }
    void Start() {
        foreach (Pickup item in initItems) {
            if (item != null) {
                if (item.gameObject.scene.rootCount == 0) {
                    // it's a prefab
                    GameObject newObj = GameObject.Instantiate(item.gameObject, transform.position, Quaternion.identity);
                    AddItem(newObj.GetComponent<Pickup>());
                } else {
                    AddItem(item);
                }
            }
        }
        PopulateContentActions();
    }
    virtual protected void PopulateContentActions() {
        if (lockObject != null)
            return;
        interactions = new List<Interaction>();

        Interaction stasher = new Interaction(this, "Put", "Store");
        stasher.validationFunction = true;
        interactions.Add(stasher);

        retrieveActions = new Dictionary<Pickup, Interaction>();
        foreach (Pickup pickup in items) {
            Pickup closurePickup = pickup;
            Action<Component> removeIt = (comp) => {
                Inventory i = comp as Inventory;
                Remove(i, closurePickup);
            };
            Interaction newInteraction = new Interaction(this, closurePickup.itemName, removeIt);
            newInteraction.actionDelegate = removeIt;
            newInteraction.parameterTypes = new List<Type>();
            newInteraction.parameterTypes.Add(typeof(Inventory));
            newInteraction.descString = "Retrieve " + Toolbox.Instance.GetName(closurePickup.gameObject);
            newInteraction.holdingOnOtherConsent = false;
            // newInteraction.otherOnSelfConsent = false;
            interactions.Add(newInteraction);
            PhysicalBootstrapper bs = pickup.gameObject.GetComponent<PhysicalBootstrapper>();
            if (bs) {
                bs.doInit = false;
            }
            retrieveActions[pickup] = newInteraction;
        }
    }
    protected void RemoveAllRetrieveActions() {
        interactions = new List<Interaction>();
        Interaction stasher = new Interaction(this, "Put", "Store");
        stasher.validationFunction = true;
        interactions.Add(stasher);
    }
    protected void RemoveRetrieveAction(Pickup pickup) {
        if (!retrieveActions.ContainsKey(pickup))
            return;
        interactions.Remove(retrieveActions[pickup]);
    }
    public bool Store_Validation(Inventory inv) {
        if (lockObject != null)
            return false;
        if (inv.holding) {
            if (inv.holding.largeObject || inv.holding.heavyObject)
                return false;
            if (inv.holding.gameObject != gameObject) {
                return true;
            }
            return false;
        } else {
            return false;
        }
    }
    virtual public void Store(Inventory inv) {
        Pickup pickup = inv.holding;
        if (maxNumber == 0 || items.Count < maxNumber) {
            inv.SoftDropItem();
            AddItem(pickup);
        } else {
            Toolbox.Instance.SendMessage(inv.gameObject, this, new MessageSpeech("It's full.") as Message);
        }
    }

    public string Store_desc(Inventory inv) {
        if (inv.holding) {
            string itemname = Toolbox.Instance.GetName(inv.holding.gameObject);
            string myname = Toolbox.Instance.GetName(gameObject);
            return "Put " + itemname + " in " + myname;
        } else {
            return "";
        }
    }
    public void AddItem(Pickup pickup) {
        items.Add(pickup);
        ClaimsManager.Instance.ClaimObject(pickup.gameObject, this);

        // disable its physical
        PhysicalBootstrapper physical = pickup.GetComponent<PhysicalBootstrapper>();
        if (physical)
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
        foreach (Interactive interactive in interactives)
            interactive.disableInteractions = true;

        // make rigidbody kinematic
        if (pickup.GetComponent<Rigidbody2D>())
            pickup.GetComponent<Rigidbody2D>().isKinematic = true;

        Action<Component> removeIt = (comp) => {
            Inventory i = comp as Inventory;
            Remove(i, pickup);
        };
        PopulateContentActions();
    }
    public virtual void Remove(Inventory inv, Pickup pickup) {
        Vector3 pos = inv.transform.position;
        pickup.transform.parent = null;
        pickup.GetComponent<Collider2D>().enabled = true;

        // enable interactions
        Interactive[] interactives = pickup.GetComponents<Interactive>();
        foreach (Interactive interactive in interactives)
            interactive.disableInteractions = false;

        // make rigidbody un kinematic
        if (pickup.GetComponent<Rigidbody2D>())
            pickup.GetComponent<Rigidbody2D>().isKinematic = false;

        pickup.transform.position = pos;
        inv.GetItem(pickup);

        if (openSprite != null) {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = openSprite;
        }

        if (disableContents)
            pickup.gameObject.SetActive(true);
        ClaimsManager.Instance.DisclaimObject(pickup.gameObject, this);
        items.Remove(pickup);
        RemoveRetrieveAction(pickup);
    }
    public void Dump(Pickup pickup) {
        Vector3 pos = transform.position;
        pickup.transform.parent = null;
        pickup.GetComponent<Collider2D>().enabled = true;

        // enable interactions
        Interactive[] interactives = pickup.GetComponents<Interactive>();
        foreach (Interactive interactive in interactives)
            interactive.disableInteractions = false;

        // make rigidbody un kinematic
        if (pickup.GetComponent<Rigidbody2D>())
            pickup.GetComponent<Rigidbody2D>().isKinematic = false;

        pickup.transform.position = pos;

        if (disableContents)
            pickup.gameObject.SetActive(true);

        PhysicalBootstrapper physical = pickup.GetComponent<PhysicalBootstrapper>();
        if (physical && physical.doInit)
            physical.InitPhysical(0.05f, Vector2.zero);

        ClaimsManager.Instance.DisclaimObject(pickup.gameObject, this);
        items.Remove(pickup);
        RemoveRetrieveAction(pickup);
    }
    public void DropMessage(GameObject obj) {
        Pickup pickup = obj.GetComponent<Pickup>();
        if (pickup != null)
            Dump(pickup);
    }
    public virtual void WasDestroyed(GameObject obj) {
        Pickup pickup = obj.GetComponent<Pickup>();
        LiquidContainer liquidContainer = obj.GetComponent<LiquidContainer>();
        LiquidContainer myLiquidContainer = GetComponent<LiquidContainer>();
        if (liquidContainer && myLiquidContainer) {
            myLiquidContainer.FillFromContainer(liquidContainer);
        }
        if (pickup != null) {
            if (items.Contains(pickup)) {
                items.Remove(pickup);
                RemoveRetrieveAction(pickup);
            }
        }
    }
    void OnApplicationQuit() {
        isQuitting = true;
    }
    void OnDestroy() {
        if (isQuitting)
            return;
        while (items.Count > 0) {
            if (items[0] == null) {
                // Dump(items[0]);
                items.RemoveAt(0);
            } else {
                foreach (MonoBehaviour component in items[0].GetComponents<MonoBehaviour>())
                    component.enabled = true;
                Dump(items[0]);
            }
        }
    }
    public virtual void SaveData(PersistentComponent data) {
        data.ints["maxItems"] = maxNumber;
        data.bools["disableContents"] = disableContents;
        data.ints["itemCount"] = items.Count;
        data.bools["locked"] = lockObject != null;
        if (items.Count > 0) {
            for (int i = 0; i < items.Count; i++) {
                // data.ints["item"+i.ToString()] = MySaver.GameObjectToID(instance.items[i].gameObject);
                MySaver.UpdateGameObjectReference(items[i].gameObject, data, "item" + i.ToString());
                MySaver.AddToReferenceTree(data.id, items[i].gameObject);
                MySaver.AddToReferenceTree(gameObject, items[i].gameObject);
            }
        }
    }
    public virtual void LoadData(PersistentComponent data) {
        initItems = new List<Pickup>();
        maxNumber = data.ints["maxItems"];
        disableContents = data.bools["disableContents"];
        if (data.ints["itemCount"] > 0) {
            for (int i = 0; i < data.ints["itemCount"]; i++) {
                GameObject go = MySaver.IDToGameObject(data.GUIDs["item" + i.ToString()]);
                if (go != null) {
                    AddItem(go.GetComponent<Pickup>());
                    PhysicalBootstrapper phys = go.GetComponent<PhysicalBootstrapper>();
                    if (phys)
                        phys.doInit = false;
                }
                // Debug.Log("container containing "+MySaver.loadedObjects[data.ints["item"+i.ToString()]].name);
            }
        }
        if (!data.bools["locked"]) {
            // if (lockObject != null) {
            Unlock();
            // }
        }
    }
    override public void Unlock() {
        Destroy(lockObject);
        lockObject = null;
        PopulateContentActions();
        if (openSprite != null) {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = openSprite;
        }
    }
    override public bool Unlockable() {
        return lockObject != null;
    }
}
