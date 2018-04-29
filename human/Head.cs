using UnityEngine;
public class Head : Interactive, IExcludable, ISaveable {
    private GameObject hatPoint;
    public Hat hat;
    public Hat initHat;
    private SpriteRenderer spriteRenderer;
    void Awake() {
        hatPoint = transform.Find("hatPoint").gameObject;
        spriteRenderer = GetComponent<SpriteRenderer>();
        Interaction wearAct = new Interaction(this, "Wear", "DonHat");
        wearAct.dontWipeInterface = false;
        wearAct.validationFunction = true;
        wearAct.playerOnOtherConsent = false;
        interactions.Add(wearAct);
    }
    public void Start() {
        if (initHat) {
            // Debug.Log("donning init hat");
            if (initHat.isActiveAndEnabled) {
                DonHat(initHat);
            } else {
                Hat instance = Instantiate(initHat) as Hat;
                DonHat(instance);
            }
        }
    }
    public void DonHat(Hat h) {
        if (hat)
            RemoveHat();
        ClaimsManager.Instance.ClaimObject(h.gameObject, this);
        hat = h;
        PhysicalBootstrapper phys = h.GetComponent<PhysicalBootstrapper>();
        if (phys)
            phys.DestroyPhysical();
        if (h.helmet) {
            h.transform.position = transform.position;
            spriteRenderer.enabled = false;
        } else {
            h.transform.position = hatPoint.transform.position;
        }
        h.transform.SetParent(hatPoint.transform, true);
        hatPoint.transform.localScale = Vector3.one;
        transform.localScale = Vector3.one;
        h.transform.rotation = Quaternion.identity;
        // h.GetComponent<Rigidbody2D>().isKinematic = true;
        h.GetComponent<Rigidbody2D>().simulated = false;
        h.GetComponent<Collider2D>().isTrigger = true;
        HatAnimation hatAnimator = hat.GetComponent<HatAnimation>();
        if (hatAnimator) {
            hatAnimator.RegisterDirectable();
        }
        Toolbox.Instance.AddChildIntrinsics(transform.parent.gameObject, this, h.gameObject);
        GameManager.Instance.CheckItemCollection(h.gameObject, transform.root.gameObject);
    }
    public string DonHat_desc(Hat h) {
        return "Wear " + Toolbox.Instance.GetName(h.gameObject);
    }
    public bool DonHat_Validation(Hat h) {
        return !ClaimsManager.Instance.claimedItems.ContainsKey(h.gameObject);
    }
    void RemoveHat() {
        if (hat.helmet) {
            spriteRenderer.enabled = true;
        }
        Toolbox.Instance.RemoveChildIntrinsics(GetComponentInParent<Intrinsics>().gameObject, this);
        ClaimsManager.Instance.DisclaimObject(hat.gameObject, this);
        HatAnimation hatAnimator = hat.GetComponent<HatAnimation>();
        if (hatAnimator) {
            hatAnimator.RemoveDirectable();
        }
        hat.GetComponent<Rigidbody2D>().isKinematic = false;
        hat.GetComponent<Collider2D>().isTrigger = false;
        PhysicalBootstrapper phys = hat.GetComponent<PhysicalBootstrapper>();
        if (phys) {
            phys.InitPhysical(0.2f, Vector2.zero);
        } else {
            hat.transform.parent = null;
        }
        SpriteRenderer hatRenderer = hat.GetComponent<SpriteRenderer>();
        if (hatRenderer) {
            hatRenderer.sortingLayerName = "main";
        }
        hat = null;
    }
    public void DropMessage(GameObject obj) {
        RemoveHat();
    }
    public void WasDestroyed(GameObject obj) {
        if (obj == hat.gameObject) {
            RemoveHat();
        }
    }
    public void SaveData(PersistentComponent data) {
        if (hat != null) {
            MySaver.UpdateGameObjectReference(hat.gameObject, data, "hat");
            MySaver.AddToReferenceTree(data.id, hat.gameObject);
        } else {
            data.ints["hat"] = -1;
        }
    }
    public void LoadData(PersistentComponent data) {
        initHat = null;
        if (data.ints["hat"] != -1) {
            GameObject hat = MySaver.IDToGameObject(data.ints["hat"]);
            if (hat != null) {
                DonHat(hat.GetComponent<Hat>());
            }
        }
    }
}
