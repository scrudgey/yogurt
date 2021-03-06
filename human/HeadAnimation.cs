﻿using UnityEngine;

public class HeadAnimation : MonoBehaviour, IDirectable, ISaveable {
    public SkinColor _skinColor;
    public SkinColor skinColor {
        get { return _skinColor; }
        set {
            _skinColor = value;
            LoadSprites();
        }
    }
    private bool speaking;
    private string _spriteSheet;
    public string spriteSheet {
        get { return _spriteSheet; }
        set {
            if (value != _spriteSheet) {
                _spriteSheet = value;
                LoadSprites();
            }
        }
    }
    private string _sequence;
    private string sequence {
        get { return _sequence; }
        set {
            if (_sequence != value) {
                _sequence = value;
                UpdateSequence();
            }
        }
    }
    public Sprite[] sprites;
    private SpriteRenderer spriteRenderer;
    public string baseName;
    private int baseFrame;
    private int frame;
    public bool eating;
    private ParticleSystem crumbs;
    private ParticleSystem vomit;
    private bool vomiting;
    public Color crumbColor = Color.white;
    private float eatingCountDown;
    public Controllable.HitState hitState;
    private string lastPressed;
    public void LoadSprites() {
        // sprites = Toolbox.ApplySkinToneToSpriteSheet(spriteSheet, skinColor);
        sprites = Toolbox.MemoizedSkinTone(spriteSheet, skinColor);
    }
    public void UpdateSequence() {
        Animation animationComponent = GetComponent<Animation>();
        animationComponent.Play(sequence);
    }
    public void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        MessageDirectable directableMessage = new MessageDirectable();
        directableMessage.addDirectable.Add(this);
        Toolbox.Instance.SendMessage(gameObject, this, directableMessage);

        GameObject vomObject = Instantiate(Resources.Load("particles/vom"), transform.position, Quaternion.identity) as GameObject;
        vomit = vomObject.GetComponent<ParticleSystem>();
        vomObject.transform.SetParent(transform, false);
        vomObject.transform.localPosition = new Vector3(-0.009f, 0.0292f, 0);

        GameObject crumbObject = Instantiate(Resources.Load("particles/crumbs"), transform.position, Quaternion.identity) as GameObject;
        crumbs = crumbObject.GetComponent<ParticleSystem>();
        crumbObject.transform.SetParent(transform, false);
        crumbObject.transform.localPosition = new Vector3(0.0121f, 0.0250f, 0);

        Toolbox.RegisterMessageCallback<MessageHead>(this, HandleMessageHead);
        Toolbox.RegisterMessageCallback<MessageHitstun>(this, HandleHitStun);
        Toolbox.RegisterMessageCallback<MessageNetIntrinsic>(this, HandleNetIntrinsic);
        Update();
    }
    public void HandleNetIntrinsic(MessageNetIntrinsic message) {
        if (message.netBuffs[BuffType.undead].boolValue) {
            skinColor = SkinColor.undead;
        }
    }
    void HandleMessageHead(MessageHead message) {
        switch (message.type) {
            case MessageHead.HeadType.eating:
                crumbColor = message.crumbColor;
                eating = message.value;
                var mainModule = crumbs.main;
                mainModule.startColor = crumbColor;
                if (eating) {
                    eatingCountDown = 2f;
                    if (!crumbs.isPlaying)
                        crumbs.Play();
                }
                break;
            case MessageHead.HeadType.vomiting:
                vomiting = message.value;
                mainModule = crumbs.main;
                mainModule.startColor = crumbColor;
                if (vomiting) {
                    if (!vomit.isPlaying)
                        vomit.Play();
                } else {
                    vomit.Stop();
                }
                break;
            case MessageHead.HeadType.speaking:
                speaking = message.value;
                break;
            default:
                break;
        }
    }
    void HandleHitStun(MessageHitstun message) {
        hitState = message.hitState;
    }
    void Update() {
        string updateSheet = baseName;
        string updateSequence = "generic";
        if ((speaking || eating) && !vomiting) {
            updateSequence = updateSequence + "_speak";
        } else {
            updateSequence = updateSequence + "_idle";
        }
        if (eatingCountDown > 0) {
            eatingCountDown -= Time.deltaTime;
            if (eatingCountDown < 0) {
                eating = false;
                crumbs.Stop();
            }
        }
        updateSheet = updateSheet + "_head";
        switch (lastPressed) {
            case "down":
                baseFrame = 2;
                break;
            case "up":
                baseFrame = 4;
                break;
            default:
                baseFrame = 0;
                break;
        }
        if ((hitState > Controllable.HitState.none && !speaking && !eating) || vomiting) {
            baseFrame += 1;
        }
        spriteSheet = updateSheet;
        sequence = updateSequence;
    }
    public void SetFrame(int animationFrame) {
        frame = animationFrame + baseFrame;
        spriteRenderer.sprite = sprites[frame];
    }
    public void DirectionChange(Vector2 newdir) {
        lastPressed = Toolbox.Instance.DirectionToString(newdir);
    }
    public void SaveData(PersistentComponent data) {
        data.ints["hitstate"] = (int)hitState;
        data.strings["baseName"] = baseName;
        data.ints["skincolor"] = (int)skinColor;
    }
    public void LoadData(PersistentComponent data) {
        hitState = (Controllable.HitState)data.ints["hitstate"];
        baseName = data.strings["baseName"];
        skinColor = (SkinColor)data.ints["skincolor"];
    }
}
