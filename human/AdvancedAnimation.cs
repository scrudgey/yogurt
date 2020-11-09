using UnityEngine;

public class AdvancedAnimation : MonoBehaviour, ISaveable, IDirectable {
    public SkinColor _skinColor;
    public SkinColor skinColor {
        get { return _skinColor; }
        set {
            _skinColor = value;
            LoadSprites();
        }
    }
    private string _sequence;
    public string sequence {
        get { return _sequence; }
        set {
            if (_sequence != value) {
                _sequence = value;
                UpdateSequence();
            }
        }
    }
    private SpriteRenderer spriteRenderer;
    public string lastPressed = "right";
    private bool swinging;
    private bool holding;
    private bool throwing;
    private bool fighting;
    private bool punching;
    private bool panic;
    private Sprite[] sprites;
    public string baseName;
    private int baseFrame;
    private int frame;
    public Animation animator;
    public Controllable.HitState hitState;
    public Rigidbody2D body;
    private bool doubledOver;
    public Controllable controllable;
    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animation>();
        LoadSprites();
        Toolbox.RegisterMessageCallback<MessageAnimation>(this, HandleAnimationMessage);
        Toolbox.RegisterMessageCallback<MessageHitstun>(this, HandleHitStun);
        Toolbox.RegisterMessageCallback<MessageInventoryChanged>(this, HandleInventoryMessage);
        Toolbox.RegisterMessageCallback<MessageNetIntrinsic>(this, HandleNetIntrinsic);
        body = GetComponent<Rigidbody2D>();
        controllable = GetComponent<Controllable>();
    }
    void Start() {
        MessageDirectable message = new MessageDirectable();
        message.addDirectable.Add(this);
        Toolbox.Instance.SendMessage(gameObject, this, message);
    }
    public void HandleNetIntrinsic(MessageNetIntrinsic message) {
        if (message.netBuffs[BuffType.undead].boolValue) {
            skinColor = SkinColor.undead;
            LoadSprites();
        }
    }
    public void HandleInventoryMessage(MessageInventoryChanged message) {
        // bool oldHolding = holding;
        bool messageHolding = message.holding != null;
        bool messageDrop = message.dropped != null;

        bool doUpdate = false;

        if (messageHolding) {
            holding = true;
            doUpdate = true;
        } else {
            holding = false;
            doUpdate = true;
        }

        if (messageDrop) {
            holding = false;
            doUpdate = true;
        } else {
            holding = true;
            doUpdate = true;
        }

        if (throwing || swinging)
            return;
        if (doUpdate) {
            LateUpdate();
            SetFrame(0);
        }
    }
    public void HandleAnimationMessage(MessageAnimation anim) {
        switch (anim.type) {
            case MessageAnimation.AnimType.fighting:
                fighting = anim.value;
                LateUpdate();
                SetFrame(0);
                break;
            case MessageAnimation.AnimType.throwing:
                throwing = anim.value;
                break;
            case MessageAnimation.AnimType.swinging:
                swinging = anim.value;
                break;
            case MessageAnimation.AnimType.punching:
                punching = anim.value;
                break;
            case MessageAnimation.AnimType.panic:
                panic = anim.value;
                break;
            default:
                break;
        }
        if (anim.outfitName != "") {
            baseName = anim.outfitName;
            LoadSprites();
            LateUpdate();
        }
    }
    public void HandleHitStun(MessageHitstun message) {
        hitState = message.hitState;
        doubledOver = message.doubledOver;
        LateUpdate();
        SetFrame(0);
    }
    public void LoadSprites() {
        string spriteSheet = baseName + "_spritesheet";
        // sprites = Toolbox.ApplySkinToneToSpriteSheet(spriteSheet, skinColor);
        sprites = Toolbox.MemoizedSkinTone(spriteSheet, skinColor);
        SetFrame(0);
    }
    public void UpdateSequence() {
        // Debug.Log("updatesequence "+sequence);
        animator.Stop();
        animator.Play(sequence);
        // Debug.Log(animation.IsPlaying(sequence));
    }
    public void LateUpdate() {
        // spriteSheet = baseName + "_spritesheet";
        string updateSequence = "generic3";

        if (swinging) {
            updateSequence = GetSwingState(updateSequence);
        } else if (throwing) {
            updateSequence = GetThrowState(updateSequence);
        } else if (punching) {
            updateSequence = GetFightState(updateSequence);
        } else if (fighting && body.velocity.magnitude < 0.1) {
            updateSequence = GetFightState(updateSequence);
        } else if (panic) {
            updateSequence = GetPanicState(updateSequence);
        } else {
            updateSequence = GetWalkState(updateSequence);
        }

        if (hitState > Controllable.HitState.none) {
            updateSequence = GetHitStunState("generic3");
            if (sequence == null)
                return;
            GetComponent<Animation>().Play(sequence);
        }
        sequence = updateSequence;
    }
    string GetSwingState(string updateSequence) {
        updateSequence = updateSequence + "_swing_" + lastPressed;
        switch (lastPressed) {
            case "down":
                baseFrame = 44;
                break;
            case "up":
                baseFrame = 46;
                break;
            default:
                baseFrame = 42;
                break;
        }
        return updateSequence;
    }
    string GetThrowState(string updateSequence) {
        updateSequence = updateSequence + "_throw_" + lastPressed;
        switch (lastPressed) {
            case "down":
                baseFrame = 50;
                break;
            case "up":
                baseFrame = 52;
                break;
            default:
                baseFrame = 48;
                break;
        }
        return updateSequence;
    }
    string GetWalkState(string updateSequence) {
        if (controllable.running)
            return GetRunningState(updateSequence);
        switch (lastPressed) {
            case "down":
                baseFrame = 7;
                break;
            case "up":
                baseFrame = 14;
                break;
            default:
                baseFrame = 0;
                break;
        }
        if (holding) {
            baseFrame += 21;
        }
        if (body.velocity.magnitude > 0.1) {
            updateSequence = updateSequence + "_run_" + lastPressed;
            baseFrame += 1;
        } else {
            updateSequence = updateSequence + "_idle_" + lastPressed;
        }
        return updateSequence;
    }

    string GetRunningState(string updateSequence) {
        switch (lastPressed) {
            case "down":
                baseFrame = 89;
                break;
            case "up":
                baseFrame = 94;
                break;
            default:
                baseFrame = 84;
                break;
        }
        if (body.velocity.magnitude > 0.1) {
            updateSequence = updateSequence + "_panic_right";
            baseFrame += 1;
        } else {
            updateSequence = updateSequence + "_idle_" + lastPressed;
        }
        return updateSequence;
    }
    string GetPanicState(string updateSequence) {
        switch (lastPressed) {
            case "down":
                baseFrame = 74;
                break;
            case "up":
                baseFrame = 79;
                break;
            default:
                baseFrame = 69;
                break;
        }
        if (body.velocity.magnitude > 0.1) {
            updateSequence = updateSequence + "_panic_right";
            baseFrame += 1;
        } else {
            updateSequence = updateSequence + "_idle_" + lastPressed;
        }
        return updateSequence;
    }
    string GetFightState(string updateSequence) {
        switch (lastPressed) {
            case "down":
                baseFrame = 57;
                break;
            case "up":
                baseFrame = 60;
                break;
            default:
                baseFrame = 54;
                break;
        }
        if (!punching) {
            updateSequence = updateSequence + "_idle_" + lastPressed;
        } else {
            updateSequence = updateSequence + "_punch_" + lastPressed;
        }
        return updateSequence;
    }
    string GetHitStunState(string updateSequence) {
        switch (lastPressed) {
            case "down":
                baseFrame = 64;
                break;
            case "up":
                baseFrame = 65;
                break;
            default:
                baseFrame = 63;
                break;
        }
        if (!doubledOver) {
            updateSequence = updateSequence + "_idle_" + lastPressed;
        } else {
            updateSequence = updateSequence + "_doubled_" + lastPressed;
            baseFrame += 3;
        }
        return updateSequence;
    }
    public void SetFrame(int animationFrame) {
        frame = animationFrame + baseFrame;
        if (frame >= sprites.Length) {
            Debug.Log("coud not set frame");
            Debug.Log(baseName);
        } else {
            spriteRenderer.sprite = sprites[frame];
        }
    }
    public void SaveData(PersistentComponent data) {
        data.strings["baseName"] = baseName;
        data.ints["hitstate"] = (int)hitState;
        data.ints["skincolor"] = (int)skinColor;
    }
    public void LoadData(PersistentComponent data) {
        hitState = (Controllable.HitState)data.ints["hitstate"];
        baseName = data.strings["baseName"];
        skinColor = (SkinColor)data.ints["skincolor"];
    }
    public void DirectionChange(Vector2 newDirection) {
        lastPressed = Toolbox.Instance.DirectionToString(newDirection);
        if (lastPressed == "left")
            lastPressed = "right";
    }
}
