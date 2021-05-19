using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Easings;
using UnityEngine.SceneManagement;
using TMPro;
public class CutscenePostCreditsDeath : Cutscene {
    enum State { explode, fadeout, translate, transform, end, goodEnd1, goodEnd2 }
    State state;
    private float timer;
    Controller satanController;
    Controller magicianController;
    GameObject satanObj;
    GameObject playerObj;
    GameObject magicianObj;
    SpriteRenderer[] background;
    Vector3 playerPosition;
    Vector3 satanPosition;
    Camera camera;
    SpriteRenderer[] satanRenderers;
    SpriteRenderer[] magicianRenderers;
    Material textMaterial;
    TextMeshProUGUI panel1Text;
    Image panel1Image;
    Vector3 camInitialPosition;
    readonly float SpiralInTime = 15f;
    bool startGoodEnding;
    bool playedRisingTone;
    BackgroundColorController backgroundColorController;
    TextMeshProUGUI poem1;
    TextMeshProUGUI poem2;
    TextMeshProUGUI poem3;
    TextMeshProUGUI poem4;
    public override void Configure() {
        UINew.Instance.RefreshUI(active: false);
        playerObj = GameManager.Instance.playerObject;
        Debug.Log("starting post credits death");
        // stop music
        MusicController.Instance.StopMusic();
        configured = true;
        satanObj = GameObject.Find("Satan");
        if (satanObj == null) {
            CreateFreshSatan();
        }
        if (playerObj == null) {
            CreateFreshPlayer();
        }

        CameraControl camControl = GameObject.FindObjectOfType<CameraControl>();
        camControl.enabled = false;
        camera = camControl.GetComponent<Camera>();

        GameObject.Find("throne").SetActive(false);

        background = GameObject.Find("background").GetComponentsInChildren<SpriteRenderer>();
        state = State.explode;

        SceneManager.sceneLoaded += SceneWasLoaded;
        camInitialPosition = camControl.transform.position;

        GameManager.Instance.PlayPublicSound(Resources.Load("sounds/8-bit/sfx_exp_various7") as AudioClip);

        backgroundColorController = new BackgroundColorController(camera);

        InputController.Instance.suspendInput = true;

        foreach (Collider2D collider in playerObj.GetComponentsInChildren<Collider2D>()) {
            collider.enabled = false;
        }
    }
    public void SceneWasLoaded(Scene scene, LoadSceneMode mode) {
        UINew.Instance.Clear();
        state = State.goodEnd1;

        panel1Image = GameObject.Find("Canvas/panel1/logo").GetComponent<Image>();
        panel1Text = GameObject.Find("Canvas/panel1/text").GetComponent<TextMeshProUGUI>();

        poem1 = GameObject.Find("Canvas/panel2/text1").GetComponent<TextMeshProUGUI>();
        poem2 = GameObject.Find("Canvas/panel2/text2").GetComponent<TextMeshProUGUI>();
        poem3 = GameObject.Find("Canvas/panel2/text3").GetComponent<TextMeshProUGUI>();
        poem4 = GameObject.Find("Canvas/panel2/text4").GetComponent<TextMeshProUGUI>();

        timer = 0f;
    }
    void CreateFreshSatan() {
        if (satanObj != null) {
            GameObject.Destroy(satanObj);
        }
        satanObj = GameObject.Instantiate(Resources.Load("prefabs/Satan"), satanPosition, Quaternion.identity) as GameObject;
        satanObj.transform.localScale = new Vector3(-1f, 1f, 1f);
        satanController = new Controller(satanObj);
        DisableAllComponents(satanObj);
    }
    void CreateFreshPlayer() {
        if (playerObj != null) {
            GameObject.Destroy(playerObj);
        }
        playerObj = GameManager.Instance.InstantiatePlayerPrefab();
        GameManager.Instance.SetFocus(playerObj);
        playerObj.transform.position = GameManager.Instance.lastPlayerPosition;
        DisableAllComponents(playerObj);
        UINew.Instance.RefreshUI(active: false);
    }
    public override void Update() {
        timer += Time.deltaTime;
        backgroundColorController.Update();
        switch (state) {
            case State.explode:
                float severity = (float)PennerDoubleAnimation.ExpoEaseOut(timer, 0.65f, -0.65f, 5f);

                Vector3 shakeVector = Random.insideUnitCircle * severity;
                Vector3 tempPos = camInitialPosition + shakeVector;
                tempPos.z = -10;
                camera.transform.position = tempPos;

                if (timer > 5f) {
                    state = State.fadeout;
                    timer = 0f;
                }
                break;
            case State.fadeout:
                float alpha = (float)PennerDoubleAnimation.CircEaseIn(timer, 1f, -1, 3f);
                SetBackgroundColor(alpha);
                if (timer > 3f) {
                    SetBackgroundColor(0);
                    StartTranslate();
                }
                break;
            case State.translate:
                // first, determine the proper translation positions
                playerObj.transform.position = Vector2.MoveTowards(playerObj.transform.position, playerPosition, 0.01f);
                satanObj.transform.position = Vector2.MoveTowards(satanObj.transform.position, satanPosition, 0.01f);

                bool playerInPosition = Vector2.Distance(playerObj.transform.position, playerPosition) < 0.01f;
                bool satanInPosition = Vector2.Distance(satanObj.transform.position, satanPosition) < 0.01f;
                if (playerInPosition && satanInPosition) {
                    StartTransform();
                }
                break;
            case State.transform:
                bool showMagician = false;
                float maxTime = 5f;
                float offset = 0.1f;
                float period = maxTime - timer;
                float omega = (2 * Mathf.PI) / (period);

                showMagician = Mathf.Sign(Mathf.Sin(timer * omega)) < 0;

                SetRenderersEnabled(satanRenderers, !showMagician);
                SetRenderersEnabled(magicianRenderers, showMagician);
                if (timer > maxTime - offset) {
                    SetRenderersEnabled(satanRenderers, false);
                    SetRenderersEnabled(magicianRenderers, true);

                }
                if (timer > maxTime + 2f) {
                    state = State.end;
                    Speech playerSpeech = playerObj.GetComponent<Speech>();
                    Speech magicianSpeech = magicianObj.GetComponent<Speech>();
                    magicianSpeech.defaultMonologue = "magician_end";
                    DialogueMenu menu = magicianSpeech.SpeakWith();
                    menu.cutsceneDialogue = true;
                    timer = 0f;
                }
                break;
            case State.end:
                UINew.Instance.RefreshUI(active: false);
                // float radius = camera.pixelHeight / 2f;
                float radius = (float)PennerDoubleAnimation.ExpoEaseIn(timer, camera.pixelHeight / 2f, -1f * camera.pixelHeight / 2f, SpiralInTime);
                float frequency = (float)PennerDoubleAnimation.ExpoEaseIn(timer, 0.1f, 10f, SpiralInTime);
                Vector2 mid = new Vector2(camera.pixelWidth / 2f, camera.pixelHeight / 2f);
                float phi = Mathf.PI;
                Vector2 playerPos = new Vector2(Mathf.Cos(timer * frequency + phi), Mathf.Sin(timer * frequency + phi)) * radius + mid;
                Vector2 satanPos = new Vector2(Mathf.Cos(timer * frequency + Mathf.PI + phi), Mathf.Sin(timer * frequency + Mathf.PI + phi)) * radius + mid;

                if (timer > SpiralInTime - 9f && !playedRisingTone) {
                    playedRisingTone = true;
                    // rising_synth.ogg
                    GameManager.Instance.PlayPublicSound(Resources.Load("sounds/rising_synth") as AudioClip);
                }

                playerPos = camera.ScreenToWorldPoint(playerPos);
                satanPos = camera.ScreenToWorldPoint(satanPos);


                playerObj.transform.position = Vector2.Lerp(playerObj.transform.position, playerPos, 1f);
                magicianObj.transform.position = Vector2.Lerp(magicianObj.transform.position, satanPos, 1f);

                if (timer > SpiralInTime) {
                    GameManager.Instance.data.state = GameState.normal;
                    GameManager.Instance.DoLeaveScene("good_ending", 1);
                    // complete = true;
                    state = State.goodEnd1;
                }
                break;
            case State.goodEnd1:
                float logoAlpha = 0f;
                float textAlpha = 0f;
                if (timer > 2f && !startGoodEnding) {
                    startGoodEnding = true;
                    timer = 0f;
                }
                if (!startGoodEnding) {
                    panel1Image.color = new Color(1f, 1f, 1f, 0);
                    panel1Text.outlineColor = new Color(0f, 0f, 0f, 0);
                    break;
                }
                logoAlpha = (float)PennerDoubleAnimation.QuadEaseIn(timer, 0f, 1f, 5f);

                if (timer > 6f) {
                    textAlpha = (float)PennerDoubleAnimation.QuadEaseIn(timer - 6f, 0f, 1f, 5f);
                }
                if (timer > 13f) {
                    logoAlpha = (float)PennerDoubleAnimation.QuadEaseIn(timer - 13f, 1f, -1f, 5f);
                    textAlpha = (float)PennerDoubleAnimation.QuadEaseIn(timer - 13f, 1f, -1f, 5f);
                }
                if (timer > 20f) {
                    timer = 0f;
                    state = State.goodEnd2;
                }
                // Debug.Log($"{logoAlpha} {textAlpha}");
                Color logoColor = new Color(1f, 1f, 1f, logoAlpha);
                Color textColor = new Color(0f, 0f, 0f, textAlpha);
                panel1Image.color = logoColor;
                panel1Text.outlineColor = textColor;
                break;
            case State.goodEnd2:

                if (timer > 15f) {
                    poem1.color = new Color(0f, 0f, 0f, (float)PennerDoubleAnimation.QuadEaseIn(timer - 15f, 1f, -1f, 7f));
                    poem2.color = new Color(0f, 0f, 0f, (float)PennerDoubleAnimation.QuadEaseIn(timer - 15f, 1f, -1f, 7f));
                    poem3.color = new Color(0f, 0f, 0f, (float)PennerDoubleAnimation.QuadEaseIn(timer - 15f, 1f, -1f, 7f));
                    poem4.color = new Color(0f, 0f, 0f, (float)PennerDoubleAnimation.QuadEaseIn(timer - 15f, 1f, -1f, 7f));
                } else {
                    poem1.color = new Color(0f, 0f, 0f, (float)PennerDoubleAnimation.QuadEaseIn(timer, 0f, 1f, 1f));
                    if (timer > 4f) {
                        poem2.color = new Color(0f, 0f, 0f, (float)PennerDoubleAnimation.QuadEaseIn(timer - 4f, 0f, 1f, 1f));
                    }
                    if (timer > 8f) {
                        poem3.color = new Color(0f, 0f, 0f, (float)PennerDoubleAnimation.QuadEaseIn(timer - 8f, 0f, 1f, 1f));
                    }
                    if (timer > 8.5f) {
                        poem4.color = new Color(0f, 0f, 0f, (float)PennerDoubleAnimation.QuadEaseIn(timer - 8.5f, 0f, 1f, 1f));
                    }
                }

                if (timer > 24f) {
                    complete = true;
                    SceneManager.LoadScene("title");
                }

                break;
            default:
                break;
        }
    }
    void ResurrectAll() {
        Hurtable satanHurtable = satanObj.GetComponent<Hurtable>();
        if (satanHurtable.hitState >= Controllable.HitState.unconscious) {
            CreateFreshSatan();
        } else {
            satanController = new Controller(satanObj);
        }
        Hurtable playerHurtable = playerObj.GetComponent<Hurtable>();
        if (playerHurtable.hitState >= Controllable.HitState.unconscious) {
            CreateFreshPlayer();
        }
    }
    void SetRenderersEnabled(SpriteRenderer[] spriteRenderers, bool enabled) {
        foreach (SpriteRenderer renderer in spriteRenderers) {
            if (renderer.gameObject.name.ToLower().Contains("sightcone")) {
                renderer.enabled = false;
                continue;
            }
            renderer.enabled = enabled;
        }
    }
    void SetBackgroundColor(float alpha) {
        foreach (SpriteRenderer bkg in background) {
            bkg.color = new Color(Color.white.r, Color.white.g, Color.white.b, alpha);
        }
    }
    void DisableAllComponents(GameObject target) {
        foreach (MonoBehaviour component in target.GetComponentsInChildren<MonoBehaviour>()) {
            if (component as Speech != null) {
                continue;
            }
            if (component as HeadAnimation != null)
                continue;
            if (component as AdvancedAnimation != null)
                continue;
            if (component as Text != null)
                continue;
            component.enabled = false;
        }
        foreach (Collider2D collider in target.GetComponentsInChildren<Collider2D>()) {
            collider.enabled = false;
        }
        Rigidbody2D rigidbody = target.GetComponent<Rigidbody2D>();
        if (rigidbody != null) {
            rigidbody.simulated = false;
        }
    }
    void StartTranslate() {
        state = State.translate;

        playerPosition = camera.ScreenToWorldPoint(new Vector2(camera.pixelWidth / 4f, camera.pixelHeight / 2f));
        playerPosition.z = 0;


        satanPosition = camera.ScreenToWorldPoint(new Vector2(3f * camera.pixelWidth / 4f, camera.pixelHeight / 2f));
        satanPosition.z = 0;
    }
    void StartTransform() {
        ResurrectAll();
        state = State.transform;
        Debug.Log("start transform sequence");
        timer = 0f;
        magicianObj = GameObject.Instantiate(Resources.Load("prefabs/magician"), satanPosition, Quaternion.identity) as GameObject;
        magicianController = new Controller(magicianObj);
        magicianController.SetDirection(Vector2.left);


        DisableAllComponents(magicianObj);
        magicianRenderers = magicianObj.GetComponentsInChildren<SpriteRenderer>();
        satanRenderers = satanObj.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in magicianRenderers) {
            renderer.enabled = false;
        }
        Vector3 newPos = camera.transform.position;
        newPos.z = 0;
        GameObject fx = GameObject.Instantiate(Resources.Load("particles/spaceStars")) as GameObject;
        fx.transform.position = newPos;
    }
    public override void EscapePressed() {
        // complete = true;
        // GameManager.Instance.NewDayCutscene();
    }
    public override void CleanUp() {
        base.CleanUp();
        satanController.Deregister();
        magicianController.Deregister();
    }
}

