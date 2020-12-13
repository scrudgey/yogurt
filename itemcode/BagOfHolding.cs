using UnityEngine;
using System.Collections.Generic;
using System;

public class BagOfHolding : Interactive, IExcludable, ISaveable {
    public List<GameObject> items = new List<GameObject>();
    public int maxNumber;
    public bool disableContents = true;
    private bool isQuitting;
    public AudioSource audioSource;
    public AudioClip addSound;
    public AudioClip removeSound;
    virtual protected void Awake() {
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        PopulateContentActions();
    }

    virtual protected void PopulateContentActions() {
        interactions = new List<Interaction>();

        Interaction stasher = new Interaction(this, "Put", "Store");
        stasher.validationFunction = true;
        interactions.Add(stasher);

        Interaction stuffer = new Interaction(this, "Put", "Stash");
        stuffer.validationFunction = true;
        interactions.Add(stuffer);

        if (items.Count > 0) {
            Interaction power = new Interaction(this, "Browse...", "ShowInventoryMenu");
            power.descString = "Browse contents";
            power.unlimitedRange = true;
            power.holdingOnOtherConsent = false;
            interactions.Add(power);

            Interaction dump = new Interaction(this, "Empty", "DumpAll");
            dump.descString = "Dump all contents";
            dump.unlimitedRange = true;
            dump.holdingOnOtherConsent = false;
            interactions.Add(dump);
        }
    }
    public void ShowInventoryMenu() {
        GameObject inventoryMenu = UINew.Instance.ShowMenu(UINew.MenuType.inventory);
        if (inventoryMenu != null) {
            InventoryMenu menu = inventoryMenu.GetComponent<InventoryMenu>();
            menu.Initialize(this);
        }
    }
    public void DumpAll() {
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

    public bool Stash_Validation(Inventory holder) {
        if (!holder.holding)
            return false;
        GameObject obj = holder.holding.gameObject;
        MyMarker marker = obj.GetComponent<MyMarker>();
        if (marker != null && !marker.apartmentObject) {

            // if the player is holding something, we want to invalidate so that "put" will apply to the held object
            // Inventory inv = obj.GetComponent<Inventory>();
            // if (inv != null && inv.holding != null) {
            //     return false;
            // }

            return true;
        } else return false;
    }
    virtual public void Stash(Inventory holder) {
        if (!holder.holding)
            return;
        GameObject obj = holder.holding.gameObject;
        if (maxNumber == 0 || items.Count < maxNumber) {
            AddItem(obj);
        } else {
            // Toolbox.Instance.SendMessage(inv.gameObject, this, new MessageSpeech("It's full.") as Message);
        }
    }

    public string Stash_desc(Inventory holder) {
        if (!holder.holding)
            return "";
        GameObject obj = holder.holding.gameObject;
        if (obj) {
            string itemname = Toolbox.Instance.GetName(obj);
            string myname = Toolbox.Instance.GetName(gameObject);
            return "Put " + itemname + " in " + myname;
        } else {
            return "";
        }
    }

    public bool Store_Validation(GameObject obj) {
        MyMarker marker = obj.GetComponent<MyMarker>();
        if (marker != null && !marker.apartmentObject) {

            // if the player is holding something, we want to invalidate so that "put" will apply to the held object
            // Inventory inv = obj.GetComponent<Inventory>();
            // if (inv != null && inv.holding != null) {
            //     return false;
            // }

            return true;
        } else return false;
    }
    virtual public void Store(GameObject obj) {
        if (maxNumber == 0 || items.Count < maxNumber) {
            AddItem(obj);
        } else {
            // Toolbox.Instance.SendMessage(inv.gameObject, this, new MessageSpeech("It's full.") as Message);
        }
    }

    public string Store_desc(GameObject obj) {
        if (obj) {
            string itemname = Toolbox.Instance.GetName(obj);
            string myname = Toolbox.Instance.GetName(gameObject);
            return "Put " + itemname + " in " + myname;
        } else {
            return "";
        }
    }
    public void AddItem(GameObject pickup) {

        if (this.transform.IsChildOf(pickup.transform.root)) {
            Inventory inv = pickup.GetComponent<Inventory>();
            if (inv != null) {
                inv.DropItem();
            }
        }

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

        Action removeIt = () => {
            Remove(pickup);
        };
        audioSource.PlayOneShot(addSound);

        PopulateContentActions();
    }
    public virtual void Remove(GameObject pickup) {
        pickup.transform.parent = null;
        pickup.GetComponent<Collider2D>().enabled = true;

        // enable interactions
        Interactive[] interactives = pickup.GetComponents<Interactive>();
        foreach (Interactive interactive in interactives)
            interactive.disableInteractions = false;

        // make rigidbody un kinematic
        if (pickup.GetComponent<Rigidbody2D>())
            pickup.GetComponent<Rigidbody2D>().isKinematic = false;

        pickup.transform.position = transform.position;

        if (disableContents)
            pickup.gameObject.SetActive(true);
        ClaimsManager.Instance.DisclaimObject(pickup.gameObject, this);
        items.Remove(pickup);
        audioSource.PlayOneShot(removeSound);
        PopulateContentActions();

    }
    public GameObject Dump(GameObject pickup) {
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
        if (physical)// && physical.doInit)
            physical.InitPhysical(0.05f, Vector2.zero);

        // restart head animation
        HeadAnimation playerHead = pickup.GetComponentInChildren<HeadAnimation>();
        if (playerHead) {
            playerHead.UpdateSequence();
        }

        ClaimsManager.Instance.DisclaimObject(pickup.gameObject, this);
        items.Remove(pickup);
        PopulateContentActions();
        audioSource.PlayOneShot(removeSound);

        return pickup;
    }
    public void DropMessage(GameObject obj) {
        if (obj != null)
            Dump(obj);
    }
    public virtual void WasDestroyed(GameObject obj) {
        LiquidContainer liquidContainer = obj.GetComponent<LiquidContainer>();
        LiquidContainer myLiquidContainer = GetComponent<LiquidContainer>();
        if (liquidContainer && myLiquidContainer) {
            myLiquidContainer.FillFromContainer(liquidContainer);
        }
        if (obj != null) {
            if (items.Contains(obj)) {
                items.Remove(obj);
            }
        }
    }

    public virtual void SaveData(PersistentComponent data) {
        data.ints["maxItems"] = maxNumber;
        data.bools["disableContents"] = disableContents;
        data.ints["itemCount"] = items.Count;
        for (int i = 0; i < items.Count; i++) {
            if (data.GUIDs.ContainsKey($"item{i}"))
                data.GUIDs.Remove($"item{i}");
        }
        if (items.Count > 0) {
            for (int i = 0; i < items.Count; i++) {
                // data.ints["item"+i.ToString()] = MySaver.GameObjectToID(instance.items[i].gameObject);
                MySaver.UpdateGameObjectReference(items[i], data, "item" + i.ToString());
                MySaver.AddToReferenceTree(data.id, items[i]);
                MySaver.AddToReferenceTree(gameObject, items[i]);
            }
        }
    }
    public virtual void LoadData(PersistentComponent data) {
        maxNumber = data.ints["maxItems"];
        disableContents = data.bools["disableContents"];
        if (data.ints["itemCount"] > 0) {
            for (int i = 0; i < data.ints["itemCount"]; i++) {
                GameObject go = MySaver.IDToGameObject(data.GUIDs["item" + i.ToString()]);
                if (go != null) {
                    AddItem(go);
                    Bones itemBones = go.GetComponent<Bones>();
                    if (itemBones != null) {
                        if (itemBones.follower == null)
                            itemBones.Start();
                    }
                } else {
                    Debug.LogError($"{this} could not locate contained object {data.GUIDs["item" + i.ToString()]}. Possible lost saved object on the loose!");
                }
                // Debug.Log("container containing "+MySaver.loadedObjects[data.ints["item"+i.ToString()]].name);
            }
        }

    }

}
