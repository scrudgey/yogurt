using UnityEngine;
using System.Collections.Generic;
public class CutsceneDancingGod : Cutscene {
    enum State { walkTo, jump, god, replace }
    State state;
    Godhead godhead;
    DancingGod dancingGod;
    Transform standPoint;
    CameraControl cam;
    float timer;
    PhysicalBootstrapper item;
    GameObject pankrator;
    public void Configure(PhysicalBootstrapper item) {
        this.item = item;
        configured = true;
        GameObject god = GameObject.Instantiate(Resources.Load("godhead")) as GameObject;
        dancingGod = GameObject.FindObjectOfType<DancingGod>();
        cam = GameObject.FindObjectOfType<CameraControl>();

        standPoint = GameObject.Find("godStandPoint").transform;
        godhead = god.GetComponent<Godhead>();
        godhead.transform.position = godhead.startPoint;
        godhead.gameObject.SetActive(false);
        godhead.item = item;
        UINew.Instance.RefreshUI(active: false);
        item.gameObject.SetActive(false);

        pankrator = GameObject.Find("Pankrator");
        pankrator.GetComponent<Controllable>().control = Controllable.ControlType.none;
    }
    public override void Update() {
        timer += Time.deltaTime;
        switch (state) {
            case State.walkTo:
                if (timer > 3f) {
                    EnterJumpState();
                }
                UpdateWalkTo();
                break;
            case State.jump:
                UpdateJump();
                break;
            case State.god:
                break;
            case State.replace:
                UpdateReplace();
                break;
            default:
                break;
        }
    }
    public override void CleanUp() {
        UINew.Instance.RefreshUI(active: true);
    }
    void UpdateWalkTo() {
        if (Mathf.Abs(Controller.Instance.focus.transform.position.x - standPoint.position.x) < 0.1) {
            EnterJumpState();
        } else {
            doWalk();
        }
    }
    void doWalk() {
        if (Mathf.Abs(Controller.Instance.focus.transform.position.x - standPoint.position.x) > 0.1) {
            if (Controller.Instance.focus.transform.position.x > standPoint.position.x) {
                Controller.Instance.focus.leftFlag = true;
                Controller.Instance.focus.rightFlag = false;
            } else if (Controller.Instance.focus.transform.position.x < standPoint.position.x) {
                Controller.Instance.focus.rightFlag = true;
                Controller.Instance.focus.leftFlag = false;
            }
        }
    }
    void EnterJumpState() {
        Controller.Instance.focus.ResetInput();
        Controller.Instance.state = Controller.ControlState.cutscene;
        state = State.jump;
        timer = 0f;
        cam.maxSize = 0.85f;
        cam.offset = new Vector3(0, 0.4f, 0);
    }
    void UpdateJump() {
        dancingGod.enabled = false;
        if (timer < 1.5f) {
            doWalk();
        } else if (timer > 1.5f) {
            Controller.Instance.focus.ResetInput();
            dancingGod.transform.localPosition = Vector3.Lerp(dancingGod.transform.localPosition, dancingGod.jumpUpPosition, 0.02f);
        }
        if (timer > 4f) {
            Controller.Instance.focus.ResetInput();
            Controller.Instance.state = Controller.ControlState.cutscene;
            state = State.god;
            timer = 0f;
            godhead.gameObject.SetActive(true);
            godhead.onQuit += GodHeadCallback;
        }
    }
    void GodHeadCallback() {
        Controller.Instance.state = Controller.ControlState.normal;
        dancingGod.enabled = true;
        timer = 0f;
        state = State.replace;
    }
    void UpdateReplace() {
        cam.maxSize = 0.65f;
        cam.offset = new Vector3(0, 0.2f, 0);
        dancingGod.transform.localPosition = Vector3.Lerp(dancingGod.transform.localPosition, dancingGod.initPosition, 0.1f);
        if (Vector2.Distance(dancingGod.transform.localPosition, dancingGod.initPosition) < 0.01f) {
            complete = true;
            dancingGod.numberOfJumps = 0;
            dancingGod.FinishJump();
            UINew.Instance.RefreshUI(active: true);
        }
        if (timer > 2f) {
            complete = true;
            UINew.Instance.RefreshUI(active: true);
            cam.maxSize = 0.65f;
            cam.offset = new Vector3(0, 0.2f, 0);
            pankrator.GetComponent<Controllable>().control = Controllable.ControlType.AI;
        }
    }
}
// sfx