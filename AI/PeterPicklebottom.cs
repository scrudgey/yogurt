using UnityEngine;
using System.Collections.Generic;
using AI;

public class PeterPicklebottom : MonoBehaviour {
    public enum AIState{none, walkToTarget, slewAtTarget, leave}
    public AIState state;
    public Controllable controllable;
    public Ref<Duplicatable> target;
    public Ref<GameObject> objRef;
    public float timer;
    public Stack<Duplicatable> targets;
    private RoutineWalkToGameobject routine;
    public Doorway door;
    public AudioClip theme;
    public AudioClip[] takeSound;
    private AudioSource audioSource;
    public void Start(){
        controllable = GetComponent<Controllable>();
        target = new Ref<Duplicatable>(null);
        objRef = new Ref<GameObject>(null);
        if (targets == null){
            targets = new Stack<Duplicatable>();
            foreach(Duplicatable dup in FindObjectsOfType<Duplicatable>()){
                targets.Push(dup);
            }
        }
        if (targets.Count > 0){
            target.val = targets.Pop();
            objRef.val = target.val.gameObject;
        }
        state = AIState.walkToTarget;
        routine = new RoutineWalkToGameobject(gameObject, controllable, objRef);
        routine.minDistance = 0.1f;
        timer = -2f;
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
    }
    public void PlayThemeSong(){
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        audioSource.PlayOneShot(theme);
    }
    public void Update(){
        timer += Time.deltaTime;
        if (timer < 0)
            return;
        if (target.val == null && targets.Count == 0 && state != AIState.leave){
            state = AIState.leave;
        }
        switch(state){
            case AIState.walkToTarget:
            float distanceToTarget = Vector2.Distance(transform.position, target.val.transform.position);
            if (distanceToTarget > 0.1f){
                routine.Update();
            } else {
                timer = 0;
                state = AIState.slewAtTarget;
                controllable.ResetInput();
            }
            break;
            case AIState.slewAtTarget:
            if (timer > 1f && target.val != null){
                target.val.Nullify();
                ClaimsManager.Instance.WasDestroyed(target.val.gameObject);
                audioSource.PlayOneShot(takeSound[Random.Range(0, takeSound.Length)]);
            }
            if (timer > 2f){
                state = AIState.walkToTarget;
                target.val = targets.Pop();
                objRef.val = target.val.gameObject;
                timer = 0f;
            }
            break;
            case AIState.leave:
            objRef.val = door.gameObject;
            float doorDistance = Vector2.Distance(transform.position, objRef.val.transform.position);
            if (doorDistance > 0.1f){
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
