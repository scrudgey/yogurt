using UnityEngine;
// using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UINew : Singleton<UINew> {
		
	private struct actionButton{
		public GameObject gameobject;
		public ActionButtonScript buttonScript;
		public Text buttonText;
	}
	public string MOTD = "smoke weed every day";
	private Inventory inventory;
	private GameObject UICanvas;
	private GameObject lastLeftClicked;
	private List<GameObject> activeElements = new List<GameObject>();
	private List<GameObject> bottomElements = new List<GameObject>();
	private Interaction defaultInteraction;
	private GameObject inventoryButton;
	private GameObject inventoryMenu;
	private bool init = false;
	public bool inventoryVisible = false;
	
	private string statusText;
	private string statusTemp;
	private float statusTempTime;
	private TextFX statusFX;
	private TextFX.FXstyle statusTempStyle;
	public TextFX.FXstyle statusStyle;
	
	public Text status;

	void Start(){
		if (!init)
			PostLoadInit();
	}
	public void PostLoadInit() {
		init = true;
		UICanvas = GameObject.Find("NeoUICanvas");
		inventoryButton = UICanvas.transform.Find("bottomdock/InvButtonDock/InventoryButton").gameObject;
		inventoryMenu = GameObject.Find("InventoryScreen");
		status = UICanvas.transform.Find("topdock/topBar/status").GetComponent<Text>();
		statusFX = status.gameObject.GetComponent<TextFX>();
		status.gameObject.SetActive(false);
		inventoryMenu.SetActive(false);
		inventoryButton.SetActive(false);
		if (GameManager.Instance.playerObject)
			inventory = GameManager.Instance.playerObject.GetComponent<Inventory>();
		if (inventory){
			HandleInventoryButton();
			InventoryCallback(inventory);
		}
		CloseClosetMenu();
	}
	
	public void SetStatus(string text){
		statusText = text;
	}
	public void SetTempStatus(string text, float time, TextFX.FXstyle style){
		statusTemp = text;
		statusTempTime = time;
		statusTempStyle = style;
	}
	public void SetStatusStyle(TextFX.FXstyle style){
		// statusFX.style = style;
		statusStyle = style;
	}
	void Update(){
		if (statusTempTime > 0){
			statusTempTime -= Time.deltaTime;
		}
		if (statusTempTime > 0){
			status.text = statusTemp;
			if (statusFX.style != statusTempStyle){
				statusFX.style = statusTempStyle;
			}
		} else {
			status.text = statusText;
			if (statusFX.style != statusStyle){
				statusFX.style = statusStyle;
			}
		}
	}

	public void Clicked(GameObject clicked){
		if (lastLeftClicked == clicked && activeElements.Count > 0){
			ClearWorldButtons();
			lastLeftClicked = null;
		} else {
			lastLeftClicked = clicked;
			if (clicked.transform.IsChildOf(GameManager.Instance.playerObject.transform) || clicked == GameManager.Instance.playerObject){
				if (inventory)
					if (inventory.holding)
						DisplayHandActions();
			} else {
				SetClickedActions();
			}
		}
	}

	private void DisplayHandActions(){
		ClearWorldButtons();
		activeElements = new List<GameObject>();
		List<actionButton> buttons = new List<actionButton>();
		for (int i = 1; i <= 3; i ++){
			actionButton newbutton = spawnButton(null);
			newbutton.buttonScript.manualAction = true;
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

	public void HandActionCallback(ActionButtonScript.buttonType bType){
		switch (bType){
		case ActionButtonScript.buttonType.Drop:
			inventory.DropItem();
			ClearWorldButtons();
			break;
		case ActionButtonScript.buttonType.Throw:
			inventory.ThrowItem();
			ClearWorldButtons();
			break;
		case ActionButtonScript.buttonType.Stash:
			inventory.StashItem(inventory.holding.gameObject);
			ClearWorldButtons();
			HandleInventoryButton();
			if (inventoryVisible){
				CloseInventoryMenu();
				ShowInventoryMenu();
			}
			break;
		default:
			break;
		}
	}

	private void HandleInventoryButton(){
		if (inventory.items.Count > 0){
			inventoryButton.SetActive(true);
		} else {
			inventoryButton.SetActive(false);
		}
	}

	public void CloseInventoryMenu(){
		inventoryVisible = false;
		Transform itemDrawer = inventoryMenu.transform.Find("menu/itemdrawer");
		int children = itemDrawer.childCount;
		for (int i = 0; i < children; ++i)
			Destroy(itemDrawer.GetChild(i).gameObject);
		inventoryMenu.SetActive(false);
		if (inventory.items.Count == 0)
			inventoryButton.SetActive(false);
	}

	public void ShowInventoryMenu(){
		inventoryVisible = true;
		inventoryMenu.SetActive(true);
		Transform itemDrawer = inventoryMenu.transform.Find("menu/itemdrawer");
		foreach (GameObject item in inventory.items){
			GameObject button = Instantiate(Resources.Load("UI/ItemButton")) as GameObject;
			button.transform.SetParent(itemDrawer.transform, false);
			button.GetComponent<ItemButtonScript>().SetButtonAttributes(item);
		}
	}
	
	public void CloseClosetMenu(){
		GameObject closetMenu = GameObject.Find("ClosetMenu");
		Destroy(closetMenu);
	}
	public void ShowClosetMenu(){
		GameObject closetMenu = Instantiate(Resources.Load("UI/ClosetMenu")) as GameObject;
		closetMenu.name = Toolbox.Instance.CloneRemover(closetMenu.name);
		closetMenu.GetComponent<Canvas>().worldCamera = UICanvas.GetComponent<Canvas>().worldCamera;
	}

	public void ItemButtonCallback(ItemButtonScript button){
		inventory.RetrieveItem(button.itemName);
		CloseInventoryMenu();
	}

	private void SetClickedActions(){
		ClearWorldButtons();
		activeElements = new List<GameObject>();
		List<Interaction> clickedActions = Interactor.GetInteractions(GameManager.Instance.playerObject, lastLeftClicked);
		List<actionButton> actionButtons = CreateButtonsFromActions(clickedActions);;
		foreach (actionButton button in actionButtons)
			activeElements.Add(button.gameobject);
		activeElements.Add(CircularizeButtons(actionButtons, lastLeftClicked));
	}

	public void ClearWorldButtons(){
		foreach (GameObject element in activeElements)
			Destroy(element);
		activeElements = new List<GameObject>();
	}

	public void ClearBottomButtons(){
		foreach (GameObject element in bottomElements)
			Destroy(element);
	}

	private List<actionButton> CreateButtonsFromActions(List<Interaction> interactions, bool removeColliders=false){
		List<actionButton> returnList = new List<actionButton>();
		foreach (Interaction interaction in interactions){
			actionButton newButton = spawnButton(interaction);
			if (removeColliders){
				Destroy(newButton.gameobject.GetComponent<CircleCollider2D>());
				Destroy(newButton.gameobject.GetComponent<Rigidbody2D>());
			}
			returnList.Add(newButton);
		}
		return returnList;
	}

	actionButton spawnButton(Interaction interaction){
		GameObject newButton = Instantiate(Resources.Load("UI/NeoActionButton"), Vector2.zero, Quaternion.identity) as GameObject;
		ActionButtonScript buttonScript = newButton.GetComponent<ActionButtonScript>();
		Text buttonText = newButton.transform.FindChild("Text").GetComponent<Text>();
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
		Camera renderingCamera = UICanvas.GetComponent<Canvas>().worldCamera;

		GameObject buttonAnchor = Instantiate(Resources.Load("UI/ButtonAnchor"), UICanvas.transform.position, Quaternion.identity) as GameObject;
		Rigidbody2D firstBody = null;
		Rigidbody2D priorBody = null;
		int n = 0;
		Vector2 centerPosition = renderingCamera.WorldToScreenPoint(UICanvas.transform.position);
		
		foreach(actionButton button in buttons){
			Vector2 initLocation = (Vector2)target.transform.position + Toolbox.Instance.RotateZ(Vector2.right, angle);
			Vector2 initPosition = UICanvas.GetComponent<Canvas>().worldCamera.WorldToScreenPoint(initLocation);
			n++;
			// instantiate button
			button.gameobject.transform.SetParent(UICanvas.transform, false);
			button.gameobject.transform.localPosition = initPosition - centerPosition;
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
		return buttonAnchor;
	}

	private void ArrangeButtonsOnScreenBottom(List<actionButton> buttons){
		GameObject bottomBar = UICanvas.transform.Find("bottomdock/bottom").gameObject;
		foreach (actionButton button in buttons){
			button.gameobject.transform.SetParent(bottomBar.transform, false);
		}
	}

	private void MakeButtonDefault(actionButton button){
		Image image = button.gameobject.GetComponent<Image>();
		image.sprite = Resources.Load<Sprite>("UI/BoxTexture5");
	}

	public void InventoryButtonsCheck(){
		if (inventory)
			InventoryCallback(inventory);
	}

	public void InventoryCallback(Inventory inv){
		if (inv.gameObject != GameManager.Instance.playerObject)
			return;
		inventory = inv;
		ClearBottomButtons();
		bottomElements = new List<GameObject>();
		defaultInteraction = null;

		if (!inv.holding)
			return;

		List<Interaction> manualActions = Interactor.ReportManualActions(inv.holding.gameObject, GameManager.Instance.playerObject);
		foreach (Interaction inter in Interactor.ReportRightClickActions(GameManager.Instance.playerObject, inv.holding.gameObject))
			if (!manualActions.Contains(inter))   // inverse double-count diode
				manualActions.Add(inter);
		foreach (Interaction inter in Interactor.ReportFreeActions(inv.holding.gameObject))
			if (!manualActions.Contains(inter))
				manualActions.Add(inter);
		defaultInteraction = Interactor.GetDefaultAction(manualActions);

		List<actionButton> manualButtons = CreateButtonsFromActions(manualActions, true);
		foreach (actionButton button in manualButtons){
			bottomElements.Add(button.gameobject);
			button.buttonScript.manualAction = true;
			if (button.buttonScript.action == defaultInteraction)
				MakeButtonDefault(button);
		}
		ArrangeButtonsOnScreenBottom(manualButtons);
	}

	public void ShootPressed(){
		if (defaultInteraction != null)
			defaultInteraction.DoAction();
	}

	public void ShootHeld(){
		if (defaultInteraction != null){
			if (defaultInteraction.continuous)	
				defaultInteraction.DoAction();
		}

	}

	public void PauseMenu(){
		GameObject temp = GameObject.Find("PauseMenu");
		if (temp){
			Destroy(temp);
		} else {
			GameObject menu = Instantiate(Resources.Load("UI/PauseMenu")) as GameObject;
			menu.gameObject.name = Toolbox.Instance.ScrubText(menu.gameObject.name);
			Canvas menuCanvas = menu.GetComponent<Canvas>();
			menuCanvas.worldCamera = UICanvas.GetComponent<Canvas>().worldCamera;
		}
	}
    
    

}
