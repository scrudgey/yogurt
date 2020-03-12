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
        // wearAct.debug = true;
        wearAct.dontWipeInterface = false;
        wearAct.validationFunction = true;
        // wearAct.selfOnOtherConsent = true;
        // wearAct.selfOnSelfConsent = false;
        // wearAct.self
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
        h.head = this;
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
        h.transform.localRotation = Quaternion.identity;
        h.transform.localScale = transform.localScale;
        // h.transform.rotation = transform.rotation;

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
        return h.head == null;
    }
    void RemoveHat() {
        if (hat.helmet) {
            spriteRenderer.enabled = true;
        }
        hat.head = null;
        Toolbox.Instance.RemoveChildIntrinsics(GetComponentInParent<Intrinsics>().gameObject, this);
        ClaimsManager.Instance.DisclaimObject(hat.gameObject, this);
        HatAnimation hatAnimator = hat.GetComponent<HatAnimation>();
        if (hatAnimator) {
            hatAnimator.RemoveDirectable();
        }
        Rigidbody2D hatBody = hat.GetComponent<Rigidbody2D>();
        hatBody.isKinematic = false;
        hatBody.simulated = true;
        hat.GetComponent<Collider2D>().isTrigger = false;
        PhysicalBootstrapper phys = hat.GetComponent<PhysicalBootstrapper>();
        if (phys) {
            phys.gameObject.SetActive(true);
            phys.InitPhysical(0.2f, Vector2.zero);
        } else {
            hat.transform.parent = null;
        }
        SpriteRenderer hatRenderer = hat.GetComponent<SpriteRenderer>();
        if (hatRenderer) {
            hatRenderer.sortingLayerName = "main";
        }
        MessageDirectable directable = new MessageDirectable();
        directable.removeDirectable.AddRange(hat.GetComponentsInParent<IDirectable>());
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
            data.GUIDs["hat"] = System.Guid.Empty;
        }
    }
    public void LoadData(PersistentComponent data) {
        initHat = null;
        if (data.GUIDs["hat"] != System.Guid.Empty) {
            GameObject hat = MySaver.IDToGameObject(data.GUIDs["hat"]);
            if (hat != null) {
                DonHat(hat.GetComponent<Hat>());
            }
        }
    }
}
