﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UINew: Singleton<UINew> {
	public enum MenuType{none, escape, inventory, speech, closet, scriptSelect, commercialReport, newDayReport, email, diary, dialogue, phone}
	private Dictionary<MenuType, string> menuPrefabs = new Dictionary<MenuType, string>{
		{MenuType.escape, 					"UI/PauseMenu"},
		{MenuType.inventory, 				"UI/InventoryScreen"},
		{MenuType.speech, 					"UI/SpeechMenu"},
		{MenuType.closet, 					"UI/ClosetMenu"},
		{MenuType.scriptSelect, 			"UI/ScriptSelector"},
		{MenuType.commercialReport, 		"UI/commercialReport"},
		{MenuType.newDayReport, 			"UI/NewDayReport"},
		{MenuType.email, 					"UI/EmailUI"},
		{MenuType.diary,					"UI/Diary"},
		{MenuType.dialogue,					"UI/DialogueMenu"},
		{MenuType.phone,					"UI/PhoneMenu"}
	};
	private static List<MenuType> actionRequired = new List<MenuType>{MenuType.commercialReport, MenuType.diary};
	private GameObject activeMenu;
	private MenuType activeMenuType;
	private bool menuRequiresAction;
	private struct actionButton{
		public GameObject gameobject;
		public ActionButtonScript buttonScript;
		public Text buttonText;
	}
	public string MOTD = "smoke weed every day";
	public GameObject UICanvas;
	private List<GameObject> activeElements = new List<GameObject>();
	private List<GameObject> bottomElements = new List<GameObject>();
	private GameObject inventoryButton;
	private GameObject fightButton;
	private GameObject punchButton;
	private GameObject speakButton;	
	private bool init = false;
	public bool inventoryVisible = false;
	public Text status;
	public Text actionTextObject;
	public string actionTextString;
	private Stack<AchievementPopup.CollectedInfo> collectedStack = new Stack<AchievementPopup.CollectedInfo>();
	private Stack<Achievement> achievementStack = new Stack<Achievement>();
	public bool achievementPopupInProgress;
	public Texture2D cursorDefault;
	public Texture2D cursorHighlight;
	public RectTransform lifebar;
	private Vector2 lifebarDefaultSize;
	public GameObject topRightBar;
	public void Start(){
		if (!init){
			ConfigureUIElements();
			init = true;
		}
		cursorDefault = (Texture2D)Resources.Load("UI/cursor3_64_2");
		cursorHighlight = (Texture2D)Resources.Load("UI/cursor3_64_1");
	}
	void Update(){
		if (!achievementPopupInProgress && collectedStack.Count > 0){
			PopupCollected(collectedStack.Pop());
		}
		if (!achievementPopupInProgress && achievementStack.Count > 0){
			PopupAchievement(achievementStack.Pop());
		}
		actionTextObject.text = actionTextString;
		bool highlight = false;
		RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        foreach (RaycastHit2D hit in hits){
            if (hit.collider != null && !Controller.Instance.forbiddenColliders.Contains(hit.collider.tag)){
                highlight = true;
            }
        }
		if (highlight){
			Cursor.SetCursor(cursorHighlight, new Vector2(28, 16), CursorMode.Auto);
		} else {
			Cursor.SetCursor(cursorDefault, new Vector2(28, 16), CursorMode.Auto);
		}
		if (Controller.Instance.focusHurtable != null){
			float width = (Controller.Instance.focusHurtable.health / Controller.Instance.focusHurtable.maxHealth) * lifebarDefaultSize.x;
			lifebar.sizeDelta = new Vector2(width, lifebarDefaultSize.y);
		}
	}
	public void ConfigureUIElements() {
		init = true;
		UICanvas = GameObject.Find("NeoUICanvas");
		if (UICanvas == null){
			UICanvas = GameObject.Instantiate(Resources.Load("required/NeoUICanvas")) as GameObject;
			UICanvas.name = Toolbox.Instance.CloneRemover(UICanvas.name);
		}
		GameObject.DontDestroyOnLoad(UICanvas);
		UICanvas.GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
		inventoryButton = UICanvas.transform.Find("topdock/InventoryButton").gameObject;
		fightButton = UICanvas.transform.Find("topdock/FightButton").gameObject;
		punchButton = UICanvas.transform.Find("topdock/PunchButton").gameObject;
		speakButton = UICanvas.transform.Find("topdock/SpeakButton").gameObject;
		actionTextObject = UICanvas.transform.Find("bottomdock/ActionText").GetComponent<Text>();
		lifebar = UICanvas.transform.Find("topright/lifebar/mask/fill").GetComponent<RectTransform>();
		topRightBar = UICanvas.transform.Find("topright").gameObject;
		if (lifebarDefaultSize == Vector2.zero)
			lifebarDefaultSize = new Vector2(lifebar.rect.width, lifebar.rect.height);
		inventoryButton.SetActive(false);
		fightButton.SetActive(false);
		speakButton.SetActive(false);
		topRightBar.SetActive(false);
		HidePunchButton();
	}
	public GameObject ShowMenu(MenuType typeMenu){
		if (activeMenu == null){
			menuRequiresAction = false;
			activeMenuType = MenuType.none;
		}
		if (activeMenuType == typeMenu){
			CloseActiveMenu();
			return null;
		}
		if (menuRequiresAction)
			return null;
		CloseActiveMenu();
		activeMenu = GameObject.Instantiate(Resources.Load(menuPrefabs[typeMenu])) as GameObject;
		activeMenuType = typeMenu;
		if (actionRequired.Contains(typeMenu))
			menuRequiresAction = true;
		return activeMenu;
	}
	public void CloseActiveMenu(){
		if (activeMenu){
			menuRequiresAction = false;
			Destroy(activeMenu);
			activeMenuType = MenuType.none;
			UpdateInventoryButton();
			Controller.Instance.suspendInput = false;
			Time.timeScale = 1f;
		}
	}
	public void SetActiveUI(bool active=false){
		List<GameObject> buttons = new List<GameObject>(){inventoryButton, fightButton, punchButton, speakButton};
		foreach (GameObject button in buttons){
			if (button)
				button.SetActive(active);
		}
		topRightBar.SetActive(active);
		CloseActiveMenu();
		ClearWorldButtons();
		ClearActionButtons();
		if (active)
			UpdateButtons();
	}
	public void UpdateButtons(){
		// TODO: this is why the buttons are active but not the health bar
		if (GameManager.Instance.playerObject.GetComponent<Speech>()){
			speakButton.SetActive(true);
		}
		Inventory inv = GameManager.Instance.playerObject.GetComponent<Inventory>();
		if (inv){
			Controller.Instance.focus.UpdateActions(inv);
			UpdateInventoryButton(inv);
		}
		if (Controller.Instance.focus.fightMode){
			ShowPunchButton();
		} else {
			HidePunchButton();
		}
		fightButton.SetActive(true);
	}
	public void UpdateRecordButtons(Commercial commercial){
		if (GameManager.Instance.activeCommercial == null){
			return;
		}
		VideoCamera videoCam = GameObject.FindObjectOfType<VideoCamera>();
		if (commercial.Evaluate(GameManager.Instance.activeCommercial)){
			videoCam.EnableBubble();
		} else {
			videoCam.DisableBubble();
		}
	}
	public void UpdateInventoryButton(){
		UpdateInventoryButton(GameManager.Instance.playerObject.GetComponent<Inventory>());
	}
	public void UpdateInventoryButton(Inventory inventory){
		if (inventory.items.Count > 0){
			inventoryButton.SetActive(true);
		} else {
			inventoryButton.SetActive(false);
		}
		if (activeMenuType == MenuType.inventory){
			CloseActiveMenu();
			ShowInventoryMenu();
		}
	}
	public void ShowFightButton(){
		fightButton.SetActive(true);
	}
	public void HideFightButton(){
		fightButton.SetActive(false);
	}
	public void ShowPunchButton(){
		punchButton.SetActive(true);
	}
	public void HidePunchButton(){
		punchButton.SetActive(false);
	}

	public void PopupCounter(string text, float initValue, float finalValue, Commercial commercial){
		GameObject existingPop = GameObject.Find("Poptext(Clone)");
		if (existingPop == null){
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
	public void PopupCollected(GameObject obj){
		PopupCollected(new AchievementPopup.CollectedInfo(obj));
	}
	public void PopupCollected(AchievementPopup.CollectedInfo info){
		GameObject existingPop = GameObject.Find("AchievementPopup(Clone)");
		if (existingPop == null){
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

	public void PopupAchievement(Achievement achieve){
		GameObject existingPop = GameObject.Find("AchievementPopup(Clone)");
		if (existingPop == null){
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


	public void SetActionText(string text){
		actionTextString = text;
	}
	// public void SetStatus(string text){
	// 	statusText = text;
	// }
	// public void SetTempStatus(string text, float time, TextFX.FXstyle style){
	// 	statusTemp = text;
	// 	statusTempTime = time;
	// 	statusTempStyle = style;
	// }
	// public void SetStatusStyle(TextFX.FXstyle style){
	// 	statusStyle = style;
	// }
	
	public void DisplayHandActions(Inventory inventory){
		ClearWorldButtons();
		activeElements = new List<GameObject>();
		List<actionButton> buttons = new List<actionButton>();
		for (int i = 1; i <= 3; i ++){
			actionButton newbutton = SpawnButton(null);
			newbutton.buttonScript.manualAction = true;
			newbutton.buttonScript.inventory = inventory;
			if (i == 1){
				newbutton.buttonScript.bType = ActionButtonScript.buttonType.Drop;
				newbutton.buttonText.text = "Drop";
				newbutton.buttonScript.buttonText = "Drop";
			} else if (i == 2){
				newbutton.buttonScript.bType = ActionButtonScript.buttonType.Throw;
				newbutton.buttonText.text = "Throw";
				newbutton.buttonScript.buttonText = "Throw";
			} else if (i == 3){
				newbutton.buttonScript.bType = ActionButtonScript.buttonType.Stash;
				newbutton.buttonText.text = "Stash";
				newbutton.buttonScript.buttonText = "Stash";
			}
			activeElements.Add(newbutton.gameobject);
			buttons.Add(newbutton);
		}
		activeElements.Add(CircularizeButtons(buttons, GameManager.Instance.playerObject));
	}

	public void ShowInventoryMenu(){
		Inventory inventory = GameManager.Instance.playerObject.GetComponent<Inventory>();
		GameObject inventoryMenu = ShowMenu(UINew.MenuType.inventory);
		if (inventoryMenu != null){
			Transform itemDrawer = inventoryMenu.transform.Find("menu/itemdrawer");
			foreach (GameObject item in inventory.items){
				GameObject button = Instantiate(Resources.Load("UI/ItemButton")) as GameObject;
				button.transform.SetParent(itemDrawer.transform, false);
				button.GetComponent<ItemButtonScript>().SetButtonAttributes(item, inventory);
			}
		}
	}
	
	public void SetClickedActions(GameObject clickedOn, GameObject clickSite){
		ClearWorldButtons();
		activeElements = new List<GameObject>();
		List<Interaction> clickedActions = Interactor.GetInteractions(GameManager.Instance.playerObject, clickedOn);
		List<actionButton> actionButtons = CreateButtonsFromActions(clickedActions);;
		foreach (actionButton button in actionButtons)
			activeElements.Add(button.gameobject);
		activeElements.Add(CircularizeButtons(actionButtons, clickSite));
	}

	public void ClearWorldButtons(){
		foreach (GameObject element in activeElements)
			Destroy(element);
		activeElements = new List<GameObject>();
	}

	private List<actionButton> CreateButtonsFromActions(List<Interaction> interactions, bool removeColliders=false){
		List<actionButton> returnList = new List<actionButton>();
		foreach (Interaction interaction in interactions){
			actionButton newButton = SpawnButton(interaction);
			if (removeColliders){
				Destroy(newButton.gameobject.GetComponent<CircleCollider2D>());
				Destroy(newButton.gameobject.GetComponent<Rigidbody2D>());
			}
			returnList.Add(newButton);
		}
		return returnList;
	}

	actionButton SpawnButton(Interaction interaction){
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

	private GameObject CircularizeButtons(List<actionButton> buttons, GameObject target){
		float incrementAngle = (Mathf.PI * 2f) / buttons.Count; 
		float angle = 0f;
		RectTransform canvasRect = UICanvas.GetComponent<RectTransform>();
		Camera renderingCamera = UICanvas.GetComponent<Canvas>().worldCamera;

		GameObject buttonAnchor = Instantiate(Resources.Load("UI/ButtonAnchor"), UICanvas.transform.position, Quaternion.identity) as GameObject;
		Rigidbody2D firstBody = null;
		Rigidbody2D priorBody = null;
		int n = 0;
		Vector2 centerPosition = new Vector2(renderingCamera.pixelWidth/2f, renderingCamera.pixelHeight/2f);
		// Debug.Log("center position: "+centerPosition.ToString());
		foreach(actionButton button in buttons){
			Vector2 initLocation = (Vector2)target.transform.position + Toolbox.Instance.RotateZ(Vector2.right/4, angle);
			Vector2 initPosition = renderingCamera.WorldToScreenPoint(initLocation);
			n++;
			button.gameobject.transform.SetParent(UICanvas.transform, false);
			initPosition = (initPosition - centerPosition) / (renderingCamera.pixelWidth / 800f);
			// Debug.Log("initial position screenpoint: "+initPosition.ToString());
			if (initPosition.y > canvasRect.rect.height / 2f){
				initPosition.y = canvasRect.rect.height / 2f - 50f;
			}
			if (initPosition.y < canvasRect.rect.height / -2f){
				initPosition.y = canvasRect.rect.height / -2f + 50f;
			}
			if (initPosition.x > canvasRect.rect.width / 2f){
				initPosition.x = canvasRect.rect.width / 2f - 50f;
			}
			if (initPosition.x < canvasRect.rect.width / -2f){
				initPosition.x = canvasRect.rect.width / -2f + 50f;
			}
			button.gameobject.transform.localPosition = initPosition;
			// Debug.Log("local canvas position: "+button.gameobject.transform.localPosition.ToString());
			if (priorBody){
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
			if (n == buttons.Count && n > 2){
				SpringJoint2D finalSpring = button.gameobject.AddComponent<SpringJoint2D>();
				finalSpring.autoConfigureDistance = false;
				finalSpring.distance = 0.5f;
				finalSpring.dampingRatio = 0.9f;
				finalSpring.connectedBody = firstBody;
			}
			priorBody = button.gameobject.GetComponent<Rigidbody2D>();
			angle += incrementAngle;
		}
		buttonAnchor.transform.position = target.transform.position;
		buttonAnchor.transform.SetParent(target.transform);
		// Debug.Log("anchor screen point: "+renderingCamera.WorldToScreenPoint(target.transform.position).ToString());
		return buttonAnchor;
	}
	private void ArrangeButtonsOnScreenTop(List<actionButton> buttons){
		GameObject bottomBar = UICanvas.transform.Find("topdock").gameObject;
		foreach (actionButton button in buttons){
			ContentSizeFitter buttonSizeFitter = button.gameobject.GetComponent<ContentSizeFitter>();
			if (buttonSizeFitter){
				buttonSizeFitter.enabled = false;
			}
			button.gameobject.transform.SetParent(bottomBar.transform, false);
			button.gameobject.transform.SetSiblingIndex(3);
			RectTransform buttonRect = button.gameobject.GetComponent<RectTransform>();
			buttonRect.sizeDelta = new Vector2(100, 40);
		}
	}
	private void MakeButtonDefault(actionButton button){
		GameObject indicator = Instantiate(Resources.Load("UI/defaultButtonIndicator")) as GameObject;
		indicator.transform.SetParent(button.gameobject.transform, false);
		indicator.transform.SetAsLastSibling();
		// Image image = button.gameobject.GetComponent<Image>();
		// image.sprite = Resources.Load<Sprite>("sprites/UI/BoxTexture5");
	}
	public void CreateActionButtons(List<Interaction> manualActions, Interaction defaultInteraction){
		// Debug.Log("create action buttons");
		ClearActionButtons();
		// Debug.Break();
		List<actionButton> manualButtons = CreateButtonsFromActions(manualActions, true);
		// Debug.Break();
		foreach (actionButton button in manualButtons){
			bottomElements.Add(button.gameobject);
			button.buttonScript.manualAction = true;
			if (button.buttonScript.action == defaultInteraction)
				MakeButtonDefault(button);
		}
		ArrangeButtonsOnScreenTop(manualButtons);
	}

	public void ClearActionButtons(){
		// Debug.Log("clear action buttons");
		foreach (GameObject element in bottomElements)
			Destroy(element);
		bottomElements = new List<GameObject>();
	}
	public void BounceText(string text, GameObject target){
		GameObject bounce = Instantiate(Resources.Load("UI/BounceText")) as GameObject;
		BounceText bounceScript = bounce.GetComponent<BounceText>();
		if (target){
			bounceScript.target = target;
		}
		bounceScript.text = text;
	}


}
