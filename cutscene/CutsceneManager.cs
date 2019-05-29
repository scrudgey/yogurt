using UnityEngine;
using UnityEngine.UI;
using Easings;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
public abstract class Cutscene {
    public virtual void Update() { }
    public virtual void Configure() { }
    public virtual void EscapePressed() { }
    public virtual void CleanUp() { }
    public bool complete;
    public bool configured;
}
public class CutsceneMoonLanding : Cutscene {
    private float timer;
    public Dictionary<Collider2D, PhysicsMaterial2D> materials;
    Controllable playerControllable;
    AdvancedAnimation playerAnimation;
    HeadAnimation playerHeadAnimation;
    Collider2D playerCollider;
    Speech playerSpeech;
    public override void Configure() {
        materials = new Dictionary<Collider2D, PhysicsMaterial2D>();
        Transform spawnPoint = GameObject.Find("cannonEntryPoint").transform;
        GameObject player = GameManager.Instance.playerObject;
        configured = true;
        player.transform.localScale = new Vector3(-1f, 1f, 1f);
        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;
        playerControllable = GameManager.Instance.playerObject.GetComponent<Controllable>();
        playerAnimation = GameManager.Instance.playerObject.GetComponent<AdvancedAnimation>();
        playerHeadAnimation = GameManager.Instance.playerObject.GetComponent<HeadAnimation>();
        playerSpeech = GameManager.Instance.playerObject.GetComponent<Speech>();
        playerCollider = GameManager.Instance.playerObject.GetComponent<Collider2D>();
        if (playerControllable != null) {
            playerControllable.enabled = false;
        }
        if (playerAnimation != null) {
            playerAnimation.enabled = false;
        }
        if (playerHeadAnimation != null) {
            playerHeadAnimation.enabled = false;
        }
        if (playerSpeech != null) {
            playerSpeech.enabled = false;
        }
        if (playerCollider != null) {
            playerCollider.enabled = false;
        }
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        body.gravityScale = GameManager.Instance.gravity;
        body.drag = 0f;
        body.AddForce(8000f * spawnPoint.up, ForceMode2D.Force);
        PhysicsMaterial2D moonMaterial = Resources.Load("moonlanding") as PhysicsMaterial2D;
        foreach (Collider2D collider in player.GetComponentsInChildren<Collider2D>()) {
            materials[collider] = collider.sharedMaterial;
            collider.sharedMaterial = moonMaterial;
        }
        // AudioSource playerAudio = Toolbox.GetOrCreateComponent<AudioSource>(GameManager.Instance.playerObject);
        // AudioSource playerAudio = Toolbox.Instance.SetUpAudioSource(GameManager.Instance.playerObject);
        AudioClip charlierAugh = Resources.Load("sounds/auugh") as AudioClip;
        // GameManager.Instance.PlayPublicSound(charlierAugh);
        Toolbox.Instance.AudioSpeaker(charlierAugh, new Vector3(0.38f, -0.65f, 0));
        // playerAudio.PlayOneShot(charlierAugh);
    }
    public override void Update() {
        if (timer == 0) {
            UINew.Instance.RefreshUI();
        }
        timer += Time.deltaTime;
        // check to reenable collider
        if (GameManager.Instance.playerObject.transform.position.y < -0.533 && playerCollider != null && !playerCollider.enabled) {
            playerCollider.enabled = true;
        }
        if (timer > 3f) {
            complete = true;
        }
    }
    public override void CleanUp() {
        UINew.Instance.RefreshUI(active: true);
        if (playerControllable != null) {
            playerControllable.enabled = true;
        }
        if (playerAnimation != null) {
            playerAnimation.enabled = true;
        }
        if (playerHeadAnimation != null) {
            playerHeadAnimation.enabled = true;
        }
        if (playerSpeech != null) {
            playerSpeech.enabled = true;
        }
        Rigidbody2D body = GameManager.Instance.playerObject.GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        foreach (KeyValuePair<Collider2D, PhysicsMaterial2D> kvp in materials) {
            kvp.Key.sharedMaterial = kvp.Value;
        }
        Hurtable playerHurable = GameManager.Instance.playerObject.GetComponent<Hurtable>();
        if (playerHurable) {
            playerHurable.KnockDown();
        }
        GameObject landingStrip = GameObject.Find("landingStrip");
        landingStrip.GetComponent<BoxCollider2D>().enabled = false;
        landingStrip.GetComponentInChildren<ParticleSystem>().Stop();
    }
}
public class CutsceneSpace : Cutscene {
    private float timer;
    public override void Configure() {
        GameObject player = GameManager.Instance.playerObject;
        configured = true;
        player.transform.localScale = new Vector3(-1f, 1f, 1f);
        player.transform.RotateAround(player.transform.position, new Vector3(0f, 0f, 1f), 90f);
        Controllable playerControllable = GameManager.Instance.playerObject.GetComponent<Controllable>();
        if (playerControllable != null) {
            playerControllable.enabled = false;
        }
        Speech playerSpeech = GameManager.Instance.playerObject.GetComponent<Speech>();
        if (playerSpeech != null) {
            playerSpeech.enabled = false;
        }
    }
    public override void Update() {
        if (timer == 0) {
            UINew.Instance.RefreshUI();
        }
        timer += Time.deltaTime;
        if (timer > 3.0f) {
            complete = true;
            SceneManager.LoadScene("moon1");
            GameManager.Instance.data.entryID = 420;
        }
    }
}
public class CutsceneCannon : Cutscene {
    public Cannon cannon;
    public CameraControl camControl;
    public Transform ejectionPoint;
    private float timer;
    private bool shot;
    public override void Configure() {
        cannon = GameObject.FindObjectOfType<Cannon>();
        Toolbox.Instance.SwitchAudioListener(cannon.gameObject);
        ejectionPoint = cannon.transform.Find("ejectionPoint");
        GameManager.Instance.playerObject.SetActive(false);
        camControl = GameObject.FindObjectOfType<CameraControl>();
        camControl.focus = cannon.gameObject;
        configured = true;
    }
    public override void Update() {
        if (timer == 0) {
            UINew.Instance.RefreshUI();
        }
        timer += Time.deltaTime;
        if (timer > 2.5f && !shot) {
            shot = true;
            cannon.Shoot();
            Toolbox.Instance.SwitchAudioListener(GameManager.Instance.playerObject);
            GameManager.Instance.playerObject.SetActive(true);
            RotateTowardMotion rot = GameManager.Instance.playerObject.AddComponent<RotateTowardMotion>();
            rot.angleOffset = 270f;
            camControl.focus = GameManager.Instance.playerObject;
            foreach (Collider2D collider in GameManager.Instance.playerObject.GetComponentsInChildren<Collider2D>()) {
                collider.enabled = false;
            }
            GameManager.Instance.playerObject.transform.position = ejectionPoint.position;
            GameManager.Instance.playerObject.transform.rotation = ejectionPoint.rotation;
            AdvancedAnimation playerAnimation = GameManager.Instance.playerObject.GetComponent<AdvancedAnimation>();
            if (playerAnimation != null) {
                playerAnimation.SetFrame(14);
                playerAnimation.enabled = false;
            }
            HeadAnimation playerHeadAnimation = GameManager.Instance.playerObject.GetComponent<HeadAnimation>();
            if (playerHeadAnimation != null) {
                playerHeadAnimation.SetFrame(4);
                playerHeadAnimation.enabled = false;
            }
            Controllable playerControllable = GameManager.Instance.playerObject.GetComponent<Controllable>();
            if (playerControllable != null) {
                playerControllable.enabled = false;
            }
            Rigidbody2D playerBody = GameManager.Instance.playerObject.GetComponent<Rigidbody2D>();
            playerBody.gravityScale = GameManager.Instance.gravity;
            AudioSource playerAudio = Toolbox.GetOrCreateComponent<AudioSource>(GameManager.Instance.playerObject);
            AudioClip charlierAugh = Resources.Load("sounds/auugh") as AudioClip;
            playerAudio.PlayOneShot(charlierAugh);

            playerBody.AddForce(10000f * ejectionPoint.up, ForceMode2D.Force);
            camControl.Shake(0.25f);
        }
        if (timer > 4f) {
            // switch scenes
            complete = true;
            SceneManager.LoadScene("space");
        }
    }
}
public class CutscenePickleBottom : Cutscene {
    CameraControl camControl;
    GameObject peter;
    PeterPicklebottom peterAI;
    GameObject nightShade;
    public override void Configure() {
        Doorway doorway = null;
        foreach (Doorway door in GameObject.FindObjectsOfType<Doorway>()) {
            if (door.entryID == 0 && !door.spawnPoint) {
                doorway = door;
            }
        }
        peter = GameObject.Instantiate(Resources.Load("prefabs/peter_picklebottom")) as GameObject;
        doorway.Enter(peter);
        camControl = GameObject.FindObjectOfType<CameraControl>();
        camControl.focus = peter;
        UINew.Instance.RefreshUI();
        nightShade = GameObject.Instantiate(Resources.Load("UI/nightShade")) as GameObject;
        nightShade.GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
        peterAI = peter.GetComponent<PeterPicklebottom>();
        peterAI.door = doorway;
        peterAI.PlayThemeSong();

        // TODO
        // random 2 / 3 items?
        peterAI.targets = new Stack<Duplicatable>();
        foreach (Duplicatable dup in GameObject.FindObjectsOfType<Duplicatable>()) {
            if (dup.Nullifiable()) {
                peterAI.targets.Push(dup);
            }
        }
        UINew.Instance.SetActionText("You have been visited by Peter Picklebottom");
        Toolbox.Instance.SwitchAudioListener(GameObject.Find("Main Camera"));
        configured = true;
    }
    public override void Update() {
        if (peterAI.targets.Count == 0 && peterAI.target.val == null) {
            complete = true;
        }
    }
    public override void CleanUp() {
        camControl.focus = GameManager.Instance.playerObject;
        UINew.Instance.RefreshUI(active: true);
        GameObject.Destroy(nightShade);
        peterAI.state = PeterPicklebottom.AIState.leave;
        UINew.Instance.SetActionText("");
    }
}
public class CutsceneBoardroom : Cutscene {
    Regex ampersandHook = new Regex(@"\&\r?$");
    Regex numberHook = new Regex(@"^([\d.]+)");
    Regex lineHook = new Regex(@"^(.*):(.+)");
    Regex endHook = new Regex(@"END");
    private float timer;
    private float globalTimer;
    private float stopTextFade = 1.5f;
    private float startDialogue = 4.5f;
    private float escPromptDelay = 14.5f;
    private bool inDialogue;
    Text settingText;
    Text escPrompt;
    GameObject longShot;
    Speech moe;
    Speech larry;
    Speech curly;
    HeadAnimation moeControl;
    GameObject moeObj;
    List<string> lines = new List<string>();
    int index = 0;
    float scriptTimeSpace = 1f;
    public override void Configure() {
        configured = true;
        longShot = GameObject.Find("long_shot");
        GameObject canvas = GameObject.Find("Canvas");
        settingText = canvas.transform.Find("text").GetComponent<Text>();
        escPrompt = canvas.transform.Find("escPrompt").GetComponent<Text>();
        escPrompt.gameObject.SetActive(false);
        Color blank = new Color(255, 255, 255, 0);
        settingText.color = blank;
        moeObj = GameObject.Find("moe");
        GameObject larryObj = GameObject.Find("larry");
        GameObject curlyObj = GameObject.Find("curly");
        moeObj.GetComponent<Humanoid>().SetDirection(Vector2.down);
        larryObj.GetComponent<Humanoid>().SetDirection(Vector2.right);
        curlyObj.GetComponent<Humanoid>().SetDirection(Vector2.left);
        moeControl = moeObj.GetComponentInChildren<HeadAnimation>();
        moeControl.DirectionChange(Vector2.down);
        moe = moeObj.GetComponent<Speech>();
        larry = larryObj.GetComponent<Speech>();
        curly = curlyObj.GetComponent<Speech>();
    }
    public override void Update() {
        timer += Time.deltaTime;
        globalTimer += Time.deltaTime;
        if (!inDialogue) {
            if (timer < stopTextFade) {
                Color col = settingText.color;
                col.a = (float)PennerDoubleAnimation.ExpoEaseIn(timer, 0, 1, stopTextFade);
                settingText.color = col;
            }
            if (timer >= startDialogue) {
                inDialogue = true;
                longShot.SetActive(false);
                settingText.gameObject.SetActive(false);
                timer = 0;
                globalTimer = 0;
                if (GameManager.Instance.activeCommercial == null) {
                    LoadScript("eat1");
                } else {
                    if (!LoadScript(GameManager.Instance.activeCommercial.cutscene))
                        LoadScript("test");
                }
            }
        } else {
            if (globalTimer > escPromptDelay && globalTimer - escPromptDelay < stopTextFade) {
                escPrompt.gameObject.SetActive(true);
                Color col = escPrompt.color;
                col.a = (float)PennerDoubleAnimation.ExpoEaseIn(globalTimer - escPromptDelay, 0, 1, stopTextFade);
                escPrompt.color = col;
            }
            // dialogue scene
            if (moe.speaking || larry.speaking || curly.speaking) {
                timer = 0;
                return;
            }
            if (timer > scriptTimeSpace) {
                timer = 0;
                ProcessLine();
            }
        }
    }
    void ProcessLine() {
        bool amp = false;
        string line = lines[index];
        if (ampersandHook.IsMatch(line)) {
            amp = true;
            line = line.Substring(0, line.Length - 1);
        }
        if (lineHook.IsMatch(line)) {
            Match match = lineHook.Match(line);
            MessageSpeech message = new MessageSpeech(match.Groups[2].Value);
            if (match.Groups[1].Value == "MOE") {
                moe.Say(message);
            } else if (match.Groups[1].Value == "LARRY") {
                larry.Say(message);
            } else if (match.Groups[1].Value == "CURLY") {
                curly.Say(message);
            }
        }
        if (line == "<LEFT>") {
            Vector3 scale = new Vector3(-1, 1, 1);
            moeObj.transform.localScale = scale;
            moeControl.DirectionChange(Vector2.left);
        } else if (line == "<RIGHT>") {
            Vector3 scale = new Vector3(1, 1, 1);
            moeObj.transform.localScale = scale;
            moeControl.DirectionChange(Vector2.right);
        } else if (line == "<DOWN>") {
            Vector3 scale = new Vector3(1, 1, 1);
            moeObj.transform.localScale = scale;
            moeControl.DirectionChange(Vector2.down);
        } else if (line == "<UP>") {
            Vector3 scale = new Vector3(1, 1, 1);
            moeObj.transform.localScale = scale;
            moeControl.DirectionChange(Vector2.up);
        }
        if (endHook.IsMatch(line)) {
            complete = true;
            GameManager.Instance.NewDayCutscene();
        }
        if (index + 1 < lines.Count - 1) {
            if (numberHook.IsMatch(lines[index + 1])) {
                Match match = numberHook.Match(lines[index + 1]);
                scriptTimeSpace = float.Parse(match.Groups[1].Value);
                index += 1;
            }
        }
        index += 1;
        if (amp)
            ProcessLine();
    }
    bool LoadScript(string filename) {
        TextAsset textData = Resources.Load("data/boardroom/" + filename) as TextAsset;
        if (textData == null) {
            return false;
        } else {
            lines = new List<string>(textData.text.Split('\n'));
            return true;
        }
    }
    public override void EscapePressed() {
        complete = true;
        GameManager.Instance.NewDayCutscene();
    }
}
public class CutsceneDungeonFall : CutsceneFall {
    override protected float fallDist() { return -3.236f; }
    private float catchPoint = 0.317f;
    private bool caught;
    private bool dumping;
    private float dumpTimer;
    private float dumpInterval = 1f;
    private Inventory playerInventory;
    private Outfit playerOutfit;
    private GameObject ejectorDump;
    private Vector2 catchPosition;
    private AudioClip dumpSound;
    public ParticleSystem magicEffect;
    public AudioSource magicAudio;
    public override void Configure() {
        base.Configure();
        ejectorDump = Resources.Load("particles/ejectorDump") as GameObject;
        dumpSound = Resources.Load("sounds/8bit_impact2") as AudioClip;
        Toolbox.Instance.SwitchAudioListener(player);
        magicEffect = GameObject.Find("magic").GetComponent<ParticleSystem>();
        magicAudio = GameObject.Find("magic").GetComponent<AudioSource>();
    }
    public override void Update() {
        if (!caught || (caught && !dumping)) {
            base.Update();
            if (player.transform.position.y < catchPoint && !caught) {
                catchPosition = player.transform.position;
                caught = true;
                dumping = true;
                magicEffect.Play();
                magicAudio.Play();
                magicEffect.transform.position = player.transform.position;
                playerInventory = player.GetComponent<Inventory>();
                playerOutfit = player.GetComponent<Outfit>();
                // stop fall
                if (playerBody) {
                    playerBody.gravityScale = 0f;
                    playerBody.drag = initDrag;
                    playerBody.velocity = Vector3.zero;
                }
            }
        } else if (dumping) {
            DumpUpdate();
        }
    }
    public void DumpUpdate() {
        dumpTimer += Time.deltaTime;
        player.transform.position = catchPosition + UnityEngine.Random.insideUnitCircle * 0.01f;
        // shake
        // rotate
        if (dumpTimer > dumpInterval) {
            dumpTimer = 0;
            if (playerInventory != null && playerInventory.holding != null) {
                GameObject toDump = playerInventory.holding.gameObject;
                playerInventory.SoftDropItem();
                Dump(toDump);
            } else if (playerInventory != null && playerInventory.items.Count > 0) {
                GameObject pickup = playerInventory.items[0];
                playerInventory.items.RemoveAt(0);
                Dump(pickup);
            } else if (playerOutfit != null && playerOutfit.wornUniformName != "nude") {
                // dump outfit
                GameObject removedUniform = playerOutfit.RemoveUniform();
                playerOutfit.GoNude();
                Dump(removedUniform);
            } else {
                player.transform.position = catchPosition;
                dumping = false;
                magicEffect.Stop();
                magicAudio.Stop();
                if (playerBody) {
                    playerBody.gravityScale = 1f;
                    playerBody.drag = 0;
                }
            }
        }
    }
    void Dump(GameObject obj) {
        // play sfx
        SpriteRenderer objSprite = obj.GetComponent<SpriteRenderer>();
        GameObject dumpObject = GameObject.Instantiate(ejectorDump);
        dumpObject.transform.position = player.transform.position;
        ParticleSystem system = dumpObject.GetComponent<ParticleSystem>();
        system.randomSeed = (uint)UnityEngine.Random.Range(0, 9999999);
        system.textureSheetAnimation.SetSprite(0, objSprite.sprite);
        system.Play();
        GameObject.Destroy(obj);
        GameManager.Instance.PlayPublicSound(dumpSound);
    }
}
public class CutsceneFall : Cutscene {
    protected GameObject player;
    AdvancedAnimation playerAnimation;
    Collider2D playerCollider;
    protected Rigidbody2D playerBody;
    Controllable playerControl;
    Hurtable playerHurtable;
    protected float initDrag;
    protected virtual float fallDist() { return -0.3f; }
    public override void Configure() {
        player = GameManager.Instance.playerObject;
        playerAnimation = player.GetComponent<AdvancedAnimation>();
        playerCollider = player.GetComponent<Collider2D>();
        playerBody = player.GetComponent<Rigidbody2D>();
        playerControl = player.GetComponent<Humanoid>();
        playerHurtable = player.GetComponent<Hurtable>();
        if (playerAnimation) {
            playerAnimation.enabled = false;
            playerAnimation.LateUpdate();
        }
        if (playerCollider)
            playerCollider.enabled = false;
        if (playerControl) {
            playerControl.enabled = false;
        }
        if (playerBody) {
            playerBody.gravityScale = 1f;
            initDrag = playerBody.drag;
            playerBody.drag = 0;
        }
        UINew.Instance.RefreshUI();
        configured = true;
    }
    public override void Update() {
        if (player.transform.position.y < fallDist()) {
            Toolbox.Instance.AudioSpeaker("Poof 01", player.transform.position);
            if (playerAnimation)
                playerAnimation.enabled = true;
            if (playerCollider)
                playerCollider.enabled = true;
            if (playerControl) {
                playerControl.enabled = true;
            }
            if (playerBody) {
                playerBody.gravityScale = 0; ;
                playerBody.drag = initDrag;
            }
            if (playerHurtable) {
                playerHurtable.KnockDown();
                playerHurtable.downedTimer = 3f;
            }
            UINew.Instance.RefreshUI(active: true);
            complete = true;
        }
    }
}
public class CutsceneMayor : Cutscene {
    private GameObject spawnPoint;
    private GameObject mayor;
    private Humanoid mayorControl;
    private DecisionMaker mayorAI;
    private Speech mayorSpeech;
    private bool inPosition;
    private bool walkingAway;
    private Dictionary<Controllable, Controllable.ControlType> initialState = new Dictionary<Controllable, Controllable.ControlType>();
    public override void Configure() {
        configured = true;
        spawnPoint = GameObject.Find("mayorSpawnpoint");
        // GameObject farmer = GameObject.Find("Farmer");
        // if (farmer){
        //     Debug.Log("disabling farmer");
        //     farmer.GetComponent<Controllable>().control = Controllable.ControlType.none;
        // }
        foreach (Controllable controllable in GameObject.FindObjectsOfType<Controllable>()) {
            // controllable.
            initialState[controllable] = controllable.control;
            controllable.control = Controllable.ControlType.none;
        }
        mayor = GameObject.Instantiate(Resources.Load("prefabs/Mayor"), spawnPoint.transform.position, Quaternion.identity) as GameObject;
        mayorControl = mayor.GetComponent<Humanoid>();
        mayorAI = mayor.GetComponent<DecisionMaker>();
        mayorSpeech = mayor.GetComponent<Speech>();
        mayorAI.enabled = false;
        Controllable playerController = GameManager.Instance.playerObject.GetComponent<Controllable>();
        playerController.SetDirection(Vector2.down);
        UINew.Instance.RefreshUI();
    }
    public override void Update() {
        if (!inPosition) {
            mayorControl.rightFlag = true;
        }
        if (!inPosition && Vector3.Distance(mayor.transform.position, GameManager.Instance.playerObject.transform.position) < 0.25) {
            inPosition = true;
            mayorControl.ResetInput();
            DialogueMenu menu = mayorSpeech.SpeakWith();
            menu.menuClosed += MenuWasClosed;
            menu.LoadDialogueTree("mayor");
        }
        if (walkingAway) {
            mayorControl.leftFlag = true;
            if (mayor.transform.position.x < spawnPoint.transform.position.x) {
                UnityEngine.Object.Destroy(mayor);
                complete = true;
                Controller.Instance.state = Controller.ControlState.normal;
            }
        }
    }
    public override void CleanUp() {
        UINew.Instance.RefreshUI(active: true);
        foreach (KeyValuePair<Controllable, Controllable.ControlType> kvp in initialState) {
            kvp.Key.control = kvp.Value;
        }
    }
    public void MenuWasClosed() {
        walkingAway = true;
        Controller.Instance.state = Controller.ControlState.cutscene;
        UINew.Instance.RefreshUI(active: false);
    }
}
public class CutsceneNewDay : Cutscene {
    private float timer;
    Text tomText;
    Text dayText;
    GameObject canvas;
    private float stopTomFade = 1.5f;
    private float startDayFade = 1.5f;
    private float stopDayFade = 2.5f;
    private float stopTime = 12f;
    public override void Configure() {
        configured = true;
        canvas = GameObject.Find("Canvas");
        tomText = canvas.transform.Find("tomText").GetComponent<Text>();
        dayText = canvas.transform.Find("dayText").GetComponent<Text>();

        tomText.text = GameManager.Instance.saveGameName + "'s house";
        dayText.text = "Day " + GameManager.Instance.data.days.ToString();

        Color blank = new Color(255, 255, 255, 0);
        tomText.color = blank;
        dayText.color = blank;
    }
    public override void Update() {
        timer += Time.deltaTime;
        if (timer < stopTomFade) {
            Color col = tomText.color;
            col.a = (float)PennerDoubleAnimation.ExpoEaseIn(timer, 0, 1, stopTomFade);
            tomText.color = col;
        }
        if (timer > startDayFade && timer < stopDayFade) {
            Color col = dayText.color;
            col.a = (float)PennerDoubleAnimation.ExpoEaseIn(timer - startDayFade, 0, 1, stopDayFade - startDayFade);
            dayText.color = col;
        }
        if (timer >= stopTime || Input.anyKey) {
            complete = true;
            GameManager.Instance.NewDay();
        }
    }
}

