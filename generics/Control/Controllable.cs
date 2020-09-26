using UnityEngine;
using System.Collections.Generic;
using System;

public class Controllable : MonoBehaviour {
    public bool big; // if true, cannot fit in doors
    public static List<Type> ControlPriority = new List<Type>{
        typeof(Intrinsics),
        typeof(Outfit),
        typeof(Inventory)
    };

    public static int CompareComponent(Component x, Component y) {
        if (ControlPriority.Contains(x.GetType()) && ControlPriority.Contains(y.GetType())) {
            return ControlPriority.IndexOf(x.GetType()) - ControlPriority.IndexOf(y.GetType());
        } else if (ControlPriority.Contains(x.GetType())) {
            return 1;
        } else if (ControlPriority.Contains(y.GetType())) {
            return -1;
        } else {
            return 0;
        }
    }

    public enum HitState { none, stun, unconscious, dead };
    // public enum ControlType { none, AI, player }
    public InteractionParam defaultInteraction;
    public static List<Type> AIComponents = new List<Type>(){
        typeof(DecisionMaker),
        typeof(PeterPicklebottom)
    };
    static public HitState AddHitState(HitState orig, HitState argument) {
        if (argument > orig) {
            return argument;
        }
        return orig;
    }
    static public HitState RemoveHitState(HitState orig, HitState argument) {
        if (argument >= orig) {
            return HitState.none;
        }
        return orig;
    }
    private bool _upFlag;
    private bool _downFlag;
    private bool _leftFlag;
    private bool _rightFlag;
    protected bool upFlag {
        get { if (disabled) return false; else return _upFlag; }
        set { _upFlag = value; }
    }
    protected bool downFlag {
        get { if (disabled) return false; else return _downFlag; }
        set { _downFlag = value; }
    }
    protected bool leftFlag {
        get { if (disabled) return false; else return _leftFlag; }
        set { _leftFlag = value; }
    }
    protected bool rightFlag {
        get { if (disabled) return false; else return _rightFlag; }
        set { _rightFlag = value; }
    }
    public Stack<Controller> controlStack = new Stack<Controller>();
    public void Register(Controller controller) {
        if (controlStack.Contains(controller)) {
            Debug.LogWarning("controller already present in controlstack");
        } else {
            if (controlStack.Count > 0) {
                controlStack.Peek().LostControl(this);
            }
            ResetInput();
            controlStack.Push(controller);
        }
    }

    public void Deregister(Controller controller) {
        // Debug.Log(this + " deregistering controller");
        if (controlStack.Peek() == controller) {
            controlStack.Pop();
            if (controlStack.Count > 0) {
                controlStack.Peek().GainedControl(this);
            }
        } else {
            Debug.LogWarning("deregister called by non-controlling controller");
        }
    }
    public void SetDirectionFlag(DirectionEnum d, bool value, Controller controller) {
        if (Authenticate(controller)) {
            // ResetInput();
            switch (d) {
                case DirectionEnum.up:
                    upFlag = value;
                    break;
                case DirectionEnum.down:
                    downFlag = value;
                    break;
                case DirectionEnum.left:
                    leftFlag = value;
                    break;
                case DirectionEnum.right:
                    rightFlag = value;
                    break;
                case DirectionEnum.none:
                    upFlag = downFlag = leftFlag = rightFlag = value;
                    break;
            }
        }
    }
    public bool Authenticate(Controller controller) {
        return controlStack.Count > 0 && controlStack.Peek() == controller;
    }
    public string lastPressed = "right";
    private Vector2 _direction = Vector2.right;
    public Vector2 direction {
        get { return _direction; }
        set {
            _direction = value;
            // Debug.Log(_direction);
            directionAngle = Toolbox.Instance.ProperAngle(_direction.x, _direction.y);
            foreach (IDirectable directable in directables) {
                directable.DirectionChange(value);
            }
        }
    }
    public float directionAngle = 0;
    public HashSet<IDirectable> directables = new HashSet<IDirectable>();
    public bool fightMode;
    public bool disabled = false;
    public HitState hitState;
    public GameObject lastRightClicked;
    public Rigidbody2D myRigidBody;

