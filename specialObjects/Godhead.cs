using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Easings;

public class Godhead : MonoBehaviour {
    enum State { none, moveHead, moveHand, handSlew, bless, destroy, retract, dialogue }
    State state;
    public Transform hand;
    public Transform handPoint;
    public Vector3 startPoint;
    public Vector3 endPoint;
    public Vector3 startHandPoint;
    public Vector3 endHandPoint;
    private Vector3 headDelta;
    private Vector3 handDelta;
    public AudioClip growl;
    public AudioSource source;
    public CameraControl cam;
    public Sprite itemSprite;
    public SpriteRenderer handPointRenderer;
    public ParticleSystem handParticles;
    public float timer;
    public float handTime = 1f;
    public float headTime = 1f;
    public PhysicalBootstrapper item;
    public Speech godSpeech;
    DialogueMenu menu;
    public delegate void MyDelegate();
    public MyDelegate onQuit;
    public void Start() {
        cam = GameObject.FindObjectOfType<CameraControl>();

        if (GameManager.Instance.data != null && GameManager.Instance.data.cosmicName != "") {
            godSpeech.defaultMonologue = "dancing_god_cosmic";
        }

        // source = Toolbox.Instance.SetUpAudioSource(gameObject);
        source = Toolbox.Instance.SetUpGlobalAudioSource(gameObject);
        state = State.moveHead;
        hand.gameObject.SetActive(false);
        headDelta = endPoint - startPoint;
        handDelta = endHandPoint - startHandPoint;
        handPointRenderer.enabled = false;
        itemSprite = item.GetComponent<SpriteRenderer>().sprite;
        handPointRenderer.sprite = itemSprite;
        // distantGod.SetActive(false);
    }
    public void Update() {
        timer += Time.deltaTime;
        switch (state) {
            case State.moveHand:
                if (!source.isPlaying) {
                    source.clip = growl;
                    source.Play();
                }
                UpdateMoveHand();
                break;
            case State.moveHead:
                if (!source.isPlaying) {
                    source.clip = growl;
                    source.Play();
                }
                UpdateMoveHead();
                break;
            case State.handSlew:
                source.Stop();
                if (timer > 2f) {
                    menu = godSpeech.SpeakWith();
                    menu.cutsceneDialogue = true;
                    state = State.dialogue;
                }
                break;
            case State.bless:
                source.Stop();
                UpdateBless();
                break;
            case State.destroy:
                source.Stop();
                UpdateDestroy();
                break;
            case State.retract:
                if (!source.isPlaying) {
                    source.clip = growl;
                    source.Play();
                }
                UpdateRetract();
                break;
            default:
                break;
        }
    }
    void UpdateMoveHead() {
        if (timer < headTime) {
            float x = (float)PennerDoubleAnimation.Linear(timer, startPoint.x, headDelta.x, headTime);
            float y = (float)PennerDoubleAnimation.Linear(timer, startPoint.y, headDelta.y, headTime);
            float z = (float)PennerDoubleAnimation.Linear(timer, startPoint.z, headDelta.z, headTime);
            transform.position = new Vector3(x, y, z);
            cam.Shake(0.015f);
        } else if (timer > headTime + 2f) {
            state = State.moveHand;
            hand.gameObject.SetActive(true);
            handParticles.Stop();
            hand.localPosition = startHandPoint;
            timer = 0f;
        } else {
            source.Stop();
        }
    }
    void UpdateMoveHand() {
        if (timer < handTime) {
            float x = (float)PennerDoubleAnimation.Linear(timer, startHandPoint.x, handDelta.x, handTime);
            float y = (float)PennerDoubleAnimation.Linear(timer, startHandPoint.y, handDelta.y, handTime);
            float z = (float)PennerDoubleAnimation.Linear(timer, startHandPoint.z, handDelta.z, handTime);
            hand.localPosition = new Vector3(x, y, z);
            cam.Shake(0.015f);
        } else if (timer > handTime + 2f) {
            handParticles.Play();
            handPointRenderer.enabled = true;
            state = State.handSlew;
            timer = 0f;
        } else {
            source.Stop();
        }
    }

    void UpdateRetract() {
        if (timer < headTime) {
            float x = (float)PennerDoubleAnimation.Linear(timer, endPoint.x, -1f * headDelta.x, headTime);
            float y = (float)PennerDoubleAnimation.Linear(timer, endPoint.y, -1f * headDelta.y, headTime);
            float z = (float)PennerDoubleAnimation.Linear(timer, endPoint.z, -1f * headDelta.z, headTime);
            transform.position = new Vector3(x, y, z);
            cam.Shake(0.015f);

            x = (float)PennerDoubleAnimation.Linear(timer, endHandPoint.x, -1f * handDelta.x, headTime);
            y = (float)PennerDoubleAnimation.Linear(timer, endHandPoint.y, -1f * handDelta.y, headTime);
            z = (float)PennerDoubleAnimation.Linear(timer, endHandPoint.z, -1f * handDelta.z, headTime);
            hand.localPosition = new Vector3(x, y, z);
        } else if (timer > headTime) {
            End();
        }

    }
    void UpdateBless() {
        if (!item.gameObject.activeInHierarchy) {
            handPointRenderer.enabled = false;
            item.transform.position = handPoint.position;
            BlessItem(item);
        }
        if (timer > 1f) {
            godSpeech.defaultMonologue = "dancing_god_bless";
            menu = godSpeech.SpeakWith();
            menu.cutsceneDialogue = true;
            menu.menuClosed += FinalSpeechCallback;
            state = State.dialogue;
        }
    }
    public static void BlessItem(PhysicalBootstrapper item) {
        item.gameObject.SetActive(true);
        item.InitPhysical(0.05f, Vector3.zero);
        item.physical.StartGroundMode();

        Intrinsics itemIntrinsics = Toolbox.GetOrCreateComponent<Intrinsics>(item.gameObject);
        Uniform uniform = item.GetComponent<Uniform>();
        Hat hat = item.GetComponent<Hat>();
        if (uniform != null || hat != null) {
            Buff bless = new Buff(BuffType.invulnerable, true, 0, 30f);
            itemIntrinsics.liveBuffs.Add(bless);
            itemIntrinsics.IntrinsicsChanged();
        } else {
            Buff bless = new Buff(BuffType.invulnerable, true, 0, 3);
            itemIntrinsics.buffs.Add(bless);
            itemIntrinsics.IntrinsicsChanged();
        }
        itemIntrinsics.IntrinsicsChanged();
    }
    void UpdateDestroy() {
        if (item != null) {
            Destroy(item);
        }
        if (timer > 1f) {
            godSpeech.defaultMonologue = "dancing_god_destroy";
            menu = godSpeech.SpeakWith();
            menu.cutsceneDialogue = true;
            menu.menuClosed += FinalSpeechCallback;
            state = State.dialogue;
        }
    }
    public void FinalSpeechCallback() {
        InputController.Instance.state = InputController.ControlState.cutscene;
        timer = 0f;
        state = State.retract;
        handParticles.Stop();
        handPointRenderer.enabled = false;
    }
    public void Bless() {
        InputController.Instance.state = InputController.ControlState.cutscene;
        state = State.bless;
        timer = 0f;
    }
    public void Destroy() {
        InputController.Instance.state = InputController.ControlState.cutscene;
        state = State.destroy;
        timer = 0f;
    }
    public void End() {
        onQuit();
        Destroy(gameObject);
    }
}
