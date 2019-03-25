﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Easings;
using UnityEngine.EventSystems;
// using System.Linq;

public class UINew : Singleton<UINew> {
    public enum MenuType { none, escape, inventory, speech, closet, scriptSelect, commercialReport, newDayReport, email, diary, dialogue, phone, perk, teleport }
    private enum EasingDirection { none, up, down }
    private Dictionary<MenuType, string> menuPrefabs = new Dictionary<MenuType, string>{
        {MenuType.escape,                   "UI/PauseMenu"},
        {MenuType.inventory,                "UI/InventoryScreen"},
        {MenuType.speech,                   "UI/SpeechMenu"},
        {MenuType.closet,                   "UI/ClosetMenu"},
        {MenuType.scriptSelect,             "UI/ScriptSelector"},
        {MenuType.commercialReport,         "UI/commercialReport"},
        {MenuType.newDayReport,             "UI/NewDayReport"},
        {MenuType.email,                    "UI/EmailUI"},
        {MenuType.diary,                    "UI/Diary"},
        {MenuType.dialogue,                 "UI/DialogueMenu"},
        {MenuType.phone,                    "UI/PhoneMenu"},
        {MenuType.perk,                     "UI/PerkMenu"},
        {MenuType.teleport,                 "UI/TeleportMenu"}
    };
    private static List<MenuType> actionRequired = new List<MenuType> { MenuType.commercialReport, MenuType.diary, MenuType.perk, MenuType.dialogue };
    public GameObject activeMenu;
    private MenuType activeMenuType;
    // private bool menuRequiresAction;
    private struct actionButton {
        public GameObject gameobject;
        public ActionButtonScript buttonScript;
        public Text buttonText;
    }
    public GameObject UICanvas;
    private List<GameObject> activeElements = new List<GameObject>();
    private List<GameObject> bottomElements = new List<GameObject>();
    private GameObject inventoryButton;
    private GameObject fightButton;
    private GameObject punchButton;
    private GameObject speakButton;
    private GameObject saveButton;
    private GameObject loadButton;
    private GameObject testButton;
    private GameObject hypnosisButton;
    private GameObject vomitButton;
    private bool init = false;
    public bool inventoryVisible = false;
    public Text status;
    public Text actionTextObject;
    public Text sceneNameText;
    public string actionTextString;
    private Stack<AchievementPopup.CollectedInfo> collectedStack = new Stack<AchievementPopup.CollectedInfo>();
    private Stack<Achievement> achievementStack = new Stack<Achievement>();
    public bool achievementPopupInProgress;
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

