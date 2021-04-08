using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crossbow : Interactive, IDirectable, ISaveable {
    public Transform right;
    public Transform down;
    public Transform up;
    public float height;
    public string lastPressed;
    public float velocity;
    public AudioClip squirtSound;
    protected AudioSource audioSource;
    public GameObject dartPrefab;
    public bool tranq;
    public Liquid liquid;
    public void Awake() {

        Interaction squirtAction = new Interaction(this, "Shoot", "Squirt");//, false, true);
        squirtAction.selfOnOtherConsent = false;
        squirtAction.otherOnSelfConsent = false;
        squirtAction.defaultPriority = 6;
        squirtAction.validationFunction = false;
        interactions.Add(squirtAction);

        Interaction spray2 = new Interaction(this, "Shoot", "SprayObject");//, true, false);
        spray2.selfOnSelfConsent = false;
        spray2.otherOnSelfConsent = false;
        spray2.unlimitedRange = true;
        spray2.dontWipeInterface = true;
        spray2.validationFunction = false;
        interactions.Add(spray2);

        if (tranq) {
            Interaction fillReservoir = new Interaction(this, "Dip...", "FillFromReservoir");
            Interaction fillContainer = new Interaction(this, "Dip...", "FillFromContainer");
            fillContainer.validationFunction = true;

            interactions.Add(fillReservoir);
            interactions.Add(fillContainer);
            // TODO: initialize with tranquilizer
        }

        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    public void Squirt() {
        Transform direction;
        switch (lastPressed) {
            case "down":
                direction = down;
                break;
            case "up":
                direction = up;
                break;
            default:
                direction = right;
                break;
        }
        doSquirt(direction.up, direction.position);
    }

    public void SprayObject(GameObject item) {
        Vector3 direction = (item.transform.position - transform.position).normalized;
        doSquirt(direction, transform.position);
    }

    public string SprayObject_desc(GameObject item) {
        string itemname = Toolbox.Instance.GetName(item.gameObject);
        return $"Shoot at {itemname}";
    }

    public virtual void doSquirt(Vector3 direction, Vector3 position) {
        GameObject dart = GameObject.Instantiate(dartPrefab, transform.position, Quaternion.identity) as GameObject;
        PhysicalBootstrapper phys = dart.GetComponent<PhysicalBootstrapper>();
        phys.impactsMiss = true;
        Vector2 initpos = transform.position;
        Inventory holderInv = GetComponentInParent<Inventory>();
        Projectile projectile = dart.GetComponent<Projectile>();
        if (projectile != null) {
            projectile.responsibleParty = holderInv.gameObject;
        }
        float initHeight = height;
        if (holderInv) {
            initHeight += holderInv.dropHeight;
        }
        phys.doInit = false;
        phys.silentImpact = true;
        phys.InitPhysical(initHeight, direction * velocity);
        phys.physical.StartZipMode();

        if (tranq) {
            Projectile proj = dart.GetComponent<Projectile>();
            proj.liquid = liquid;
        }

        if (squirtSound != null)
            audioSource.PlayOneShot(squirtSound);
        foreach (Collider2D myCollider in transform.root.GetComponentsInChildren<Collider2D>()) {
            foreach (Collider2D dropCollider in dart.transform.root.GetComponentsInChildren<Collider2D>()) {
                Physics2D.IgnoreCollision(myCollider, dropCollider, true);
            }
        }
    }
    public void DirectionChange(Vector2 newdir) {
        lastPressed = Toolbox.Instance.DirectionToString(newdir);
    }

    public void FillFromReservoir(LiquidResevoir l) {
        liquid = l.liquid;
    }
    public string FillFromReservoir_desc(LiquidResevoir l) {
        string resname = "reservoir";
        if (l.genericName != "") {
            resname = l.genericName;
        } else {
            resname = Toolbox.Instance.GetName(l.gameObject);
        }
        return "Dip darts in " + l.liquid.name + " from " + resname;
    }
    public void FillFromContainer(LiquidContainer l) {
        if (l.amount > 0) {
            liquid = l.liquid;
        }
    }
    public bool FillFromContainer_Validation(LiquidContainer l) {
        return l.amount > 0;
    }
    public string FillFromContainer_desc(LiquidContainer l) {
        string resname = Toolbox.Instance.GetName(l.gameObject);
        return "Dip darts in " + l.liquid.name + " from " + resname;
    }
    public void SaveData(PersistentComponent data) {
        if (tranq) {
            data.liquids["liquid"] = liquid;
        }
    }
    public void LoadData(PersistentComponent data) {
        if (tranq) {
            liquid = data.liquids["liquid"];
        }
    }
}
