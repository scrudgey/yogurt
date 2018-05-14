using UnityEngine;
using System.Collections.Generic;
public class Physical : MonoBehaviour {
    public enum mode { none, fly, ground, zip }
    // public AudioClip[] impactSounds = new AudioClip[0];
    public AudioClip[] landSounds = new AudioClip[0];
    public PhysicalBootstrapper bootstrapper;
    private GameObject trueObject;
    public Rigidbody2D body;
    public Rigidbody2D objectBody;
    public Collider2D objectCollider;
    public Collider2D groundCollider;
    public Rigidbody2D hingeBody;
    public GameObject hinge;
    public SliderJoint2D slider;
    public EdgeCollider2D horizonCollider;
    public mode currentMode;
    public float height;
    public bool doFly;
    private bool doGround;
    private bool doZip;
    private bool doStartTable;
    private bool doStopTable;
    private SpriteRenderer spriteRenderer;
    public SpriteRenderer objectRenderer;
    public float groundDrag;
    private float ziptime;
    public bool suppressLandSound;
    private Table table;
    public List<Collider2D> temporaryDisabledColliders = new List<Collider2D>();
    public bool impactsMiss;
    public bool noCollisions;
    public AudioSource audioSource;
    void Awake() {
        InitValues();
    }
    public void Start() {
        body = Toolbox.GetOrCreateComponent<Rigidbody2D>(gameObject);
        // ignore collisions between ground and all other objects
        foreach (Physical phys in GameObject.FindObjectsOfType<Physical>()) {
            if (phys == this)
                continue;
            Physics2D.IgnoreCollision(horizonCollider, phys.GetComponent<Collider2D>(), true);
            foreach (Collider2D col in phys.GetComponentsInChildren<Collider2D>()) {
                if (col.tag == "fire")
                    continue;
                Physics2D.IgnoreCollision(objectCollider, col, true);
            }
        }
        Physics2D.IgnoreCollision(horizonCollider, objectCollider, false);
        if (currentMode == mode.none)
            FlyMode();
    }
    public void InitValues() {
        slider = GetComponent<SliderJoint2D>();
        hinge = transform.Find("hinge").gameObject;
        trueObject = hinge.transform.GetChild(0).gameObject;
        objectCollider = trueObject.GetComponent<Collider2D>();
        horizonCollider = transform.Find("horizon").GetComponent<EdgeCollider2D>();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate() {
        if (trueObject == null) {
            Destroy(this);
            return;
        }
        height = horizonCollider.offset.y + hinge.transform.localPosition.y;
        if (height < 0) {
            // Debug.Log(name + " under horizon");
            float angle = Vector2.Angle(Vector2.right, trueObject.transform.right) * Mathf.Deg2Rad;
            Vector2 hingePosition = Vector2.zero;
            hingePosition.y += Mathf.Cos(angle) * objectCollider.offset.y + Mathf.Sin(angle) * objectCollider.offset.x;
            hingePosition.y += Mathf.Cos(angle) * objectCollider.bounds.extents.y + Mathf.Sin(angle) * objectCollider.bounds.extents.x;
            hinge.transform.localPosition = hingePosition;
            trueObject.transform.localPosition = Vector2.zero;
            Vector3 objectV = objectBody.velocity;
            Vector3 bodyV = body.velocity;
            Vector3 hingeV = hingeBody.velocity;
            objectV.y = 0f;
            bodyV.y = 0f;
            hingeV.y = 0f;
            objectBody.velocity = objectV;
            body.velocity = bodyV;
            hingeBody.velocity = hingeV;
        }
        if (currentMode == mode.fly) {
            if (height < 0.1 && height > 0) {
                // body.drag = 5;
                body.mass = objectBody.mass;
            } else {
                // body.drag = 0;
                body.mass = 1;
            }
        }
        if (ziptime > 0) {
            ziptime -= Time.fixedDeltaTime;
            if (ziptime <= 0) {
                doFly = true;
            }
        }
        if (currentMode == mode.fly || currentMode == mode.zip) {
            // projectiles should be on their own layer: if i placed them on main, they would 
            // not collide with their footprint. if i placed them on feet, they would not collide with 
            // extended objects.
            trueObject.layer = 11;
        } else {
            trueObject.layer = 10;
        }
        if (doGround) {
            doGround = false;
            StartGroundMode();
        }
        if (doFly) {
            doFly = false;
            StartFlyMode();
        }
        if (doZip) {
            doZip = false;
            StartZipMode();
        }
        if (doStartTable) {
            doStartTable = false;
            StartTable();
        }
        if (doStopTable) {
            doStopTable = false;
            StopTable();
        }
        if (currentMode == mode.ground) {
            if (objectCollider.Distance(horizonCollider).isOverlapped) {
                slider.useLimits = false;
            } else {
                if (slider.useLimits == false) {
                    SetSliderLimit(0);
                }
            }
        }
    }
    public void GroundMode() {
        // Debug.Log(name + " groundmode");
        doGround = true;
    }
    public void FlyMode() {
        // Debug.Log(name + " flymode");
        doFly = true;
    }
    public void ZipMode() {
        // Debug.Log(name + " zipmode");
        doZip = true;
    }
    public void SetSliderLimit(float offset) {
        JointTranslationLimits2D tempLimits = slider.limits;
        tempLimits.min = 0;
        tempLimits.max = hinge.transform.localPosition.y + offset;
        slider.limits = tempLimits;
        slider.useLimits = true;
    }
    public void StartGroundMode() {
        // Debug.Log(name + " start ground mode");
        if (body == null)
            Start();
        doGround = false;
        doFly = false;
        doZip = false;
        ziptime = 0f;
        ClearTempColliders();
        body.drag = groundDrag;
        body.mass = objectBody.mass;
        currentMode = mode.ground;
        foreach (Physical phys in FindObjectsOfType<Physical>()) {
            if (phys == this)
                continue;
            if (phys.currentMode == mode.ground) {
                Physics2D.IgnoreCollision(objectCollider, phys.horizonCollider, true);
                Physics2D.IgnoreCollision(horizonCollider, phys.objectCollider, true);
            }
        }
        if (landSounds != null)
            if (landSounds.Length > 0 && !suppressLandSound)
                audioSource.PlayOneShot(landSounds[Random.Range(0, landSounds.Length)]);
        suppressLandSound = false;
        SetSliderLimit(1f * objectCollider.Distance(horizonCollider).distance);
        if (noCollisions) {
            gameObject.layer = 15;
        } else {
            gameObject.layer = 16;
        }
        if (spriteRenderer)
            spriteRenderer.enabled = false;
        if (objectRenderer)
            objectRenderer.sortingLayerName = "main";
        bootstrapper.BroadcastMessage("GroundModeStart", this, SendMessageOptions.DontRequireReceiver);
    }
    public void ClearTempColliders() {
        foreach (Collider2D temporaryCollider in temporaryDisabledColliders) {
            Physics2D.IgnoreCollision(temporaryCollider, objectCollider, false);
            Physics2D.IgnoreCollision(temporaryCollider, horizonCollider, false);
            Physics2D.IgnoreCollision(temporaryCollider, groundCollider, false);
        }
        temporaryDisabledColliders = new List<Collider2D>();
    }
    public void StartFlyMode() {
        // Debug.Log(name + " start fly mode");
        doGround = false;
        doFly = false;
        doZip = false;
        ziptime = 0f;
        ClearTempColliders();
        // set object gravity on
        objectBody.gravityScale = GameManager.Instance.gravity;
        slider = GetComponent<SliderJoint2D>();
        slider.useLimits = false;
        body = GetComponent<Rigidbody2D>();
        body.drag = 0;
        //update mode
        currentMode = mode.fly;
        // set ground friction
        body = GetComponent<Rigidbody2D>();
        body.drag = 0;
        body.mass = 1;
        foreach (Physical phys in FindObjectsOfType<Physical>()) {
            if (phys != this) {
                Physics2D.IgnoreCollision(horizonCollider, phys.objectCollider, true);
                Physics2D.IgnoreCollision(objectCollider, phys.horizonCollider, true);
            }
        }
        gameObject.layer = 15;
        if (spriteRenderer)
            spriteRenderer.enabled = true;
        if (objectRenderer)
            objectRenderer.sortingLayerName = "air";
    }
    public void StartZipMode() {
        doZip = false;
        doFly = false;
        StartFlyMode();
        objectBody.gravityScale = 0;
        ziptime = 0.5f;
        currentMode = mode.zip;
    }
    void OnTriggerEnter2D(Collider2D coll) {
        if (coll.tag == "table" && coll.gameObject != gameObject) {
            // START TABLE
            // Debug.Log(name + " table collision detected, dostarttable true");
            table = coll.GetComponentInParent<Table>();
            doStartTable = true;
            // Debug.Break();
        }
    }
    void OnTriggerExit2D(Collider2D coll) {
        if (coll.tag == "table" && coll.gameObject != gameObject) {
            // STOP TABLE
            table = coll.GetComponentInParent<Table>();
            doStopTable = true;
        }
    }
    void StartTable() {
        // Debug.Log(name + " start table");
        Vector2 newOffset = new Vector2(0f, table.height);
        horizonCollider.offset = newOffset;
        Vector3 objectPosition = hinge.transform.localPosition;
        if (objectPosition.y < table.height) {
            objectPosition.y += table.height + 0.02f;
        }
        hinge.transform.localPosition = objectPosition;
        JointTranslationLimits2D tempLimits = slider.limits;
        tempLimits.min = 0;
        tempLimits.max = hinge.transform.localPosition.y;
        slider.limits = tempLimits;
        transform.SetParent(table.transform);
        if (spriteRenderer)
            spriteRenderer.enabled = false;
        if (objectRenderer)
            objectRenderer.sortingLayerName = "main";
    }
    void StopTable() {
        horizonCollider.offset = Vector2.zero;
        JointTranslationLimits2D tempLimits = slider.limits;
        tempLimits.min = 0;
        tempLimits.max = hinge.transform.localPosition.y;
        slider.limits = tempLimits;
        transform.SetParent(null);
        if (spriteRenderer)
            spriteRenderer.enabled = true;
        // FlyMode();
    }
    void OnCollisionEnter2D(Collision2D collision) {
        bootstrapper.Collision(collision);
    }
}
