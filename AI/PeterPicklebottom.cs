﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using AI;

public class PeterPicklebottom : MonoBehaviour {
    public enum AIState { none, walkToTarget, slewAtTarget, leave, passive }
    public AIState state;
    public Controller controller;
    public SimpleControl controllable;
    public Ref<Duplicatable> target;
    public Ref<GameObject> objRef;
    public float timer;
    public Stack<Duplicatable> targets;
    private RoutineWalkToGameobject routine;
    public Doorway door;
    public AudioClip theme;
    public AudioClip[] takeSound;
    private AudioSource audioSource;
    public float slewTime = 1f;
    public void SetTargets() {
        targets = new Stack<Duplicatable>();
        int maxNumber = Random.Range(5, 10);
        foreach (Duplicatable dup in GameObject.FindObjectsOfType<Duplicatable>()) {
            if (dup.PickleReady()) {
                targets.Push(dup);
            }
            if (targets.Count >= maxNumber)
                break;
        }
    }
    public void Start() {
        controllable = GetComponent<SimpleControl>();
        controller = new Controller(controllable);

        target = new Ref<Duplicatable>(null);
        objRef = new Ref<GameObject>(null);
        if (SceneManager.GetActiveScene().name == "apartment") {
            foreach (Doorway doorway in GameObject.FindObjectsOfType<Doorway>()) {
                if (doorway.entryID == 0 && !doorway.spawnPoint) {
                    door = doorway;
                }
            }
            if (targets == null) {
                SetTargets();
            }
            if (targets.Count > 0) {
                target.val = targets.Pop();
                objRef.val = target.val.gameObject;
            }
            state = AIState.walkToTarget;
            routine = new RoutineWalkToGameobject(gameObject, controller, objRef);
            routine.minDistance = 0.1f;
            timer = -2f;
            audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        } else {
            state = AIState.passive;
            controller.Deregister();
        }
    }
    public void PlayThemeSong() {
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        audioSource.PlayOneShot(theme);
    }
    public void Update() {
        if (state == AIState.passive)
            return;
        timer += Time.deltaTime;
        if (timer < 0)
            return;
        if (target.val == null && targets.Count == 0 && state != AIState.leave) {
            state = AIState.leave;
        }

        switch (state) {
            case AIState.walkToTarget:
                while (target.val == null && targets.Count > 0) {
                    target.val = targets.Pop();
                    if (target.val != null) {
                        objRef.val = target.val.gameObject;
                        Debug.Log(target.val);
                    }
                }
                if (target.val == null && targets.Count == 0) {
                    state = AIState.leave;
                }
                float distanceToTarget = Vector2.Distance(transform.position, target.val.transform.position);
                status routineStatus = routine.Update();
                if (routineStatus == status.success) {
                    timer = 0;
                    state = AIState.slewAtTarget;
                    controller.ResetInput();
                }
                break;
            case AIState.slewAtTarget:
                if (timer > slewTime && target.val != null) {
                    target.val.nullifyFX = null;
                    target.val.Nullify();
                    ClaimsManager.Instance.WasDestroyed(target.val.gameObject);
                    audioSource.PlayOneShot(takeSound[Random.Range(0, takeSound.Length)]);
                    controllable.maxSpeed += 0.15f;
                }
                if (timer > 2f * slewTime) {
                    state = AIState.walkToTarget;
                    target.val = targets.Pop();
                    if (target.val != null)
                        objRef.val = target.val.gameObject;
                    timer = 0f;
                    slewTime -= 0.2f;
                    slewTime = Mathf.Max(0.3f, slewTime);
                }
                break;
            case AIState.leave:
                controllable.maxSpeed = 0.4f;
                objRef.val = door.gameObject;
                float doorDistance = Vector2.Distance(transform.position, objRef.val.transform.position);
                if (doorDistance > 0.1f) {
                    routine.Update();
                } else {
                    door.PlayExitSound();
                    Destroy(gameObject);
                }
                break;
            default:
                break;
        }
    }
}
