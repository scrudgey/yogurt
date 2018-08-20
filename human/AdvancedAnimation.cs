using UnityEngine;

public class AdvancedAnimation : MonoBehaviour, ISaveable, IDirectable {
    private string _spriteSheet;
    private string spriteSheet {
        get { return _spriteSheet; }
        set {
            if (value != _spriteSheet) {
                _spriteSheet = value;
                LoadSprites();
            }
        }
    }
    private string _sequence;
    public string sequence {
        get { return _sequence; }
        set {
            // Debug.Log("setting sequence "+value);
            // Debug.Log("old value: "+_sequence);
            if (_sequence != value) {
                _sequence = value;
                UpdateSequence();
            }
        }
    }
    private SpriteRenderer spriteRenderer;
    // private Controllable controllable;
    private string lastPressed;
    private bool swinging;
    private bool holding;
    private bool throwing;
    private bool oldHolding;
    private bool fighting;
    private bool punching;
    private Sprite[] sprites;
    public string baseName;
    private int baseFrame;
    private int frame;
    public Animation animator;
    public Controllable.HitState hitState;
    private bool doubledOver;
    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // controllable = GetComponent<Controllable>();
        animator = GetComponent<Animation>();
        LoadSprites();
        Toolbox.RegisterMessageCallback<MessageAnimation>(this, HandleAnimationMessage);
        Toolbox.RegisterMessageCallback<MessageHitstun>(this, HandleHitStun);
    }
    void Start(){
        MessageDirectable message = new MessageDirectable();
        message.addDirectable.Add(this);
        Toolbox.Instance.SendMessage(gameObject, this, message);
    }
    public void HandleAnimationMessage(MessageAnimation anim) {
        switch (anim.type) {
            case MessageAnimation.AnimType.fighting:
                fighting = anim.value;
                LateUpdate();
                SetFrame(0);
                break;
            case MessageAnimation.AnimType.holding:
                holding = anim.value;
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
            default:
                break;
        }
        if (anim.outfitName != "") {
            baseName = anim.outfitName;
            LateUpdate();
            SetFrame(0);
        }
    }
    public void HandleHitStun(MessageHitstun message) {
        hitState = message.hitState;
        doubledOver = message.doubledOver;
        LateUpdate();
        SetFrame(0);
    }
    public void LoadSprites() {
        sprites = Resources.LoadAll<Sprite>("spritesheets/" + spriteSheet);
    }
    public void UpdateSequence() {
        // Debug.Log("updatesequence "+sequence);
        animator.Stop();
        animator.Play(sequence);
        // Debug.Log(animation.IsPlaying(sequence));
    }
    public void LateUpdate() {
        // if (controllable == null)
        //     Awake();
        spriteSheet = baseName + "_spritesheet";
        string updateSequence = "generic3";

        if (swinging) {
            updateSequence = GetSwingState(updateSequence);
        } else if (throwing) {
            updateSequence = GetThrowState(updateSequence);
        } else if (punching) {
            updateSequence = GetFightState(updateSequence);
        } else {
            if (fighting && GetComponent<Rigidbody2D>().velocity.magnitude < 0.1) {
                updateSequence = GetFightState(updateSequence);
            } else {
                updateSequence = GetWalkState(updateSequence);
            }
            if (oldHolding != holding)
                SetFrame(0);
        }
        if (hitState > Controllable.HitState.none) {
            updateSequence = GetHitStunState("generic3");
            if (sequence == null)
                return;
            GetComponent<Animation>().Play(sequence);
        }
        sequence = updateSequence;
        oldHolding = holding;
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
        if (GetComponent<Rigidbody2D>().velocity.magnitude > 0.1) {
            updateSequence = updateSequence + "_run_" + lastPressed; ;
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
        // Debug.Log("set frame "+animationFrame.ToString());
        frame = animationFrame + baseFrame;
        if (frame >= sprites.Length) {
            Debug.Log("coud not set frame");
        } else {
            spriteRenderer.sprite = sprites[frame];
        }
    }
    public void SaveData(PersistentComponent data) {
        data.strings["baseName"] = baseName;
        data.ints["hitstate"] = (int)hitState;
    }
    public void LoadData(PersistentComponent data) {
        hitState = (Controllable.HitState)data.ints["hitstate"];
        baseName = data.strings["baseName"];
        LoadSprites();
    }
    public void DirectionChange(Vector2 newDirection){
        lastPressed = Toolbox.Instance.DirectionToString(newDirection);
        if (lastPressed == "left")
            lastPressed = "right";
    }
}
