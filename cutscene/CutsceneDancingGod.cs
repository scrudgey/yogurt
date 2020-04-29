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
    Controller pankratorController;
    Controller playerController;
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
        pankratorController = new Controller(pankrator);
        playerController = new Controller(InputController.Instance.focus);
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
        if (Mathf.Abs(InputController.Instance.focus.transform.position.x - standPoint.position.x) < 0.1) {
            EnterJumpState();
        } else {
            doWalk();
        }
    }
    void doWalk() {
        if (Mathf.Abs(InputController.Instance.focus.transform.position.x - standPoint.position.x) > 0.1) {
            if (InputController.Instance.focus.transform.position.x > standPoint.position.x) {
                playerController.leftFlag = true;
                playerController.rightFlag = false;
            } else if (InputController.Instance.focus.transform.position.x < standPoint.position.x) {
                playerController.rightFlag = true;
                playerController.leftFlag = false;
            }
        }
    }
    void EnterJumpState() {
        playerController.ResetInput();
        InputController.Instance.state = InputController.ControlState.cutscene;
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
            playerController.ResetInput();
            dancingGod.transform.localPosition = Vector3.Lerp(dancingGod.transform.localPosition, dancingGod.jumpUpPosition, 0.02f);
        }
        if (timer > 4f) {
            playerController.ResetInput();
            InputController.Instance.state = InputController.ControlState.cutscene;
            state = State.god;
            timer = 0f;
            godhead.gameObject.SetActive(true);
            godhead.onQuit += GodHeadCallback;
        }
    }
    void GodHeadCallback() {
        InputController.Instance.state = InputController.ControlState.normal;
        dancingGod.enabled = true;
        timer = 0f;
        state = State.replace;
    }
    void UpdateReplace() {
        cam.maxSize = 0.65f;
        cam.offset = new Vector3(0, 0.2f, 0);
        dancingGod.transform.localPosition = Vector3.Lerp(dancingGod.transform.localPosition, dancingGod.initPosition, 0.1f);
        if (Vector2.Distance(dancingGod.transform.localPosition, dancingGod.initPosition) < 0.01f) {
            pankratorController.Deregister();
            playerController.Deregister();
            complete = true;
            dancingGod.numberOfJumps = 0;
            dancingGod.FinishJump();
            UINew.Instance.RefreshUI(active: true);
        }
        if (timer > 2f) {
            pankratorController.Deregister();
            playerController.Deregister();
            complete = true;
            UINew.Instance.RefreshUI(active: true);
            cam.maxSize = 0.65f;
            cam.offset = new Vector3(0, 0.2f, 0);
        }
    }
}
// sfx