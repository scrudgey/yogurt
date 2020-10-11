using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Easings;
using UnityEngine.EventSystems;

public partial class UINew : Singleton<UINew> {

    private enum EasingDirection { none, up, down }

    public GameObject UICanvas;
    private List<GameObject> activeWorldButtons = new List<GameObject>();
    private List<GameObject> activeTopButtons = new List<GameObject>();
    private GameObject inventoryButton;
    private GameObject fightButton;
    private Text fightButtonText;
    private GameObject punchButton;
    private GameObject speakButton;
    private GameObject saveButton;
    private GameObject musicToggle;
    private GameObject loadButton;
    private GameObject testButton;
    private GameObject hypnosisButton;
    private GameObject vomitButton;
    private GameObject teleportButton;
    private bool init = false;
    public bool inventoryVisible = false;
    public Text status;
    public Text actionTextObject;
    public Text sceneNameText;
    public string actionTextString;
    public Texture2D cursorDefault;
    public Texture2D cursorHighlight;
    public Texture2D cursorTarget;
    public RectTransform lifebar;
    public RectTransform oxygenbar;
    private Vector2 lifebarDefaultSize;
    private Vector2 oxygenbarDefaultSize;
    public GameObject topRightBar;
    public GameObject cursorText;
    public Text cursorTextText;
    private RectTransform topRightRectTransform;
    private float healthBarEasingTimer;
    private float oxygenBarEasingTimer;
    private EasingDirection healthBarEasingDirection;
    private EasingDirection oxygenBarEasingDirection;
    private string lastTarget;
    public string actionButtonText;
    public UIHitIndicator hitIndicator;
    public Transform objectivesContainer;
    public Transform iconDock;
    public GameObject buttonAnchor;
    public FadeInOut fader;
    public List<string> previousTopButtons = new List<string>();
    public AudioClip mouseOverObjectSound;
    public AudioClip actionMenuOpenSound;
    public AudioClip actionButtonPressedSound;
    public AudioSource audioSource;
    public void Start() {
        Awake();
    }
    public void Awake() {
        if (!init) {
            ConfigureUIElements();
            init = true;
        }
        // Sprite[] sprites = Resources.LoadAll<Sprite>("UI/Cursor2_128");
        // cursorDefault = sprites[0].texture;
        // cursorHighlight = sprites[1].texture;


        cursorDefault = (Texture2D)Resources.Load("UI/cursor_128_2");
        cursorHighlight = (Texture2D)Resources.Load("UI/cursor_128_1");

        // cursorDefault = (Texture2D)Resources.Load("UI/cursor3_64_2");
        // cursorHighlight = (Texture2D)Resources.Load("UI/cursor3_64_1");
        cursorTarget = (Texture2D)Resources.Load("UI/cursor3_target3");
    }
    public void ConfigureUIElements() {
        actionMenuOpenSound = Resources.Load("sounds/UI/plunger-pop-4") as AudioClip;
        // actionMenuOpenSound = Resources.Load("sounds/UI/plunger-ffpop") as AudioClip;
        audioSource = Toolbox.Instance.SetUpGlobalAudioSource(gameObject);


        init = true;
        UICanvas = GameObject.Find("NeoUICanvas");
        if (UICanvas == null) {
            UICanvas = GameObject.Instantiate(Resources.Load("required/NeoUICanvas")) as GameObject;
            UICanvas.name = Toolbox.Instance.CloneRemover(UICanvas.name);
        }
        GameObject.DontDestroyOnLoad(UICanvas);
        UICanvas.GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
        inventoryButton = UICanvas.transform.Find("topdock/InventoryButton").gameObject;
        fightButton = UICanvas.transform.Find("topdock/FightButton").gameObject;
        fightButtonText = fightButton.GetComponentInChildren<Text>();
        punchButton = UICanvas.transform.Find("topdock/PunchButton").gameObject;
        speakButton = UICanvas.transform.Find("topdock/SpeakButton").gameObject;
        hypnosisButton = UICanvas.transform.Find("topdock/HypnosisButton").gameObject;
        vomitButton = UICanvas.transform.Find("topdock/VomitButton").gameObject;
        teleportButton = UICanvas.transform.Find("topdock/TeleportButton").gameObject;
        saveButton = UICanvas.transform.Find("save").gameObject;
        loadButton = UICanvas.transform.Find("load").gameObject;
        testButton = UICanvas.transform.Find("test").gameObject;
        musicToggle = UICanvas.transform.Find("musicToggle").gameObject;
        cursorText = UICanvas.transform.Find("cursorText").gameObject;
        cursorTextText = cursorText.GetComponent<Text>();
        actionTextObject = UICanvas.transform.Find("ActionText").GetComponent<Text>();
        sceneNameText = UICanvas.transform.Find("sceneText").GetComponent<Text>();
        sceneNameText.enabled = false;
        lifebar = UICanvas.transform.Find("topright/lifebar/mask/fill").GetComponent<RectTransform>();
        oxygenbar = UICanvas.transform.Find("topright/oxygenbar/mask/fill").GetComponent<RectTransform>();
        topRightBar = UICanvas.transform.Find("topright").gameObject;
        hitIndicator = UICanvas.transform.Find("hitIndicator").GetComponent<UIHitIndicator>();
        objectivesContainer = UICanvas.transform.Find("objectives");
        fader = UICanvas.transform.Find("fader").GetComponent<FadeInOut>();
        iconDock = UICanvas.transform.Find("iconDock");
        topRightRectTransform = topRightBar.GetComponent<RectTransform>();
        if (lifebarDefaultSize == Vector2.zero)
            lifebarDefaultSize = new Vector2(lifebar.rect.width, lifebar.rect.height);
        if (oxygenbarDefaultSize == Vector2.zero)
            oxygenbarDefaultSize = new Vector2(oxygenbar.rect.width, oxygenbar.rect.height);
        inventoryButton.SetActive(false);
        fightButton.SetActive(false);
        hypnosisButton.SetActive(false);
        speakButton.SetActive(false);
        topRightBar.SetActive(false);
        cursorText.SetActive(false);
        vomitButton.SetActive(false);
        teleportButton.SetActive(false);
        HidePunchButton();
        if (!GameManager.Instance.debug) {
            if (saveButton)
                saveButton.SetActive(false);
            if (loadButton)
                loadButton.SetActive(false);
            if (testButton)
                testButton.SetActive(false);
            if (musicToggle)
                musicToggle.SetActive(false);
        }
    }

    public void RefreshUI(bool active = false) {
        List<GameObject> buttons = new List<GameObject>() { inventoryButton, fightButton, punchButton, speakButton, hypnosisButton, vomitButton, teleportButton };
        foreach (GameObject button in buttons) {
            if (button)
                button.SetActive(false);
        }

        // health / oxygen bars
        topRightBar.SetActive(false);

        // action buttons
        ClearWorldButtons();
        ClearTopButtons();

        // objectives
        UpdateObjectives();

        // buff effects
        foreach (Transform child in iconDock) {
            child.gameObject.SetActive(active);
        }

        if (active) {
            // top action buttons
            UpdateTopActionButtons();

            // health bar
            if (GameManager.Instance.playerObject.GetComponent<Hurtable>()) {
                topRightBar.SetActive(true);
            }
        }

    }
}
