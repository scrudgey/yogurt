using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class InputController : Singleton<InputController> {
    MyControls controls;
    public enum ControlState {
        normal,
        inMenu,
        waitForMenu,
        commandSelect,
        swearSelect,
        insultSelect,
        cutscene,
        hypnosisSelect
    }
    private Controllable _focus;
    public Controllable focus {
        get {
            return _focus;
        }
        set {
            _focus = value;
            if (_focus != null) {
                focusHurtable = _focus.GetComponent<Hurtable>();
                controller.Register(_focus);
            }
        }
    }
    public Controller controller = new Controller();
    public Hurtable focusHurtable;
    private bool _suspendInput = false;
    public bool suspendInput {
        get {
            return _suspendInput;
        }
        set {
            _suspendInput = value;
            // if (focus)
            controller.ResetInput();
        }
    }
    private GameObject lastLeftClicked;
    public static List<string> forbiddenTags = new List<string>() {
        "fire",
        "sightcone",
        "table",
        "background",
        "occurrenceFlag",
        "occurrenceSound",
        "footprint",
        "zombieSpawnZone"
        };
    public static List<ControlState> selectionStates = new List<ControlState>(){InputController.ControlState.swearSelect,
                                                                                InputController.ControlState.insultSelect,
                                                                                InputController.ControlState.hypnosisSelect,
                                                                                InputController.ControlState.commandSelect};
    private ControlState _state;
    public ControlState state {
        get { return _state; }
        set {
            ControlState previousState = _state;
            // Debug.Log("changing state to "+value.ToString());
            _state = value;
            ChangeState(previousState);
        }
    }
    public GameObject commandTarget;
    public Controller commandController;
    public Controller commandPlayerController;
    public bool doCommand;
    public InteractionParam commandAct = null;
    public ActionButtonScript.buttonType commandButtonType = ActionButtonScript.buttonType.none;
    public Vector2 inputVector;
    public bool firePressedHeld;
    public bool firePressedThisFrame;

    // public bool escapeHeld;
    public bool escaprePressedThisFrame;
    public bool leftClickHeld;
    public bool leftClickedThisFrame;
    void Awake() {
        controls = new MyControls();

        // enable controls
        controls.Player.Move.Enable();
        controls.Player.Fire.Enable();
        controls.Player.InteractWith.Enable();
        controls.Player.Escape.Enable();

        // Move
        controls.Player.Move.performed += ctx => inputVector = ctx.ReadValue<Vector2>();

        // Fire
        controls.Player.Fire.performed += ctx => {
            firePressedThisFrame = ctx.ReadValueAsButton();
            firePressedHeld = ctx.ReadValueAsButton();
        };

        // Left click
        controls.Player.InteractWith.performed += ctx => {
            leftClickedThisFrame = ctx.ReadValueAsButton();
            leftClickHeld = ctx.ReadValueAsButton();
        };

        // Escape
        controls.Player.Escape.performed += ctx => {
            escaprePressedThisFrame = ctx.ReadValueAsButton();
        };

        // Button up
        controls.Player.Fire.canceled += _ => firePressedHeld = false;
        controls.Player.InteractWith.canceled += _ => leftClickHeld = false;
        controls.Player.Move.canceled += _ => inputVector = Vector2.zero;
    }
    void ChangeState(ControlState previousState) {
        // TODO: code for transitioning between states
        // if (focus) {
        // focus.ResetInput();
        // }
        controller.ResetInput();
        UINew.Instance.ClearWorldButtons();
        UINew.Instance.SetActionText("");
        ResetLastLeftClicked();
        if (previousState == ControlState.inMenu || previousState == ControlState.waitForMenu)
            UINew.Instance.RefreshUI(active: true);

        if (previousState == ControlState.commandSelect) {
            // if we've aborted command state without success
            // reset command character and target control
            if (!doCommand)
                ResetCommandState();
        }
        if (state == ControlState.commandSelect) {
            commandController = new Controller(commandTarget);
            commandPlayerController = new Controller(InputController.Instance.focus);
        }
    }
    public void ResetCommandState() {
        if (commandTarget != null) {
            // Controllable targetControl = commandTarget.GetComponent<Controllable>();
            // targetControl.control = Controllable.ControlType.AI;
        }
        if (commandController != null) {
            commandController.Deregister();
        }
        if (commandPlayerController != null) {
            commandPlayerController.Deregister();
        }
        commandTarget = null;
        // reset command state
        suspendInput = false;
        UINew.Instance.RefreshUI(active: true);
        commandAct = null;
        commandButtonType = ActionButtonScript.buttonType.none;
        doCommand = false;
    }
    public void MenuClosedCallback() {
        if (state == ControlState.inMenu || state == ControlState.waitForMenu) {
            state = ControlState.normal;
        }
        // if a command action is specified, we should process it after the dialogue menu is closed.
        if (doCommand) {
            DoCommand();
        }
    }
    void Update() {
        if (escaprePressedThisFrame) {
            // TODO: exit command states
            if (state != ControlState.cutscene) {
                if (selectionStates.Contains(state)) {
                    state = ControlState.normal;
                    UINew.Instance.RefreshUI(active: true);
                } else {
                    if (UINew.Instance.activeMenu != null) {
                        UINew.Instance.CloseActiveMenu();
                    } else {
                        UINew.Instance.ShowMenu(UINew.MenuType.escape);
                    }
                }
            } else {
                CutsceneManager.Instance.EscapePressed();
            }
        }
        escaprePressedThisFrame = false;

        if (state != ControlState.normal & state != ControlState.commandSelect & state != ControlState.hypnosisSelect & state != ControlState.insultSelect & state != ControlState.swearSelect)
            return;
        if (focus != null & !suspendInput) {
            controller.ResetInput();
            if (inputVector.y > 0)
                controller.upFlag = true;
            if (inputVector.y < 0)
                controller.downFlag = true;
            if (inputVector.x < 0)
                controller.leftFlag = true;
            if (inputVector.x > 0)
                controller.rightFlag = true;
            //Fire key 
            if (firePressedThisFrame) {
                controller.ShootPressed();
            }
            if (firePressedHeld) {
                controller.ShootHeld();
            }
        }

        // left click
        if (leftClickedThisFrame) {
            LeftClick();
        }
        firePressedThisFrame = false;
        leftClickedThisFrame = false;
    }
    SpriteRenderer WhichIsFirst(SpriteRenderer one, SpriteRenderer two) {
        if (one == null && two == null) {
            return one;
        }
        if (one == null && two != null) {
            return two;
        }
        if (one != null && two == null) {
            return one;
        }
        if (one.sortingLayerID > two.sortingLayerID) {
            return one;
        }
        if (two.sortingLayerID > one.sortingLayerID) {
            return two;
        }
        if (one.sortingOrder > two.sortingOrder) {
            return one;
        }
        if (two.sortingOrder > one.sortingOrder) {
            return two;
        }
        return one;
    }
    public GameObject GetFrontObject(RaycastHit2D[] hits, bool debug = false) {
        if (debug)
            Debug.Log("*******************");
        List<GameObject> candidates = new List<GameObject>();
        foreach (RaycastHit2D hit in hits) {
            if (debug)
                Debug.Log(hit.collider.gameObject);
            if (hit.collider != null && !forbiddenTags.Contains(hit.collider.tag)) {
                candidates.Add(hit.collider.gameObject);
            }
        }
        if (candidates.Count == 0)
            return null;
        if (candidates.Count == 1)
            return candidates[0];
        GameObject front = null;
        SpriteRenderer frontRenderer = null;
        foreach (GameObject candidate in candidates) {
            if (front == null) {
                front = candidate;
                frontRenderer = candidate.GetComponent<SpriteRenderer>();
                continue;
            }
            frontRenderer = WhichIsFirst(frontRenderer, candidate.GetComponent<SpriteRenderer>());
            if (frontRenderer != null)
                front = frontRenderer.gameObject;
        }
        return front;
    }
    public GameObject GetBaseInteractive(Transform target) {
        Transform currentChild = target;
        if (currentChild.tag == "footprint") {
            Physical physical = target.GetComponent<Physical>();
            if (physical) {
                if (physical.objectBody != null)
                    return physical.objectBody.gameObject;
            }
        }
        Controllable baseControllable = target.GetComponentInParent<Controllable>();
        if (baseControllable != null) {
            return baseControllable.gameObject;
        }
        while (true) {
            if (currentChild.tag == "Physical") {
                return currentChild.gameObject;
            }
            if (currentChild.GetComponent<ItemBoundary>()) {
                return currentChild.gameObject;
            }
            if (currentChild.parent == null) {
                return currentChild.gameObject;
            }
            currentChild = currentChild.parent;
        }
    }
    void LeftClick() {
        if (state == ControlState.inMenu || state == ControlState.waitForMenu)
            return;
        if (focus == null)
            return;
        if (focus.hitState >= Controllable.HitState.stun)
            return;

        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector2.zero).OrderBy(h => h.collider.gameObject.name).ToArray();

        switch (state) {
            case ControlState.swearSelect:
                foreach (RaycastHit2D hit in hits) {
                    if (hit.collider != null && !forbiddenTags.Contains(hit.collider.tag)) {
                        state = ControlState.normal;
                        MessageSpeech message = new MessageSpeech("");
                        message.swearTarget = hit.collider.gameObject;
                        Toolbox.Instance.SendMessage(focus.gameObject, this, message);
                        UINew.Instance.SetActionText("");
                    }
                }
                return;
            case ControlState.insultSelect:
                GameObject top = InputController.Instance.GetFrontObject(hits);
                if (top != null) {
                    state = ControlState.normal;
                    GameObject target = InputController.Instance.GetBaseInteractive(top.transform);
                    Speech speech = focus.GetComponent<Speech>();
                    if (speech) {
                        speech.InsultMonologue(target);
                    }
                    UINew.Instance.SetActionText("");
                }
                return;
            case ControlState.hypnosisSelect:
                GameObject hypnoTop = InputController.Instance.GetFrontObject(hits);
                if (hypnoTop != null) {
                    state = ControlState.normal;
                    GameObject target = InputController.Instance.GetBaseInteractive(hypnoTop.transform);
                    Intrinsics targetIntrinsics = Toolbox.GetOrCreateComponent<Intrinsics>(target);
                    // Intrinsics targetIntrinsics = target.GetComponent<Intrinsics>();
                    if (targetIntrinsics.NetBuffs()[BuffType.clearHeaded].boolValue) {
                        UINew.Instance.SetActionText("");
                        MessageSpeech message = new MessageSpeech("Something prevents my hypnotic power!");
                        Toolbox.Instance.SendMessage(GameManager.Instance.playerObject, this, message);
                        return;
                    }
                    Controllable controllable = target.GetComponent<Controllable>();
                    GameObject hypnosisEffect = Instantiate(Resources.Load("prefabs/fx/hypnosisEffect"), GameManager.Instance.playerObject.transform.position, Quaternion.identity) as GameObject;
                    HypnosisEffect fx = hypnosisEffect.GetComponent<HypnosisEffect>();
                    fx.target = target;

                    if (controllable) {
                        if (controllable.hitState < Controllable.HitState.unconscious)
                            GameManager.Instance.SetFocus(target);
                    }
                    UINew.Instance.SetActionText("");
                }
                return;
            default:
                break;
        }
        // IsPointerOverGameObject is required here to exclude clicks if we are hovering over a UI element.
        // this may or may not cause problems down the road, but I'm unsure how else to do this.
        // NOTE: if an overlapping UI is causing problems, add a layout group and uncheck "blocks raycast"
        if (state == ControlState.normal || state == ControlState.commandSelect) {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
                GameObject top = GetFrontObject(hits, debug: false);
                if (top != null) {
                    Clicked(GetBaseInteractive(top.transform), top);
                }
            }
        }
    }
    public void Clicked(GameObject clicked, GameObject clickSite) {
        // Debug.Log("clicked "+clicked.name + " last: "+lastLeftClicked);
        if (lastLeftClicked == clicked) {
            UINew.Instance.ClearWorldButtons();
            lastLeftClicked = null;
        } else {
            lastLeftClicked = clicked;
            GameObject actor = focus.gameObject;
            if (commandTarget != null)
                actor = commandTarget;
            if (clicked.transform.IsChildOf(actor.transform) || clicked == actor) {
                UINew.Instance.ShowActionsForPlayerClick(actor);
            } else {
                UINew.Instance.ShowActionsForWorldClick(lastLeftClicked, clickSite);
            }
        }
    }
    public void ResetLastLeftClicked() {
        lastLeftClicked = null;
    }
    public bool InteractionIsWithinRange(Interaction i) {
        // TODO: this all should use something other than lastleftclicked, for persistent buttons of sorts.
        // using i.action.parent doesn't work, because some actions are sourced from player gameobject, not "target", whatever it is
        if (i == null || lastLeftClicked == null)
            return false;
        if (i.unlimitedRange)
            return true;
        Transform focusTransform = focus.transform;
        Collider2D[] focusColliders = null;
        if (commandTarget != null) {
            focusColliders = commandTarget.GetComponentsInChildren<Collider2D>();
            focusTransform = commandTarget.transform;
        } else {
            focusColliders = focus.GetComponentsInChildren<Collider2D>();
        }
        Collider2D clickedCollider = lastLeftClicked.GetComponent<Collider2D>();
        float dist = float.MaxValue;
        if (clickedCollider != null && focusColliders.Length > 0) {
            foreach (Collider2D focusCollider in focusColliders) {
                if (forbiddenTags.Contains(focusCollider.tag))
                    continue;
                if (focusCollider.enabled == false)
                    continue;
                if (clickedCollider != focusCollider)
                    dist = Mathf.Min(dist, clickedCollider.Distance(focusCollider).distance);
                else dist = 0;
            }
        }
        if (dist == float.MaxValue) {
            dist = Vector3.SqrMagnitude(lastLeftClicked.transform.position - focusTransform.position);
        }
        if (dist < i.range) {
            return true;
        } else {
            return false;
        }
    }
    public void DoCommand() {
        // Hand action
        if (commandButtonType == ActionButtonScript.buttonType.none) {
            commandAct.DoAction();
        } else if (commandButtonType == ActionButtonScript.buttonType.Punch) {
            controller.ShootPressed();
        } else {
            Inventory inventory = commandTarget.GetComponent<Inventory>();
            Controllable controllable = commandTarget.GetComponent<Controllable>();
            inventory.ButtonCallback(commandButtonType);
        }
        ResetCommandState();
        commandAct = null;
    }
    public void ButtonClicked(ActionButtonScript button) {
        // normal click
        if (state != ControlState.commandSelect) {
            if (button.bType == ActionButtonScript.buttonType.Action) {
                if (InteractionIsWithinRange(button.action) || button.manualAction) {
                    button.action.DoAction(button.parameters);
                    if (!button.action.dontWipeInterface) {
                        UINew.Instance.RefreshUI(active: true);
                        ResetLastLeftClicked();
                    }
                }
            } else {
                button.HandAction();
                ResetLastLeftClicked();
            }
            UINew.Instance.SetActionText("");
            GUI.FocusControl("none");
            return;
        }
        // handle commands clicked
        if (button.bType == ActionButtonScript.buttonType.Action) {
            if (InteractionIsWithinRange(button.action) || button.manualAction) {
                commandAct = new InteractionParam(button.action, button.parameters);
                DialogueCommand().CommandCallback(commandAct);
            }
        } else {
            commandButtonType = button.bType;
            DialogueCommand().HandCommandCallback(button.bType);
        }
        UINew.Instance.SetActionText("");
        GUI.FocusControl("none");
    }
    private DialogueMenu DialogueCommand() {
        doCommand = true;
        UINew.Instance.RefreshUI(active: true);
        ResetLastLeftClicked();
        GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.dialogue);
        DialogueMenu dialogue = menuObject.GetComponent<DialogueMenu>();
        return dialogue;
    }
}