    protected void ResetInput() {
        upFlag = false;
        downFlag = false;
        leftFlag = false;
        rightFlag = false;
    }
    public virtual void Awake() {
        myRigidBody = GetComponent<Rigidbody2D>();

        Toolbox.RegisterMessageCallback<MessageHitstun>(this, HandleHitStun);
        Toolbox.RegisterMessageCallback<MessageDirectable>(this, HandleDirectable);
        Toolbox.RegisterMessageCallback<MessageInventoryChanged>(this, HandleInventoryMessage);
        Toolbox.RegisterMessageCallback<MessageNoise>(this, HandleNoise);
    }
    public virtual void Start() {
        foreach (Component component in gameObject.GetComponentsInChildren<Component>()) {
            if (component is IDirectable) {
                directables.Add((IDirectable)component);
            }
        }
        UpdateDefaultInteraction();
    }
    public void HandleNoise(MessageNoise message) {
        // if (hitState >= HitState.stun || fightMode)
        //     return;

        // LookAtPoint(message.location);
    }
    public void HandleInventoryMessage(MessageInventoryChanged message) {
        // Debug.Log(gameObject.name + " updating default actions on inv change");
        UpdateDefaultInteraction();
    }
    public InteractionParam UpdateDefaultInteraction() {
        defaultInteraction = null;
        HashSet<InteractionParam> manualActions = Interactor.SelfOnSelfInteractions(gameObject);

        InteractionParam defaultButton = Interactor.GetDefaultAction(manualActions);
        if (defaultButton != null) {
            defaultInteraction = defaultButton;
        }
        return defaultInteraction;
    }
    void HandleHitStun(MessageHitstun message) {
        hitState = message.hitState;
    }
    void HandleDirectable(MessageDirectable message) {
        foreach (IDirectable directable in message.addDirectable) {
            directables.Add(directable);
            directable.DirectionChange(direction);
        }
        foreach (IDirectable directable in message.removeDirectable) {
            directables.Remove(directable);
        }
    }

    void Update() {
        if (hitState > 0) {
            ResetInput();
        }
        if (hitState > Controllable.HitState.none) {
            ResetInput();
            return;
        }
        if (rightFlag || leftFlag)
            lastPressed = "right";
        if (downFlag)
            lastPressed = "down";
        if (upFlag)
            lastPressed = "up";
        // update direction vector if speed is above a certain value
        if (myRigidBody.velocity.magnitude > 0.1) {
            SetDirection(myRigidBody.velocity);
            directionAngle = Toolbox.Instance.ProperAngle(direction.x, direction.y);
        }
    }
    protected virtual void SetDirection(Vector2 d) {
        d = d.normalized;
        if (d == Vector2.zero)
            return;
        direction = d;
    }
    protected virtual void LookAtPoint(Vector3 target) {
        ResetInput();
        Vector2 dif = (Vector2)target - (Vector2)gameObject.transform.position;
        SetDirection(dif);
    }
    public void SetDirection(Vector2 d, Controller controller) {
        if (Authenticate(controller))
            SetDirection(d);
    }
    public void LookAtPoint(Vector3 target, Controller controller) {
        if (Authenticate(controller)) {
            ResetInput();
            Vector2 dif = (Vector2)target - (Vector2)gameObject.transform.position;
            SetDirection(dif);
        }
    }
    public void ShootPressed(Controller controller) {
        if (Authenticate(controller)) {
            if (fightMode) {
                Toolbox.Instance.SendMessage(gameObject, this, new MessagePunch());
            } else {
                if (defaultInteraction != null) {
                    defaultInteraction.DoAction();
                }
                if (InputController.Instance.focus == this) {
                    UINew.Instance.UpdateTopActionButtons();
                }
            }
        }

    }
    public void ShootHeld(Controller controller) {
        if (Authenticate(controller)) {
            if (defaultInteraction != null && defaultInteraction.interaction.continuous) {
                defaultInteraction.DoAction();
            }
        }
    }
    protected virtual void ToggleFightMode() {
        fightMode = !fightMode;
    }
    public virtual void ToggleFightMode(Controller controller) {
        if (Authenticate(controller)) {
            ToggleFightMode();
        }
    }
}
