using UnityEngine;
using System.Collections.Generic;
using Nimrod;

public class CutsceneNeconomicon : Cutscene {
    private enum State { start, rising, hovering, falling, end };
    private State state;
    protected GameObject player;
    protected Transform footPoint;
    Awareness playerAwareness;
    AdvancedAnimation playerAnimation;
    Collider2D playerCollider;
    // protected Rigidbody2D playerBody;
    Controllable playerControl;
    Hurtable playerHurtable;
    public Vector3 initPosition;
    public Vector3 targetPosition;
    public Vector3 hoverInitPosition;
    public Vector3 footpointInitPosition;
    public CameraControl cameraControl;
    public float timer;
    public Necronomicon necronomicon;
    Collider2D zombieZonezone;
    float zombieTimer;
    float occurrenceTimer;
    Grammar grammar = new Grammar();
    public List<Collider2D> colliders = new List<Collider2D>();
    public override void Configure() {
        cameraControl = GameObject.FindObjectOfType<CameraControl>();
        necronomicon = GameObject.FindObjectOfType<Necronomicon>();
        GameObject zone = GameObject.FindWithTag("zombieSpawnZone");
        if (zone == null) {
            Debug.LogError("no zombie zone in this scene!!");
            End();
        }
        zombieZonezone = zone.GetComponent<Collider2D>();
        zombieTimer = UnityEngine.Random.Range(0.25f, 1.0f);
        occurrenceTimer = UnityEngine.Random.Range(0.25f, 1.0f);
        necronomicon.StartFx();
        player = GameManager.Instance.playerObject;
        footPoint = player.transform.Find("footPoint");
        footpointInitPosition = footPoint.position;
        InputController.Instance.suspendInput = true;
        playerAnimation = player.GetComponent<AdvancedAnimation>();
        playerCollider = player.GetComponent<Collider2D>();
        playerControl = player.GetComponent<Humanoid>();
        playerHurtable = player.GetComponent<Hurtable>();
        playerAwareness = player.GetComponent<Awareness>();
        if (playerAnimation) {
            playerAnimation.sequence = "generic3_idle_" + playerAnimation.lastPressed;
            playerAnimation.enabled = false;
        }
        if (playerCollider)
            playerCollider.enabled = false;
        if (playerControl) {
            playerControl.enabled = false;
        }
        if (playerAwareness) {
            playerAwareness.enabled = false;
        }
        UINew.Instance.RefreshUI();
        initPosition = player.transform.position;
        targetPosition = initPosition + new Vector3(0, 0.25f, 0);

        // animate head
        MessageHead messageOn = new MessageHead();
        messageOn.type = MessageHead.HeadType.speaking;
        messageOn.value = true;
        Toolbox.Instance.SendMessage(player, CutsceneManager.Instance, messageOn, sendUpwards: true);

        foreach (Collider2D collider in player.GetComponentsInChildren<Collider2D>()) {
            if (collider.enabled) {
                colliders.Add(collider);
                collider.enabled = false;
            }
        }

        UINew.Instance.RefreshUI(active: false);
        state = State.rising;
        configured = true;
        Toolbox.Instance.AudioSpeaker("ominous", player.transform.position);


        EventData necroData = EventData.Necronomicon(player);
        Toolbox.Instance.OccurenceFlag(player, necroData);

        grammar.Load("horror");
    }
    public override void Update() {
        timer += Time.deltaTime;
        switch (state) {
            case State.rising:
                UINew.Instance.RefreshUI(active: false);
                player.transform.position = Vector3.Lerp(player.transform.position, targetPosition, 0.1f);
                if (timer > 1f) {
                    timer = 0f;
                    state = State.hovering;
                    hoverInitPosition = player.transform.position;
                    cameraControl.Shake(0.01f);
                    foreach (Hurtable hurtable in GameObject.FindObjectsOfType<Hurtable>()) {
                        hurtable.Resurrect();
                    }
                    bool necroGates = false;
                    foreach (NecroGate necroGate in GameObject.FindObjectsOfType<NecroGate>()) {
                        if (Vector2.Distance(player.transform.position, necroGate.transform.position) > 2f)
                            continue;
                        necroGate.Unlock();
                        necroGates = true;
                    }
                    if (necroGates)
                        MusicController.Instance.EnqueueMusic(new Music(new Track(TrackName.moonWarp)));
                }
                break;
            case State.hovering:
                zombieTimer -= Time.deltaTime;
                occurrenceTimer -= Time.deltaTime;
                // lights dim
                // effect kicks in
                // raise zombies
                float sinusoid = 0.04f * Mathf.Sin(timer * 3.14f);
                Vector3 newPos = new Vector3(hoverInitPosition.x, hoverInitPosition.y + sinusoid, hoverInitPosition.z);
                if (player != null)
                    player.transform.position = newPos;
                if (zombieTimer <= 0f) {
                    zombieTimer = UnityEngine.Random.Range(0.45f, 1.0f);
                    SpawnZombie(player);
                    if (Random.Range(0, 1f) < 0.25f) {
                        MessageSpeech message = new MessageSpeech(grammar.Parse("{reading}"));
                        Toolbox.Instance.SendMessage(player, CutsceneManager.Instance, message);
                    }
                }
                if (occurrenceTimer <= 0f) {
                    occurrenceTimer = UnityEngine.Random.Range(0.25f, 1.0f);
                    necronomicon.EldritchOccurrence();
                }
                if (timer > 5f) {
                    timer = 0f;
                    state = State.falling;
                    MessageHead messageOff = new MessageHead();
                    messageOff.type = MessageHead.HeadType.speaking;
                    messageOff.value = false;
                    Toolbox.Instance.SendMessage(player, CutsceneManager.Instance, messageOff, sendUpwards: true);
                    necronomicon.StopFx();
                }
                break;
            case State.falling:
                // lerp back to init position
                if (player != null)
                    player.transform.position = Vector3.Lerp(player.transform.position, initPosition, 0.1f);
                if (timer > 1f) {
                    timer = 0f;
                    state = State.end;
                    End();
                }
                break;
            default:
                break;
        }
        footPoint.position = footpointInitPosition;
    }
    void End() {
        InputController.Instance.suspendInput = false;
        UINew.Instance.RefreshUI(active: true);
        if (playerAnimation)
            playerAnimation.enabled = true;
        if (playerCollider)
            playerCollider.enabled = true;
        if (playerControl) {
            playerControl.enabled = true;
        }
        if (playerAwareness) {
            playerAwareness.enabled = true;
        }
        foreach (Collider2D collider in colliders) {
            collider.enabled = true;
        }
        UINew.Instance.RefreshUI(active: true);
        complete = true;
    }
    public void SpawnZombie(GameObject player) {
        Vector3 position = Toolbox.RandomPointInBox(zombieZonezone.bounds, player.transform.position);
        GameObject.Instantiate(Resources.Load("zombieSpawner"), position, Quaternion.identity);
    }
}