public class BackgroundColorController {
    enum State { normal, colorLerp }
    State state;
    public static List<Color> ColorSequence = new List<Color> {
        Color.blue,
        Color.green,
        Color.red,
        Color.magenta,
        Color.yellow,
        Color.cyan
    };
    int index;
    private Camera cam;
    private HSBColor color;
    float indexChangeInterval;
    float timer;
    private Color targetColor;
    public BackgroundColorController(Camera camera) {
        this.cam = camera;
        color = HSBColor.FromColor(cam.backgroundColor);
        indexChangeInterval = 3f;
    }

    public void Update() {
        timer += Time.deltaTime;
        // Debug.Log($"{state} {timer}");
        switch (state) {
            case State.normal:
                if (timer > indexChangeInterval) {
                    state = State.colorLerp;
                    timer = 0f;
                    index += 1;
                    if (index >= ColorSequence.Count) index = 0;
                    targetColor = ColorSequence[index];
                    indexChangeInterval *= 0.99f;
                }
                color.h += Time.deltaTime / 30f;
                if (color.h > 1)
                    color.h -= 1f;
                break;
            case State.colorLerp:
                Color current = color.ToColor();
                Color newColor = Color.Lerp(current, targetColor, 0.1f);
                if (ApproximatelyEqual(current, newColor)) {
                    // Debug.Log("state: normal");
                    state = State.normal;
                    timer = 0f;
                }
                color = HSBColor.FromColor(newColor);
                break;
        }
        if (cam != null)
            cam.backgroundColor = color.ToColor();
    }

    static bool ApproximatelyEqual(Color c1, Color c2) {
        return ((int)(c1.r * 1000) == (int)(c2.r * 1000)) &&
        ((int)(c1.g * 1000) == (int)(c2.g * 1000)) &&
        ((int)(c1.b * 1000) == (int)(c2.b * 1000));

    }
}