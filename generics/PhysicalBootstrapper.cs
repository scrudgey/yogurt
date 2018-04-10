using UnityEngine;
public class PhysicalBootstrapper : MonoBehaviour, ISaveable {
    public enum shadowSize {normal, medium, small};
    public AudioClip[] impactSounds;
    public AudioClip[] landSounds;
    public AudioClip[] scrapeSounds;
    private GameObject hingeObject;
    private Rigidbody2D hingeBody;
    private HingeJoint2D hingeJoint2D;
    private GameObject groundObject;
    private Rigidbody2D groundBody;
    private Rigidbody2D body;
    public CapsuleCollider2D groundCollider;
    private GameObject horizon;
    private SliderJoint2D sliderJoint2D;
    public Physical physical;
    public float initHeight = 0.01f;
    public float groundDrag = 10f;
    public Vector2 initVelocity;
    public bool doInit = true;
    private Vector3 setV;
    private Vector3 addV;
    public GameObject thrownBy;
    public Collider2D objectCollider;
    public PersistentComponent loadData;
    public bool doLoad;
    private bool isQuitting = false;
    public bool impactsMiss;
    public bool noCollisions;
    public shadowSize size;
    private Vector3 previousVelocity;
    public float bounceCoefficient = 0.5f;
    public void Start() {
        tag = "Physical";
        GetComponent<Renderer>().sortingLayerName = "main";
        if (doInit)
            InitPhysical(initHeight, initVelocity);
        if (impactSounds!= null && impactSounds.Length > 0) {
            Toolbox.Instance.SetUpAudioSource(gameObject);
        }
    }
    public void DestroyPhysical() {
        transform.SetParent(null);
        ClaimsManager.Instance.WasDestroyed(groundObject);
        Destroy(groundObject);
        body = GetComponent<Rigidbody2D>();
        if (body){
            body.gravityScale = 0;
            body.velocity = Vector2.zero;
            if (body.drag != 0)
                groundDrag = body.drag;
        }
        physical = null;
        doInit = false;
    }
    public void InitPhysical(float height, Vector3 initialVelocity) {
        // Debug.Log(name + " init physical");
        doInit = false;
        Vector2 initPos = transform.position;
        Vector2 groundPos = transform.position;
        body = GetComponent<Rigidbody2D>();

        // Set up hinge
        hingeObject = new GameObject("hinge");
        hingeObject.transform.position = initPos;
        transform.SetParent(hingeObject.transform);

        hingeBody = hingeObject.AddComponent<Rigidbody2D>();
        hingeJoint2D = hingeObject.AddComponent<HingeJoint2D>();
        hingeBody.mass = 1;
        hingeBody.drag = 0;
        hingeBody.angularDrag = 1;
        hingeBody.gravityScale = 0;
        hingeJoint2D.connectedBody = GetComponent<Rigidbody2D>();
        hingeJoint2D.autoConfigureConnectedAnchor = false;
        hingeJoint2D.connectedAnchor = Vector2.zero;

        // Set up ground object
        groundObject = new GameObject(name + " Ground");
        groundObject.tag = "footprint";
        if (noCollisions) {
            groundObject.layer = 15;
        } else {
            groundObject.layer = 16;
        }
        groundObject.transform.position = initPos;
        Toolbox.Instance.SetUpAudioSource(groundObject);
        //rigidbody 2D
        groundBody = groundObject.AddComponent<Rigidbody2D>();
        groundBody.mass = 1f;
        groundBody.drag = groundDrag;
        groundBody.angularDrag = 0.05f;
        groundBody.gravityScale = 0;
        groundBody.freezeRotation = true;
        groundBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        groundBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        //box collider
        groundCollider = groundObject.AddComponent<CapsuleCollider2D>();
        groundCollider.direction = CapsuleDirection2D.Horizontal;
        groundCollider.sharedMaterial = Resources.Load<PhysicsMaterial2D>("ground");
        groundCollider.size = new Vector2(0.07f, 0.04f);
        foreach (Table table in Object.FindObjectsOfType<Table>()) {
            Collider2D tableCollider = table.transform.parent.GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(groundCollider, tableCollider, true);
        }

        horizon = new GameObject("horizon");
        horizon.layer = 9;
        Rigidbody2D shadowBody = horizon.AddComponent<Rigidbody2D>();
        shadowBody.bodyType = RigidbodyType2D.Kinematic;
        horizon.AddComponent<EdgeCollider2D>();
        horizon.transform.position = initPos;
        horizon.transform.SetParent(groundObject.transform);
        shadowBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        shadowBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        objectCollider = GetComponent<Collider2D>();

        hingeObject.transform.SetParent(groundObject.transform);
        Vector2 tempPos = Vector2.zero;
        height += 0.01f;
        tempPos.y = height;
        groundPos.y -= height;
        hingeObject.transform.localPosition = tempPos;
        groundObject.transform.position = groundPos;

        //sprite renderer
        SpriteRenderer groundSprite = groundObject.AddComponent<SpriteRenderer>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("shadow-sheet") as Sprite[];
        groundSprite.sprite = (Sprite)sprites[(int)size];
        groundSprite.sortingLayerName = "ground";

        //Physical
        Physical groundPhysical = groundObject.AddComponent<Physical>();
        groundPhysical.objectBody = body;
        groundPhysical.hingeBody = hingeBody;
        groundPhysical.groundDrag = groundDrag;
        groundPhysical.groundCollider = groundCollider;
        groundPhysical.objectRenderer = GetComponent<SpriteRenderer>();
        groundPhysical.objectBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        groundPhysical.objectBody.interpolation = RigidbodyInterpolation2D.Interpolate;

        //Slider joint
        sliderJoint2D = groundObject.AddComponent<SliderJoint2D>();
        sliderJoint2D.autoConfigureAngle = false;
        sliderJoint2D.autoConfigureConnectedAnchor = false;
        AnchoredJoint2D zeroAnchor = new AnchoredJoint2D();
        sliderJoint2D.anchor = Vector2.zero;
        sliderJoint2D.connectedAnchor = Vector2.zero;
        sliderJoint2D.enableCollision = false;
        sliderJoint2D.angle = 90f;
        sliderJoint2D.connectedBody = hingeBody;

        groundPhysical.impactSounds = impactSounds;
        groundPhysical.landSounds = landSounds;

        physical = groundPhysical;
        physical.InitValues();
        physical.noCollisions = noCollisions;
        physical.impactsMiss = impactsMiss;
        groundPhysical.bootstrapper = this;
        transform.localPosition = Vector2.zero;
        Set3Motion(new Vector3(initialVelocity.x, initialVelocity.y, initialVelocity.z));
    }
    void OnCollisionStay2D(Collision2D coll) {
        if (coll.gameObject == horizon){
            physical.StartGroundMode();
        }
    }
    void OnCollisionEnter2D(Collision2D coll) {
        if (coll.relativeVelocity.magnitude > 0.1) {
            if (impactSounds != null)
                if (impactSounds.Length > 0) {
                    GetComponent<AudioSource>().PlayOneShot(impactSounds[Random.Range(0, impactSounds.Length)]);
                }
            EventData data = Toolbox.Instance.DataFlag(gameObject, chaos: 1);
            data.noun = "collision";
            data.whatHappened = Toolbox.Instance.CloneRemover(coll.gameObject.name) + " collided with " + Toolbox.Instance.CloneRemover(gameObject.name);
        }
        if (coll.gameObject == groundObject) {

        } else if (coll.gameObject == horizon) {
            if (coll.relativeVelocity.magnitude > 0.1) {
                // Debug.Log(name + " bounced");
                Vector3 vel = previousVelocity;
                Vector3 groundVelocity = groundBody.velocity;
                float z = vel.y - groundVelocity.y;
                vel.y = groundVelocity.y + (-1f * bounceCoefficient * z);
                groundVelocity.y = 0.9f * groundVelocity.y;
                // vel.y = -0.5f * vel.y;
                body.velocity = vel;
                groundBody.velocity = groundVelocity;
                physical.BroadcastMessage("OnGroundImpact", physical, SendMessageOptions.DontRequireReceiver);
            } else {
                physical.suppressLandSound = true;
                physical.GroundMode();
            }
        } else {
            if (physical == null)
                return;
            if (physical.currentMode == Physical.mode.zip) {
                physical.Rebound();
                // Debug.Log("physical bootstrapper collision: "+gameObject.name+" + "+coll.gameObject.name);
                MessageDamage message = new MessageDamage();
                message.responsibleParty = thrownBy;
                ContactPoint2D contact = coll.contacts[0];
                message.force = contact.normal;
                if (physical.currentMode == Physical.mode.zip){
                    message.amount = 25f;
                } else {
                    message.amount = coll.relativeVelocity.magnitude;
                    Debug.Log(message.amount);
                }
                message.type = damageType.physical;
                Toolbox.Instance.SendMessage(gameObject, this, message);
                physical.FlyMode();
                GameObject speaker = Instantiate(Resources.Load("Speaker"), transform.position, Quaternion.identity) as GameObject;
                speaker.GetComponent<AudioSource>().clip = Resources.Load("sounds/8bit_impact1", typeof(AudioClip)) as AudioClip;
                speaker.GetComponent<AudioSource>().Play();
            }
        }
    }
    public void Set3Motion(Vector3 velocity) {
        setV = velocity;
    }
    public void Add3Motion(Vector3 velocity) {
        addV = velocity;
    }
    public void Set3MotionImmediate(Vector3 velocity) {
        Vector2 groundVelocity = new Vector2(velocity.x, velocity.y);
        Vector2 objectVelocity = new Vector2(velocity.x, velocity.z + velocity.y);
        physical.objectBody.velocity = objectVelocity;
        groundBody.velocity = groundVelocity;
    }

