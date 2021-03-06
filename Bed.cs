﻿using UnityEngine;
using UnityEngine.InputSystem;
public class Bed : Doorway {
    bool unmade = true;
    public Sprite[] bedSprites;
    public Sprite[] headSprites;
    public Sprite[] bubbleSprites;
    public AudioClip snoreSound;
    private SpriteRenderer head;
    private SpriteRenderer bubble;
    private float animationTimer;
    SpriteRenderer spriteRenderer;
    private bool sleeping;
    private bool frame;
    public AudioClip beddingSound;
    public bool keypressedThisFrame;
    public override void Awake() {
        keypressedThisFrame = false;
        InputController.Instance.PrimaryAction.action.performed += ctx => {
            keypressedThisFrame = ctx.ReadValueAsButton();
        };
        InputController.Instance.PrimaryAction.action.Enable();

        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        audioSource.spatialBlend = 0;
        spriteRenderer = GetComponent<SpriteRenderer>();
        head = transform.Find("head").GetComponent<SpriteRenderer>();
        bubble = transform.Find("bubble").GetComponent<SpriteRenderer>();
        Interaction makeBed = new Interaction(this, "Clean", "MakeBed");
        makeBed.validationFunction = true;
        makeBed.descString = "Make the bed";
        Interaction sleep = new Interaction(this, "Sleep", "Sleep");
        sleep.descString = "Go to bed";
        interactions.Add(sleep);
        interactions.Add(makeBed);
        head.gameObject.SetActive(false);
        bubble.gameObject.SetActive(false);

    }
    // void OnEnable() {
    //     InputController.Instance.PrimaryAction.action.Enable();
    // }
    public void MakeBed() {
        unmade = false;
        if (spriteRenderer) {
            spriteRenderer.sprite = bedSprites[0];
            audioSource.PlayOneShot(beddingSound);
        }
        GameManager.Instance.IncrementStat(StatType.bedsMade, 1);
    }
    public bool MakeBed_Validation() {
        return unmade;
    }
    public void Sleep() {
        MySaver.Save();
        GameManager.Instance.NewDayCutscene();
    }
    public void SleepCutscene() {
        frame = true;
        animationTimer = 0f;
        spriteRenderer.sprite = bedSprites[0];
        head.gameObject.SetActive(true);
        // change head sprite
        HeadAnimation playerHead = GameManager.Instance.playerObject.GetComponentInChildren<HeadAnimation>();
        if (playerHead != null) {
            headSprites[0] = playerHead.sprites[0];
            headSprites[1] = playerHead.sprites[1];
            head.sprite = playerHead.sprites[0];
        }

        bubble.gameObject.SetActive(true);
        Toolbox.Instance.SwitchAudioListener(gameObject);
        sleeping = true;
        // TODO: peter picklebottom cutscene threshhold
        int collectible = 0;
        foreach (Duplicatable dup in GameObject.FindObjectsOfType<Duplicatable>()) {
            if (dup.PickleReady())
                collectible++;
        }
        if (collectible > 3 && GameManager.Instance.data.days > 1 && !GameManager.Instance.data.loadedDay && GameManager.Instance.data.activeMagicianSequence == "") {
            CutsceneManager.Instance.InitializeCutscene<CutscenePickleBottom>();
        }
        UINew.Instance.RefreshUI(active: false);
        Update();
    }
    void Update() {
        if (sleeping) {
            if (keypressedThisFrame) {
                Debug.Log("key pressed for bed");
            }
            // TODO: prevent early trigger
            if (animationTimer > 0.02f && keypressedThisFrame &&
            (InputController.Instance.state != InputController.ControlState.cutscene &&
            InputController.Instance.state != InputController.ControlState.inMenu &&
            InputController.Instance.state != InputController.ControlState.waitForMenu
            )
            ) {
                sleeping = false;
                unmade = true;
                spriteRenderer.sprite = bedSprites[1];
                head.gameObject.SetActive(false);
                bubble.gameObject.SetActive(false);
                GameManager.Instance.playerObject.SetActive(true);
                Toolbox.Instance.SwitchAudioListener(GameManager.Instance.playerObject);
                audioSource.PlayOneShot(beddingSound);
                UINew.Instance.CloseActiveMenu();
                UINew.Instance.RefreshUI(active: true);
                HeadAnimation playerHead = GameManager.Instance.playerObject.GetComponentInChildren<HeadAnimation>();
                if (playerHead) {
                    playerHead.UpdateSequence();
                }
                CheckDiaryEntry();
            }
            keypressedThisFrame = false;

            animationTimer += Time.deltaTime;
            if (animationTimer > 1f) {
                animationTimer = 0f;
                frame = !frame;
                if (frame) {
                    head.sprite = headSprites[0];
                    bubble.sprite = bubbleSprites[0];
                } else {
                    head.sprite = headSprites[1];
                    bubble.sprite = bubbleSprites[1];
                    audioSource.PlayOneShot(snoreSound);
                }
            }
        }
    }
    void CheckDiaryEntry() {
        if (GameManager.Instance.data.queuedDiaryEntry != "") {
            GameManager.Instance.ShowDiaryEntryDelay(GameManager.Instance.data.queuedDiaryEntry, delay: 0.5f);
            GameManager.Instance.data.queuedDiaryEntry = "";
            return;
        }
        if (GameManager.Instance.data.days == 1) {
            GameManager.Instance.ShowDiaryEntryDelay("diaryNew", delay: 0.5f);
            return;
        }
        if (GameManager.Instance.data.deaths >= 1 && GameManager.Instance.data.deathCutscenesPlayed == 0) {
            GameManager.Instance.data.deathCutscenesPlayed = 1;
            GameManager.Instance.ShowDiaryEntryDelay("death1", delay: 0.5f);
            return;
        }
        InputController.Instance.state = InputController.ControlState.normal;
    }
}
