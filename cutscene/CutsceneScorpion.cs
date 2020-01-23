using UnityEngine;
using System.Collections.Generic;
using AI;

public class CutsceneScorpion : Cutscene {
    private enum State { none, spawn, walk, slew };
    private State state;
    private Doorway doorway;
    private float timer;
    private RoutineWalkToPoint walkRoutine;
    private GameObject greaserPrefab;
    private int numGreasers;
    public override void Configure() {
        configured = true;
        state = State.spawn;
        // doorway = GameObject.FindObjectOfType<Doorway>();
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
            if (timer > 0.5f) {
                timer = 0f;
                GameObject greaser = GameObject.Instantiate(greaserPrefab, doorway.transform.position, Quaternion.identity) as GameObject;
                Controllable control = greaser.GetComponent<Controllable>();
                Ref<Vector2> target = new Ref<Vector2>(Vector2.zero);
                walkRoutine = new RoutineWalkToPoint(greaser, control, target, 0.1f);
                state = State.walk;
                numGreasers += 1;
                doorway.PlayEnterSound();
            }
        }
        if (state == State.walk) {
            walkRoutine.Update();
            if (timer > 0.5f) {
                timer = 0f;
                if (numGreasers < 5) {
                    state = State.spawn;
                } else {
                    state = State.slew;
                }
            }
        }
        if (state == State.slew) {
            if (timer > 1f) {
                complete = true;
            }
        }
    }
}