    void FixedUpdate() {
        if (setV != Vector3.zero) {
            Vector2 groundVelocity = new Vector2(setV.x, setV.y);
            Vector2 objectVelocity = new Vector2(setV.x, setV.z + setV.y);
            physical.objectBody.velocity = objectVelocity;
            groundBody.velocity = groundVelocity;
            setV = Vector3.zero;
        }
        if (addV != Vector3.zero){
            Vector2 objectVelocity = physical.objectBody.velocity;
            Vector2 groundVelocity = groundBody.velocity;
            groundVelocity.x += addV.x;
            groundVelocity.y += addV.y;
            objectVelocity.x += addV.x;
            objectVelocity.y += addV.y;
            objectVelocity.y += addV.z;
            physical.objectBody.velocity = objectVelocity;
            groundBody.velocity = groundVelocity;
            addV = Vector3.zero;
        }
        if (doLoad) {
            doLoad = false;
            initHeight = 0;
            initVelocity = Vector3.zero;
            GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            if (loadData == null)
                return;
            if (physical != null) {
                physical.currentMode = (Physical.mode)loadData.ints["mode"];
                physical.transform.position = loadData.vectors["groundPosition"];
                physical.hinge.transform.localPosition = loadData.vectors["objectPosition"];
                Vector2 offset = new Vector2(loadData.floats["horizonOffsetX"], loadData.floats["horizonOffsetY"]);
                physical.horizonCollider.offset = offset;
                if (physical.currentMode == Physical.mode.fly) {
                    physical.StartFlyMode();
                }
                if (physical.currentMode == Physical.mode.zip) {
                    physical.StartZipMode();
                }
                if (physical.currentMode == Physical.mode.ground) {
                    physical.StartGroundMode();
                }
                physical.objectBody.velocity = Vector3.zero;
            }
        }
        previousVelocity = body.velocity;
    }
    void OnApplicationQuit() {
        isQuitting = true;
    }
    void OnDestroy() {
        if (isQuitting)
            return;
        if (physical && physical.gameObject) {
            ClaimsManager.Instance.WasDestroyed(physical.gameObject);
            Destroy(physical.gameObject);
        }
    }
    public void SaveData(PersistentComponent data) {
        data.bools["doInit"] = doInit;
        data.bools["physical"] = false;
        if (physical != null) {
            if (!physical.isActiveAndEnabled)
                return;
            data.bools["physical"] = true;
            data.ints["mode"] = (int)physical.currentMode;
            data.vectors["groundPosition"] = physical.transform.position;
            data.vectors["objectPosition"] = physical.hinge.transform.localPosition;

            data.floats["horizonOffsetX"] = physical.horizonCollider.offset.x;
            data.floats["horizonOffsetY"] = physical.horizonCollider.offset.y;
        }
    }
    public void LoadData(PersistentComponent data) {
        if (data.bools["physical"]) {
            loadData = data;
        }
        doLoad = true;
    }
}