public class CutsceneManager : Singleton<CutsceneManager> {
    public List<Type> lateConfigure = new List<Type>(){
        typeof(CutsceneNewDay),
        typeof(CutsceneBoardroom),
        typeof(CutsceneFall)};
    public Cutscene cutscene;
    void Start() {
        SceneManager.sceneLoaded += LevelWasLoaded;
    }
    public void InitializeCutscene<T>() where T : Cutscene, new() {
        Controller.Instance.state = Controller.ControlState.cutscene;
        cutscene = new T();
        if (!lateConfigure.Contains(typeof(T)))
            cutscene.Configure();
    }
    public void LevelWasLoaded(Scene scene, LoadSceneMode mode) {
        if (cutscene == null)
            return;
        if (cutscene.complete) {
            cutscene.CleanUp();
            cutscene = null;
            return;
        }
        if (cutscene.configured == false) {
            if (lateConfigure.Contains(cutscene.GetType())) {
                cutscene.Configure();
            }
        }
    }
    void Update() {
        if (cutscene == null) {
            return;
        }
        if (cutscene.complete) {
            cutscene.CleanUp();
            cutscene = null;
            Controller.Instance.state = Controller.ControlState.normal;
        } else {
            if (cutscene.configured) {
                cutscene.Update();
            } else {
                cutscene.Configure();
            }
        }
    }
    public void EscapePressed() {
        if (cutscene != null)
            cutscene.EscapePressed();
    }
}
