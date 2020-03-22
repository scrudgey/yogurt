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
    Speech speech;
    private int numSwitchblades = 0;
    public override void Configure() {
        configured = true;
        state = State.spawn;
        foreach (Doorway door in GameObject.FindObjectsOfType<Doorway>()) {
            if (!door.spawnPoint && door.entryID != 420) {
                doorway = door;
            }
        }
        greaserPrefab = Resources.Load("prefabs/greaser") as GameObject;
    }
    public override void Update() {
        timer += Time.deltaTime;
        if (state == State.spawn) {
            if (timer > 0.3f) {
                timer = 0f;
                CutsceneManager.Instance.StartCoroutine(walkCoroutine());
                if (greasers.Count >= 5) {
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
    private IEnumerator walkCoroutine() {
        Vector2 random = Random.insideUnitCircle.normalized;
        random.y = -1 * Mathf.Abs(random.y);
        Vector2 target = (Vector2)doorway.transform.position + random;

        GameObject greaser = GameObject.Instantiate(greaserPrefab, doorway.transform.position, Quaternion.identity) as GameObject;
        greasers.Add(greaser);

        DecisionMaker ai = greaser.GetComponent<DecisionMaker>();
        ai.enabled = false;

        doorway.PlayEnterSound();
        if (speech == null) {
            speech = greaser.GetComponent<Speech>();
        }
        Controllable control = greaser.GetComponent<Controllable>();
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

    public void MenuWasClosed() {
        foreach (GameObject greaser in greasers) {
            DecisionMaker ai = greaser.GetComponent<DecisionMaker>();
            ai.enabled = true;

            if ((Random.Range(0f, 1f) < 0.15f) || (greasers.Count == 0 && numSwitchblades == 0)) {
                GameObject switchBlade = GameObject.Instantiate(Resources.Load("prefabs/switchblade"), greaser.transform.position, Quaternion.identity) as GameObject;
                Inventory inv = greaser.GetComponent<Inventory>();
                Pickup pickup = switchBlade.GetComponent<Pickup>();
                inv.GetItem(pickup);
                numSwitchblades += 1;
            }
        }
        UINew.Instance.RefreshUI(active: true);
        complete = true;
    }
}