    public void Start() {
        Awake();
    }
    public void Awake() {
        if (!init) {
            ConfigureUIElements();
            init = true;
        }
        cursorDefault = (Texture2D)Resources.Load("UI/cursor3_64_2");
        cursorHighlight = (Texture2D)Resources.Load("UI/cursor3_64_1");
        cursorTarget = (Texture2D)Resources.Load("UI/cursor3_target3");
    }
    void LateUpdate() {
        LateUpdateUIElements();
        bool highlight = false;
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider != null && !Controller.forbiddenColliders.Contains(hit.collider.tag)) {
                highlight = true;
            }
        }
        GameObject target = null;
        if (highlight) {
            GameObject top = Controller.Instance.GetFrontObject(hits);
            target = Controller.Instance.GetBaseInteractive(top.transform);
        }
        bool cursorOverButton = false;
        if (EventSystem.current.IsPointerOverGameObject()) {
            cursorOverButton = true;
        }
        UpdateCursor(highlight);
        UpdateActionText(highlight, target, cursorOverButton);
    }
    public void UpdateCursor(bool highlight) {
        switch (Controller.Instance.state) {
            case Controller.ControlState.normal:
                if (cursorText.activeInHierarchy)
                    cursorText.SetActive(false);
                if (highlight) {
                    Cursor.SetCursor(cursorHighlight, new Vector2(28, 16), CursorMode.Auto);
                } else {
                    Cursor.SetCursor(cursorDefault, new Vector2(28, 16), CursorMode.Auto);
                }
                break;
            case Controller.ControlState.inMenu:
            case Controller.ControlState.waitForMenu:
                if (cursorText.activeInHierarchy)
                    cursorText.SetActive(false);
                Cursor.SetCursor(cursorHighlight, new Vector2(28, 16), CursorMode.Auto);
                break;
            case Controller.ControlState.commandSelect:
                SetCursorText("COMMAND");
                break;
            case Controller.ControlState.hypnosisSelect:
                SetCursorText("HYPNOTIZE");
                break;
            case Controller.ControlState.insultSelect:
                SetCursorText("INSULT");
                break;
            case Controller.ControlState.swearSelect:
                SetCursorText("SWEAR\nAT");
                break;
            default:
                if (cursorText.activeInHierarchy)
                    cursorText.SetActive(false);
                break;
        }
    }
    public void SetCursorText(string text) {
        if (!cursorText.activeInHierarchy)
            cursorText.SetActive(true);
        Vector3 mousePos = Input.mousePosition;
        mousePos.y -= 30;
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(UICanvas.transform as RectTransform, mousePos, GameManager.Instance.cam, out pos);
        cursorText.transform.position = UICanvas.transform.TransformPoint(pos);
        cursorTextText.text = text;

        Cursor.SetCursor(cursorTarget, new Vector2(16, 16), CursorMode.Auto);
    }
    public void UpdateActionText(bool highlight, GameObject target, bool cursorOverButton) {
        if (!activeMenu && CutsceneManager.Instance.cutscene == null) {
            if (activeElements.Count > 1) {
                if (cursorOverButton) {
                    SetActionText(actionButtonText);
                } else {
                    SetActionText(lastTarget);
                }
            } else {
                if (cursorOverButton) {
                    SetActionText(actionButtonText);
                } else if (target != null) {
                    lastTarget = Toolbox.Instance.GetName(target);
                    switch (Controller.Instance.state) {
                        case Controller.ControlState.swearSelect:
                            SetActionText("Swear at " + lastTarget);
                            break;
                        case Controller.ControlState.insultSelect:
                            SetActionText("Insult " + lastTarget);
                            break;
                        case Controller.ControlState.hypnosisSelect:
                            SetActionText("Hypnotize " + lastTarget);
                            break;
                        default:
                            SetActionText(lastTarget);
                            break;
                    }
                } else {
                    SetActionText("");
                }
            }
        } else {
            if (CutsceneManager.Instance.cutscene is CutscenePickleBottom) {
                SetActionText("You have been visited by Peter Picklebottom");
            } else {
                SetActionText("");
            }
        }
    }
    public void LateUpdateUIElements() {
        // handle pop ups, health bars, and display text

        actionTextObject.text = actionTextString;
        if (!achievementPopupInProgress && collectedStack.Count > 0) {
            PopupCollected(collectedStack.Pop());
        }
        if (!achievementPopupInProgress && achievementStack.Count > 0) {
            PopupAchievement(achievementStack.Pop());
        }
        if (Controller.Instance.focusHurtable != null) {
            float width = (Controller.Instance.focusHurtable.health / Controller.Instance.focusHurtable.maxHealth) * lifebarDefaultSize.x;
            lifebar.sizeDelta = new Vector2(width, lifebarDefaultSize.y);
            width = (Controller.Instance.focusHurtable.oxygen / Controller.Instance.focusHurtable.maxOxygen) * oxygenbarDefaultSize.x;
            oxygenbar.sizeDelta = new Vector2(width, oxygenbarDefaultSize.y);
            if (Controller.Instance.focusHurtable.health < Controller.Instance.focusHurtable.maxHealth) {
                HealthBarOn();
            } else {
                HealthBarOff();
            }
            if (Controller.Instance.focusHurtable.oxygen < Controller.Instance.focusHurtable.maxOxygen) {
                OxygenBarOn();
            } else {
                OxygenBarOff();
            }
        }
        if (healthBarEasingDirection != EasingDirection.none) {
            healthBarEasingTimer += Time.deltaTime;
            Vector3 tempPos = topRightRectTransform.anchoredPosition;
            if (healthBarEasingTimer >= 1f) {
                healthBarEasingDirection = EasingDirection.none;
                healthBarEasingTimer = 0f;
            }
            if (healthBarEasingDirection == EasingDirection.up) {
                tempPos.y = (float)PennerDoubleAnimation.ExpoEaseOut(healthBarEasingTimer, 0, 50, 1f);
                topRightRectTransform.anchoredPosition = tempPos;
            }
            if (healthBarEasingDirection == EasingDirection.down) {
                tempPos.y = (float)PennerDoubleAnimation.ExpoEaseOut(healthBarEasingTimer, 50, -50f, 1f);
                topRightRectTransform.anchoredPosition = tempPos;
            }
        }
        if (oxygenBarEasingDirection != EasingDirection.none) {
            oxygenBarEasingTimer += Time.deltaTime;
            Vector3 tempPos = topRightRectTransform.anchoredPosition;
            if (oxygenBarEasingTimer >= 1f) {
                oxygenBarEasingDirection = EasingDirection.none;
                oxygenBarEasingTimer = 0f;
            }
            if (oxygenBarEasingDirection == EasingDirection.up) {
                tempPos.y = (float)PennerDoubleAnimation.ExpoEaseOut(oxygenBarEasingTimer, -50, 100, 1f);
                topRightRectTransform.anchoredPosition = tempPos;
            }
            if (oxygenBarEasingDirection == EasingDirection.down) {
                tempPos.y = (float)PennerDoubleAnimation.ExpoEaseOut(oxygenBarEasingTimer, 50, -100f, 1f);
                topRightRectTransform.anchoredPosition = tempPos;
            }
        }
    }
    public void ConfigureUIElements() {
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
        punchButton = UICanvas.transform.Find("topdock/PunchButton").gameObject;
        speakButton = UICanvas.transform.Find("topdock/SpeakButton").gameObject;
        saveButton = UICanvas.transform.Find("save").gameObject;
        loadButton = UICanvas.transform.Find("load").gameObject;
        testButton = UICanvas.transform.Find("test").gameObject;
        hypnosisButton = UICanvas.transform.Find("topdock/HypnosisButton").gameObject;
        vomitButton = UICanvas.transform.Find("topdock/VomitButton").gameObject;
        cursorText = UICanvas.transform.Find("cursorText").gameObject;
        cursorTextText = cursorText.GetComponent<Text>();
        actionTextObject = UICanvas.transform.Find("ActionText").GetComponent<Text>();
        sceneNameText = UICanvas.transform.Find("sceneText").GetComponent<Text>();
        sceneNameText.enabled = false;
        lifebar = UICanvas.transform.Find("topright/lifebar/mask/fill").GetComponent<RectTransform>();
        oxygenbar = UICanvas.transform.Find("topright/oxygenbar/mask/fill").GetComponent<RectTransform>();
        topRightBar = UICanvas.transform.Find("topright").gameObject;
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
        HidePunchButton();
        if (!GameManager.Instance.debug) {
            if (saveButton)
                saveButton.SetActive(false);
            if (loadButton)
                loadButton.SetActive(false);
            if (testButton)
                testButton.SetActive(false);
        }
    }
    public void HealthBarOn() {
        if (Controller.Instance.focusHurtable.oxygen >= Controller.Instance.focusHurtable.maxOxygen) {
            if (healthBarEasingDirection == EasingDirection.none) {
                if (topRightRectTransform.anchoredPosition.y > 10f) {
                    healthBarEasingTimer = 0f;
                    healthBarEasingDirection = EasingDirection.down;
                }
            }
        }
    }
    public void HealthBarOff() {
        if (Controller.Instance.focusHurtable.oxygen >= Controller.Instance.focusHurtable.maxOxygen) {
            if (healthBarEasingDirection == EasingDirection.none) {
                if (topRightRectTransform.anchoredPosition.y < 40f) {
                    healthBarEasingTimer = 0f;
                    healthBarEasingDirection = EasingDirection.up;
                }
            }
        }
    }
    public void OxygenBarOn() {
        if (oxygenBarEasingDirection == EasingDirection.none) {
            if (topRightRectTransform.anchoredPosition.y > -40f) {
                oxygenBarEasingTimer = 0f;
                oxygenBarEasingDirection = EasingDirection.down;
            }
        }
    }
    public void OxygenBarOff() {
        if (Controller.Instance.focusHurtable.health >= Controller.Instance.focusHurtable.maxHealth) {
            if (oxygenBarEasingDirection == EasingDirection.none) {
                if (topRightRectTransform.anchoredPosition.y < 40f) {
                    oxygenBarEasingTimer = 0f;
                    oxygenBarEasingDirection = EasingDirection.up;
                }
            }
        }
    }
    public void ShowSceneText(string content) {
        // reset alpha visibility here
        sceneNameText.GetComponent<FadeInText>().Reset();
        sceneNameText.enabled = true;
        sceneNameText.gameObject.SetActive(true);
        sceneNameText.text = content;
    }
    public GameObject ShowMenu(MenuType typeMenu) {
        if (activeMenu == null) {
            activeMenuType = MenuType.none;
        }
        if (activeMenuType == typeMenu) {
            CloseActiveMenu();
            return null;
        }
        if (Controller.Instance.state == Controller.ControlState.waitForMenu)
            return null;
        CloseActiveMenu();
        activeMenu = GameObject.Instantiate(Resources.Load(menuPrefabs[typeMenu])) as GameObject;
        Canvas canvas = activeMenu.GetComponent<Canvas>();
        canvas.worldCamera = GameManager.Instance.cam;
        activeMenuType = typeMenu;
        if (actionRequired.Contains(typeMenu)) {
            Controller.Instance.state = Controller.ControlState.waitForMenu;
        } else {
            Controller.Instance.state = Controller.ControlState.inMenu;
        }
        return activeMenu;
    }
    public void CloseActiveMenu() {
        if (activeMenu) {
            activeMenuType = MenuType.none;
            Destroy(activeMenu);
            activeMenu.SendMessage("OnDestroy", options: SendMessageOptions.DontRequireReceiver);
            Time.timeScale = 1f;
            Controller.Instance.MenuClosedCallback();
        }
    }
    public void RefreshUI(bool active = false) {
        List<GameObject> buttons = new List<GameObject>() { inventoryButton, fightButton, punchButton, speakButton, hypnosisButton, vomitButton };
        foreach (GameObject button in buttons) {
            if (button)
                button.SetActive(false);
        }
        topRightBar.SetActive(active);
        ClearWorldButtons();
        ClearActionButtons();
        foreach (Transform child in UICanvas.transform.Find("iconDock")) {
            child.gameObject.SetActive(active);
        }
        if (active)
            UpdateButtons();
    }
    public void UpdateButtons() {
        if (UINew.Instance.activeMenuType != UINew.MenuType.none)
            return;
        if (GameManager.Instance.playerObject == null)
            return;
        // speech button
        if (GameManager.Instance.playerObject.GetComponent<Speech>()) {
            speakButton.SetActive(true);
        }
        // hand action buttons and inventory button
        Inventory inv = GameManager.Instance.playerObject.GetComponent<Inventory>();
        if (inv) {
            UpdateInventoryButton(inv);
        }
        UpdateActionButtons(inv);

        // fight button
        fightButton.SetActive(true);
        // health bar
        if (GameManager.Instance.playerObject.GetComponent<Hurtable>()) {
            topRightBar.SetActive(true);
        }
        // do we have hypnosis?
        hypnosisButton.SetActive(GameManager.Instance.data.perks["hypnosis"]);
        // do we have elective vomiting?
        if (GameManager.Instance.playerObject.GetComponent<Eater>()) {
            vomitButton.SetActive(GameManager.Instance.data.perks["vomit"]);
        }
    }
    public void UpdateActionButtons(Inventory inv) {
        HashSet<Interaction> manualActions = Controller.Instance.focus.UpdateDefaultInteraction();
        List<actionButton> actionButtons = UINew.Instance.CreateActionButtons(new HashSet<Interaction>(manualActions));
        // punch button
        if (Controller.Instance.focus.fightMode) {
            ShowPunchButton();
            return;
        } else {
            HidePunchButton();
            if (Controller.Instance.focus.defaultInteraction != null) {
                foreach (actionButton button in actionButtons) {
                    if (button.buttonScript.action == Controller.Instance.focus.defaultInteraction)
                        MakeButtonDefault(button);
                }
            }
        }

    }
    public void UpdateRecordButtons(Commercial commercial) {
        if (GameManager.Instance.activeCommercial == null) {
            return;
        }
        VideoCamera videoCam = GameObject.FindObjectOfType<VideoCamera>();
        if (commercial.Evaluate(GameManager.Instance.activeCommercial)) {
            videoCam.EnableBubble();
        } else {
            videoCam.DisableBubble();
        }
    }
    public void UpdateInventoryButton(Inventory inventory) {
        if (inventory.items.Count > 0) {
            inventoryButton.SetActive(true);
        } else {
            inventoryButton.SetActive(false);
        }
        if (activeMenuType == MenuType.inventory) {
            CloseActiveMenu();
            ShowInventoryMenu();
        }
    }
    public void ShowPunchButton() {
        punchButton.SetActive(true);
    }
    public void HidePunchButton() {
        punchButton.SetActive(false);
    }
    public void PopupCounter(string text, float initValue, float finalValue, Commercial commercial) {
        GameObject existingPop = GameObject.Find("Poptext(Clone)");
        if (existingPop == null) {
            GameObject pop = Instantiate(Resources.Load("UI/Poptext")) as GameObject;
            Canvas popCanvas = pop.GetComponent<Canvas>();
            popCanvas.worldCamera = GameManager.Instance.cam;

            Poptext poptext = pop.GetComponent<Poptext>();
            poptext.description.Add(text);
            poptext.initValueList.Add(initValue);
            poptext.finalValueList.Add(finalValue);
            poptext.commercial = commercial;
        } else {
            Poptext poptext = existingPop.GetComponent<Poptext>();
            poptext.description.Add(text);
            poptext.initValueList.Add(initValue);
            poptext.finalValueList.Add(finalValue);
        }
    }
    public void PopupCollected(GameObject obj) {
        PopupCollected(new AchievementPopup.CollectedInfo(obj));
    }
    public void PopupCollected(AchievementPopup.CollectedInfo info) {
        GameObject existingPop = GameObject.Find("AchievementPopup(Clone)");
        if (existingPop == null) {
            GameObject pop = Instantiate(Resources.Load("UI/AchievementPopup")) as GameObject;
            Canvas popCanvas = pop.GetComponent<Canvas>();
            popCanvas.worldCamera = GameManager.Instance.cam;
            AchievementPopup achievement = pop.GetComponent<AchievementPopup>();

            achievement.CollectionPopup(info);
            achievementPopupInProgress = true;
        } else {
            collectedStack.Push(info);
        }
    }
    public void PopupAchievement(Achievement achieve) {
        GameObject existingPop = GameObject.Find("AchievementPopup(Clone)");
        if (existingPop == null) {
            GameObject pop = Instantiate(Resources.Load("UI/AchievementPopup")) as GameObject;
            Canvas popCanvas = pop.GetComponent<Canvas>();
            popCanvas.worldCamera = GameManager.Instance.cam;
            AchievementPopup achievement = pop.GetComponent<AchievementPopup>();

            achievement.Achievement(achieve);
            achievementPopupInProgress = true;
        } else {
            achievementStack.Push(achieve);
        }
    }
    public void SetActionText(string text) {
        actionTextString = text;
    }
    public void DisplayHandActions(Inventory inventory) {
        ClearWorldButtons();
        activeElements = new List<GameObject>();
        List<actionButton> buttons = new List<actionButton>();
        for (int i = 1; i <= 3; i++) {
            actionButton newbutton = SpawnButton(null);
            newbutton.buttonScript.manualAction = true;
            newbutton.buttonScript.inventory = inventory;
            if (i == 1) {
                newbutton.buttonScript.bType = ActionButtonScript.buttonType.Drop;
                newbutton.buttonText.text = "Drop";
                newbutton.buttonScript.buttonText = "Drop";
            } else if (i == 2) {
                newbutton.buttonScript.bType = ActionButtonScript.buttonType.Throw;
                newbutton.buttonText.text = "Throw";
                newbutton.buttonScript.buttonText = "Throw";
            } else if (i == 3) {
                newbutton.buttonScript.bType = ActionButtonScript.buttonType.Stash;
                newbutton.buttonText.text = "Stash";
                newbutton.buttonScript.buttonText = "Stash";
            }
            activeElements.Add(newbutton.gameobject);
            buttons.Add(newbutton);
        }
        activeElements.Add(CircularizeButtons(buttons, GameManager.Instance.playerObject));
    }
    public void ShowInventoryMenu() {
        GameObject inventoryMenu = ShowMenu(UINew.MenuType.inventory);
        if (inventoryMenu != null) {
            Inventory inventory = GameManager.Instance.playerObject.GetComponent<Inventory>();
            InventoryMenu menu = inventoryMenu.GetComponent<InventoryMenu>();
            menu.Initialize(inventory);
        }
    }
    public void SetClickedActions(GameObject clickedOn, GameObject clickSite) {
        ClearWorldButtons();
        activeElements = new List<GameObject>();
        HashSet<Interaction> clickedActions = new HashSet<Interaction>();
        if (Controller.Instance.commandTarget != null) {
            clickedActions = Interactor.GetInteractions(Controller.Instance.commandTarget, clickedOn, rightClickMenu: true);
        } else {
            clickedActions = Interactor.GetInteractions(GameManager.Instance.playerObject, clickedOn, rightClickMenu: true);
        }
        List<actionButton> actionButtons = CreateButtonsFromActions(clickedActions); ;
        foreach (actionButton button in actionButtons)
            activeElements.Add(button.gameobject);
        activeElements.Add(CircularizeButtons(actionButtons, clickSite));
    }
    public void ClearWorldButtons() {
        foreach (GameObject element in activeElements)
            Destroy(element);
        activeElements = new List<GameObject>();
    }
    private List<actionButton> CreateButtonsFromActions(HashSet<Interaction> interactions, bool removeColliders = false) {
        List<actionButton> returnList = new List<actionButton>();
        foreach (Interaction interaction in interactions) {
            actionButton newButton = SpawnButton(interaction);
            if (removeColliders) {
                Destroy(newButton.gameobject.GetComponent<CircleCollider2D>());
                Destroy(newButton.gameobject.GetComponent<Rigidbody2D>());
            }
            returnList.Add(newButton);
        }
        return returnList;
    }
    actionButton SpawnButton(Interaction interaction) {
        GameObject newButton = Instantiate(Resources.Load("UI/NeoActionButton"), Vector2.zero, Quaternion.identity) as GameObject;
        ActionButtonScript buttonScript = newButton.GetComponent<ActionButtonScript>();
        buttonScript.button = newButton.GetComponent<Button>();
        Text buttonText = newButton.transform.Find("Text").GetComponent<Text>();
        actionButton returnbut;
        returnbut.gameobject = newButton;
        returnbut.buttonScript = buttonScript;
        returnbut.buttonText = buttonText;
        returnbut.buttonScript.action = interaction;
        if (interaction != null)
            returnbut.buttonText.text = interaction.actionName;
        return returnbut;
    }
    private GameObject CircularizeButtons(List<actionButton> buttons, GameObject target) {
        float incrementAngle = (Mathf.PI * 2f) / buttons.Count;
        float angle = 0f;
        RectTransform canvasRect = UICanvas.GetComponent<RectTransform>();
        Camera renderingCamera = UICanvas.GetComponent<Canvas>().worldCamera;
        GameObject buttonAnchor = Instantiate(Resources.Load("UI/ButtonAnchor"), UICanvas.transform.position, Quaternion.identity) as GameObject;
        Rigidbody2D firstBody = null;
        Rigidbody2D priorBody = null;
        int n = 0;
        Vector2 centerPosition = new Vector2(renderingCamera.pixelWidth / 2f, renderingCamera.pixelHeight / 2f);
        foreach (actionButton button in buttons) {
            Vector2 initLocation = (Vector2)target.transform.position + Toolbox.Instance.RotateZ(Vector2.right / 4, angle);
            Vector2 initPosition = renderingCamera.WorldToScreenPoint(initLocation);
            n++;
            button.gameobject.transform.SetParent(UICanvas.transform, false);
            button.gameobject.transform.SetSiblingIndex(0);
            initPosition = (initPosition - centerPosition) / (renderingCamera.pixelWidth / 800f);
            if (initPosition.y > canvasRect.rect.height / 2f) {
                initPosition.y = canvasRect.rect.height / 2f - 50f;
            }
            if (initPosition.y < canvasRect.rect.height / -2f) {
                initPosition.y = canvasRect.rect.height / -2f + 50f;
            }
            if (initPosition.x > canvasRect.rect.width / 2f) {
                initPosition.x = canvasRect.rect.width / 2f - 50f;
            }
            if (initPosition.x < canvasRect.rect.width / -2f) {
                initPosition.x = canvasRect.rect.width / -2f + 50f;
            }
            button.gameobject.transform.localPosition = initPosition;
            if (priorBody) {
                SpringJoint2D spring = button.gameobject.AddComponent<SpringJoint2D>();
                spring.autoConfigureDistance = false;
                spring.dampingRatio = 0.9f;
                spring.distance = 0.5f;
                spring.connectedBody = priorBody;
            }
            if (!firstBody)
                firstBody = button.gameobject.GetComponent<Rigidbody2D>();
            // set up spring connection to anchor
            SpringJoint2D anchorSpring = buttonAnchor.AddComponent<SpringJoint2D>();
            anchorSpring.autoConfigureDistance = false;
            anchorSpring.distance = 0.25f;
            anchorSpring.dampingRatio = 0.9f;
            anchorSpring.frequency = 15f;
            anchorSpring.connectedBody = button.gameobject.GetComponent<Rigidbody2D>();
            // connect buttons in circular sequence
            if (n == buttons.Count && n > 2) {
                SpringJoint2D finalSpring = button.gameobject.AddComponent<SpringJoint2D>();
                finalSpring.autoConfigureDistance = false;
                finalSpring.distance = 0.5f;
                finalSpring.dampingRatio = 0.9f;
                finalSpring.connectedBody = firstBody;
            }
            priorBody = button.gameobject.GetComponent<Rigidbody2D>();
            angle += incrementAngle;
        }
        buttonAnchor.transform.position = renderingCamera.ScreenToWorldPoint(Input.mousePosition);
        buttonAnchor.transform.SetParent(target.transform);
        return buttonAnchor;
    }
    private void ArrangeButtonsOnScreenTop(List<actionButton> buttons) {
        GameObject bottomBar = UICanvas.transform.Find("topdock").gameObject;
        foreach (actionButton button in buttons) {
            ContentSizeFitter buttonSizeFitter = button.gameobject.GetComponent<ContentSizeFitter>();
            if (buttonSizeFitter) {
                buttonSizeFitter.enabled = false;
            }
            button.gameobject.transform.SetParent(bottomBar.transform, false);
            button.gameobject.transform.SetSiblingIndex(3);
            RectTransform buttonRect = button.gameobject.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(100, 40);
        }
    }
    private void MakeButtonDefault(actionButton button) {
        GameObject indicator = Instantiate(Resources.Load("UI/defaultButtonIndicator")) as GameObject;
        indicator.transform.SetParent(button.gameobject.transform, false);
        indicator.transform.SetAsLastSibling();
    }
    private List<actionButton> CreateActionButtons(HashSet<Interaction> manualActions) {
        ClearActionButtons();
        List<actionButton> manualButtons = CreateButtonsFromActions(manualActions, true);
        foreach (actionButton button in manualButtons) {
            bottomElements.Add(button.gameobject);
            button.buttonScript.manualAction = true;
        }
        ArrangeButtonsOnScreenTop(manualButtons);
        return manualButtons;
    }
    public void ClearActionButtons() {
        foreach (GameObject element in bottomElements)
            Destroy(element);
        bottomElements = new List<GameObject>();
    }
    public void BounceText(string text, GameObject target) {
        GameObject bounce = Instantiate(Resources.Load("UI/BounceText")) as GameObject;
        BounceText bounceScript = bounce.GetComponent<BounceText>();
        if (target) {
            bounceScript.target = target;
        }
        bounceScript.text = text;
    }
    public void ClearStatusIcons() {
        foreach (Transform child in UICanvas.transform.Find("iconDock")) {
            Destroy(child.gameObject);
        }
    }
    public void AddStatusIcon(Buff buff) {
        GameObject icon = Instantiate(Resources.Load("UI/StatusIcon")) as GameObject;
        UIStatusIcon statusIcon = icon.GetComponent<UIStatusIcon>();
        statusIcon.Initialize(buff.type, buff);
        statusIcon.transform.SetParent(UICanvas.transform.Find("iconDock"), false);
    }
    public void PlayUISound(string path) {
        CameraControl camControl = FindObjectOfType<CameraControl>();
        camControl.audioSource.PlayOneShot(Resources.Load(path) as AudioClip);
    }
}
