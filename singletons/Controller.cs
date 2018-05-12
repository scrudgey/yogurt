using UnityEngine;
using System.Linq;
using System.Collections.Generic;
public class Controller : Singleton<Controller> {
    public enum ControlState {
        normal, inMenu, waitForMenu, commandSelect, swearSelect, insultSelect, cutscene, hypnosisSelect
    }
    private Controllable _focus;
    public Controllable focus {
        get {
            return _focus;
        }
        set {
            _focus = value;
            focusHurtable = _focus.GetComponent<Hurtable>();
        }
    }
    public Hurtable focusHurtable;
    private bool _suspendInput = false;
    public bool suspendInput {
        get {
            return _suspendInput;
        }
        set {
            _suspendInput = value;
            if (focus)
                focus.ResetInput();
        }
    }
    private GameObject lastLeftClicked;
    public static List<string> forbiddenColliders = new List<string>() { "fire", "sightcone", "table", "background", "occurrenceFlag" };
    public static List<ControlState> selectionStates = new List<ControlState>(){Controller.ControlState.swearSelect,
                                                                                Controller.ControlState.insultSelect,
                                                                                Controller.ControlState.hypnosisSelect};
    public string message = "smoke weed every day";
    private ControlState _state;
    public ControlState state {
        get { return _state; }
        set {
            ControlState previousState = _state;
            _state = value;
            ChangeState(previousState);
        }
    } 
    public GameObject commandTarget;
    public bool doCommand;
    public Interaction commandAct = null;
    public ActionButtonScript.buttonType commandButtonType = ActionButtonScript.buttonType.none;
    void ChangeState(ControlState previousState) {
        // TODO: code for transitioning between states
        if (focus) {
            focus.ResetInput();
        }
        UINew.Instance.ClearWorldButtons();
        ResetLastLeftClicked();
        if (previousState == ControlState.inMenu || previousState == ControlState.waitForMenu)
            UINew.Instance.RefreshUI(active: true);

        if (previousState == ControlState.commandSelect) {
            // if we've aborted command state without success
            // reset command character and target control
            // Debug.Log("disabling command target control");
            Controllable commandTargetControl = commandTarget.GetComponent<Controllable>();
            commandTargetControl.control = Controllable.ControlType.AI;
            if (!doCommand)
                ResetCommandState();
        }
        if (state == ControlState.commandSelect){
            Controllable commandTargetControl = commandTarget.GetComponent<Controllable>();
            commandTargetControl.control = Controllable.ControlType.none;
        }
    }
    void Update() {
        if (Input.GetButtonDown("Cancel")) {
            // TODO: exit command states
            if (state != ControlState.cutscene) {
                if (selectionStates.Contains(state) || state == ControlState.commandSelect) {
                    state = ControlState.normal;
                    UINew.Instance.RefreshUI(active: true);
                } else {
                    UINew.Instance.ShowMenu(UINew.MenuType.escape);
                }
            } else {
                CutsceneManager.Instance.EscapePressed();
            }
        }
        if (state != ControlState.normal)
            return;
        if (focus != null & !suspendInput) {
            focus.ResetInput();
            if (Input.GetAxis("Vertical") > 0)
                focus.upFlag = true;
            if (Input.GetAxis("Vertical") < 0)
                focus.downFlag = true;
            if (Input.GetAxis("Horizontal") < 0)
                focus.leftFlag = true;
            if (Input.GetAxis("Horizontal") > 0)
                focus.rightFlag = true;
            //Fire key 
            if (Input.GetButtonDown("Fire1")) {
                focus.shootPressedFlag = true;
            }
            if (Input.GetButton("Fire1")) {
                focus.shootHeldFlag = true;
            }
        }
    }
    void OnGUI() {
        if (Event.current.isMouse) {
            // right click
            if (Input.GetMouseButtonDown(1)) {
                RightClick();
            }
            // left click
            if (Input.GetMouseButtonDown(0)) {
                LeftClick();
            }
            // mouse up event
            // if (Input.GetMouseButtonUp(1)){
            // 	focus.MouseUp();
            // }
        }
    }
    void RightClick() {
        //detect if we clicked anything
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider != null && !forbiddenColliders.Contains(hit.collider.tag)) {
                focus.lastRightClicked = hit.collider.gameObject;
            }
        }
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
    public GameObject GetFrontObject(RaycastHit2D[] hits) {
        List<GameObject> candidates = new List<GameObject>();
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider != null && !forbiddenColliders.Contains(hit.collider.tag)) {
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

        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero).OrderBy(h => h.collider.gameObject.name).ToArray();

        switch (state) {
            case ControlState.swearSelect:
                foreach (RaycastHit2D hit in hits) {
                    if (hit.collider != null && !forbiddenColliders.Contains(hit.collider.tag)) {
                        state = ControlState.normal;
                        MessageSpeech message = new MessageSpeech();
                        message.swearTarget = hit.collider.gameObject;
                        Toolbox.Instance.SendMessage(focus.gameObject, this, message);
                        UINew.Instance.SetActionText("");
                    }
                }
                return;
            case ControlState.insultSelect:
                GameObject top = Controller.Instance.GetFrontObject(hits);
                if (top != null) {
                    state = ControlState.normal;
                    GameObject target = Controller.Instance.GetBaseInteractive(top.transform);
                    Speech speech = focus.GetComponent<Speech>();
                    if (speech) {
                        speech.Insult(target);
                    }
                    UINew.Instance.SetActionText("");
                }
                return;
            case ControlState.hypnosisSelect:
                GameObject hypnoTop = Controller.Instance.GetFrontObject(hits);
                if (hypnoTop != null) {
                    state = ControlState.normal;
                    GameObject target = Controller.Instance.GetBaseInteractive(hypnoTop.transform);
                    Controllable controllable = target.GetComponent<Controllable>();

                    GameObject hypnosisEffect = Instantiate(Resources.Load("prefabs/fx/hypnosisEffect"), GameManager.Instance.playerObject.transform.position, Quaternion.identity) as GameObject;
                    HypnosisEffect fx = hypnosisEffect.GetComponent<HypnosisEffect>();
                    fx.target = target;

                    if (controllable) {
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
                GameObject top = GetFrontObject(hits);
                if (top != null) {
                    Clicked(GetBaseInteractive(top.transform), top);
                }
            }
        }
    }
    public void Clicked(GameObject clicked, GameObject clickSite) {
        if (lastLeftClicked == clicked) {
            // TODO: fix this conditional!
            UINew.Instance.ClearWorldButtons();
            lastLeftClicked = null;
        } else {
            lastLeftClicked = clicked;
            GameObject actor = focus.gameObject;
            if (commandTarget != null)
                actor = commandTarget;
            if (clicked.transform.IsChildOf(actor.transform) || clicked == actor) {
                Inventory inventory = actor.GetComponent<Inventory>();
                if (inventory)
                    if (inventory.holding)
                        UINew.Instance.DisplayHandActions(inventory);
            } else {
                UINew.Instance.SetClickedActions(lastLeftClicked, clickSite);
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
        if (i.limitless)
            return true;
        float dist = Vector3.SqrMagnitude(lastLeftClicked.transform.position - focus.transform.position);
        if (dist < i.range) {
            return true;
        } else {
            return false;
        }
    }
    public void ResetCommandState() {
        // Debug.Log("resetting command state");
        if (commandTarget != null) {
            Controllable targetControl = commandTarget.GetComponent<Controllable>();
            targetControl.control = Controllable.ControlType.AI;
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
        if (doCommand) {
            if (commandButtonType != ActionButtonScript.buttonType.none) {
                Inventory inventory = commandTarget.GetComponent<Inventory>();
                Controllable controllable = commandTarget.GetComponent<Controllable>();
                switch (commandButtonType) {
                    case ActionButtonScript.buttonType.Drop:
                        inventory.DropItem();
                        UINew.Instance.ClearWorldButtons();
                        break;
                    case ActionButtonScript.buttonType.Throw:
                        inventory.ThrowItem();
                        UINew.Instance.ClearWorldButtons();
                        break;
                    case ActionButtonScript.buttonType.Stash:
                        inventory.StashItem(inventory.holding.gameObject);
                        UINew.Instance.ClearWorldButtons();
                        UINew.Instance.UpdateInventoryButton(inventory);
                        break;
                    case ActionButtonScript.buttonType.Punch:
                        controllable.shootPressedFlag = true;
                        break;
                    default:
                        break;
                }
            } else {    
                commandAct.DoAction();
            }
            ResetCommandState();
            commandAct = null;
        }
        
    }
}
