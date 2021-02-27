using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using AI;

public class CutsceneScorpion : Cutscene {
    private enum State { none, spawn, walk, slew, dialogue };
    private State state;
    private Doorway doorway;
    private float timer;
    private RoutineWalkToPoint walkRoutine;
    private GameObject greaserPrefab;
    private List<GameObject> greasers = new List<GameObject>();
    private CameraControl camControl;
    Speech speech;
    private int numSwitchblades = 0;
    private string sceneName;
    public CutsceneScorpion(string cutsceneName) {
        this.sceneName = cutsceneName;
    }
    public static Doorway FindValidDoorway() {
        foreach (Doorway doorway in GameObject.FindObjectsOfType<Doorway>()) {
            if (doorway.name == "door" && !doorway.spawnPoint && doorway.entryID != 420) {
                return doorway;
            }
        }
        return null;
    }
    public override void Configure() {
        configured = true;
        camControl = GameObject.FindObjectOfType<CameraControl>();
        state = State.spawn;
        doorway = FindValidDoorway();
        greaserPrefab = null;
        switch (sceneName) {
            default:
            case "1950s Greaser Beatdown":
                greaserPrefab = Resources.Load("prefabs/greaser") as GameObject;
                break;
            case "Combat II":
                greaserPrefab = Resources.Load("prefabs/Bruiser") as GameObject;
                break;
            case "Combat III":
                greaserPrefab = Resources.Load("prefabs/BillGhost") as GameObject;
                break;
            case "Combat IV":
                greaserPrefab = Resources.Load("prefabs/Tharr") as GameObject;
                break;
        }
        MusicController.Instance.EnqueueMusic(new MusicGreaser());
    }
    private bool SufficientGreasers() {
        switch (sceneName) {
            default:
            case "1950s Greaser Beatdown":
                return greasers.Count >= 5;
            case "Combat II":
                return greasers.Count >= 2;
            case "Combat III":
            case "Combat IV":
                return greasers.Count >= 1;
        }

    }
    public override void Update() {
        timer += Time.deltaTime;
        if (state == State.spawn) {
            if (timer > 0.3f) {
                timer = 0f;
                CutsceneManager.Instance.StartCoroutine(walkCoroutine());
                if (SufficientGreasers()) {
                    state = State.slew;
                }
            }
        }
        if (state == State.slew) {
            if (timer > 2f) {
                DialogueMenu menu = speech.SpeakWith();
                menu.menuClosed += MenuWasClosed;
                timer = 0;
                state = State.dialogue;
            }
        }
    }
    private GameObject SpawnGreaser() {
        return GameObject.Instantiate(greaserPrefab, doorway.transform.position, Quaternion.identity) as GameObject;
    }
    private IEnumerator walkCoroutine() {
        // Vector2 random = Random.insideUnitCircle.normalized;
        // random.y = -1 * Mathf.Abs(random.y);
        // Vector2 target = (Vector2)doorway.transform.position + random;

        Vector2 target = (Vector2)doorway.transform.position;
        switch (greasers.Count) {
            default:
            case 0:
                target += new Vector2(-1, -1);
                break;
            case 1:
                target += new Vector2(-1, -1);
                break;
            case 2:
                target += new Vector2(1, -1);

                break;
            case 3:
                target += new Vector2(1, -1);

                break;
            case 4:
                target += new Vector2(0, -1);

                break;
        }

        GameObject greaser = SpawnGreaser();
        greasers.Add(greaser);
        if (greasers.Count == 1) {
            camControl.focus = greaser;
        }

        DecisionMaker ai = greaser.GetComponent<DecisionMaker>();
        ai.enabled = false;

        doorway.PlayEnterSound();
        if (speech == null) {
            speech = greaser.GetComponent<Speech>();
        }
        // Controllable control = greaser.GetComponent<Controllable>();
        using (Controller control = new Controller(greaser)) {
            Ref<Vector2> walkRef = new Ref<Vector2>(target);
            walkRoutine = new RoutineWalkToPoint(greaser, control, walkRef, 0.1f);
            float walkTime = 0f;
            float targTime = Random.Range(0.4f, 0.6f);
            while (walkTime < targTime) {
                walkRoutine.Update();
                walkTime += Time.deltaTime;
                yield return null;
            }
            control.ResetInput();
        }
    }

    public void MenuWasClosed() {
        camControl.focus = GameManager.Instance.playerObject;
        foreach (GameObject greaser in greasers) {
            DecisionMaker ai = greaser.GetComponent<DecisionMaker>();
            ai.enabled = true;

            if (sceneName == "1950s Greaser Beatdown")
                if ((Random.Range(0f, 1f) < 0.15f) || (greasers.Count == 0 && numSwitchblades == 0)) {
                    GameObject switchBlade = GameObject.Instantiate(Resources.Load("prefabs/switchblade"), greaser.transform.position, Quaternion.identity) as GameObject;
                    Inventory inv = greaser.GetComponent<Inventory>();
                    Pickup pickup = switchBlade.GetComponent<Pickup>();
                    inv.GetItem(pickup);
                    numSwitchblades += 1;
                }

            Awareness awareness = greaser.GetComponent<Awareness>();
            foreach (GameObject otherGreaser in greasers) {
                PersonalAssessment assessment = awareness.FormPersonalAssessment(otherGreaser);
                if (assessment != null)
                    assessment.status = PersonalAssessment.friendStatus.friend;
            }
        }
        UINew.Instance.RefreshUI(active: true);
        complete = true;
    }
}