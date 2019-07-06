using UnityEngine;
using System.Collections.Generic;
using Easings;

public class DancingGod : MonoBehaviour {
    private enum State { wait, jump, pause }
    private State state;
    public List<Sprite> frames;
    private int frameIndex = 0;
    private float timer;
    private float interval;
    public float waitInterval = 1f;
    public float jumpInterval = 0.5f;
    public float pauseInterval = 4;
    public float jumpHeight = 0.1f;
    public int numberOfJumps;
    AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    public AudioClip jumpSound;
    public AudioClip landSound;
    private Vector3 initPosition;
    private CameraControl cam;
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) {
            Destroy(this);
        }
        spriteRenderer.sprite = frames[0];
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        initPosition = transform.localPosition;

        interval = waitInterval;
        state = State.wait;
        numberOfJumps = 3;
        cam = GameObject.FindObjectOfType<CameraControl>();
    }
    void Update() {
        timer += Time.deltaTime;
        if (state == State.jump) {
            JumpUpdate();
        }
        if (timer > interval) {
            timer = 0f;
            switch (state) {
                case State.wait:
                    FinishWait();
                    break;
                case State.jump:
                    FinishJump();
                    break;
                case State.pause:
                    FinishPause();
                    break;
                default:
                    break;
            }
        }
    }
    void JumpUpdate() {
        Vector3 pos = transform.localPosition;
        pos.y = (float)PennerDoubleAnimation.ExpoEaseOut(timer, initPosition.y, jumpHeight, jumpInterval);
        transform.localPosition = pos;
    }
    void FinishWait() {
        interval = jumpInterval;
        state = State.jump;
        audioSource.PlayOneShot(jumpSound);
    }
    void FinishJump() {
        transform.localPosition = initPosition;
        nextFrame();
        numberOfJumps -= 1;
        if (numberOfJumps > 0) {
            interval = waitInterval;
            state = State.wait;
        } else {
            interval = pauseInterval;
            state = State.pause;
        }
        audioSource.PlayOneShot(landSound);

        float dist = Vector3.Distance(transform.position, cam.transform.position);
        cam.Shake(0.0325f / (Mathf.Pow(dist, 2)));
    }
    void FinishPause() {
        numberOfJumps = 3;
        state = State.jump;
        interval = jumpInterval;
        audioSource.PlayOneShot(jumpSound);
    }
    void nextFrame() {
        frameIndex += 1;
        if (frameIndex == frames.Count) {
            frameIndex = 0;
        }
        spriteRenderer.sprite = frames[frameIndex];
    }
}
