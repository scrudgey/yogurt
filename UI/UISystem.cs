using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UISystem : Singleton<UISystem> {

	public GameObject background;
	private List<GameObject> worldButtons = new List<GameObject>();
	private List<GameObject> handButtons = new List<GameObject>();
	private List<GameObject> inventoryButtons = new List<GameObject>();
	private int oldInventoryItems;
	private Text actionText;
	private Text handText;
	private List<Interaction> _actions = new List<Interaction>();
	public List<Interaction> actions {
		get{
			return _actions;
		}
		set{
			_actions = value;
			UpdateWorldActions();
		}
	}
	private List<Interaction> _manualActions = new List<Interaction>();
	public List<Interaction> manualActions{
		get {
			return _manualActions;
		}
		set {
			_manualActions = value;
			UpdateHandActions();
		}
	}
	private GameObject UIObject;
	private GameObject worldPanel;
	private GameObject handPanel;
	private GameObject inventoryPanel;
	public string MOTD = "smoke weed every day";
	private string _targetName;
	private string _verbText;
	public string targetName{
		get {return _targetName;}
		set {
			string temp = Toolbox.Instance.ScrubText(value);
			_targetName = temp;
			UpdateText();
		}
	}
	public string verbText{
		get {return _verbText;}
		set {
			_verbText = value;
			UpdateText();
		}
	}
	private string _holdingName;
	public string holdingName{
		get {return _holdingName;}
		set {
			string temp = Toolbox.Instance.ScrubText(value);
			_holdingName = temp;
			UpdateText();
		}
	}
	private string _holdingVerb;
	public string holdingVerb{
		get {return _holdingVerb;}
		set {
			_holdingVerb = value;
			UpdateText();
		}
	}
	public Inventory inventory;
	public bool doUpdate;

	void Start(){
		PostLoadInit();
	}

	public void PostLoadInit(){
		UIObject = GameObject.Find("UI");
		actionText = UIObject.transform.Find("UIbackground/worldPanel/worldText").GetComponent<Text>();
		worldPanel = UIObject.transform.Find("UIbackground/worldPanel/worldButtons").gameObject;
		handText  = UIObject.transform.Find("UIbackground/handPanel/handText").GetComponent<Text>();
		handPanel = UIObject.transform.Find("UIbackground/handPanel/handButtons").gameObject;
		inventoryPanel = UIObject.transform.Find("UIbackground/inventoryPanel/Scrollview/inventoryButtons").gameObject;
		background = UIObject.transform.Find("GameObject").gameObject;
		doUpdate = true;
	}

	private struct but{
		public GameObject button;
		public ActionButtonScript buttonScript;
		public Text buttonText;
	}
	
	public enum panel{inventoryPanel,worldPanel,handPanel}
	
	but spawnButton(panel p){
		GameObject newButton = Instantiate( Resources.Load("UI/ActionButton"),Vector2.zero,Quaternion.identity) as GameObject;			
		ActionButtonScript buttonScript = newButton.GetComponent<ActionButtonScript>();
		Text buttonText = newButton.transform.FindChild("Text").GetComponent<Text>();
		if (p == panel.inventoryPanel)
			newButton.transform.SetParent(inventoryPanel.transform,false);
		if (p == panel.handPanel)
			newButton.transform.SetParent(handPanel.transform,false);
		if (p == panel.worldPanel)	
			newButton.transform.SetParent(worldPanel.transform,false);
		but returnbut;
		returnbut.button = newButton;
		returnbut.buttonScript = buttonScript;
		returnbut.buttonText = buttonText;
		
		return returnbut;
	}

	public void UpdateHandActions(){
		foreach (GameObject b in handButtons){
			Destroy(b);
		}
		handButtons = new List<GameObject>();
		if (inventory.holding){
			for (int i = 1; i <= 3; i ++){
				but newbutton = spawnButton(panel.handPanel);
				newbutton.buttonScript.manualAction = true;
				if (i == 1){
					newbutton.buttonScript.bType = ActionButtonScript.buttonType.Drop;
					newbutton.buttonText.text = "Drop";
					newbutton.buttonScript.buttonText = "Drop";
				}
				if (i == 2){
					newbutton.buttonScript.bType = ActionButtonScript.buttonType.Throw;
					newbutton.buttonText.text = "Throw";
					newbutton.buttonScript.buttonText = "Throw";
				}
				if (i == 3){
					newbutton.buttonScript.bType = ActionButtonScript.buttonType.Stash;
					newbutton.buttonText.text = "Stash";
					newbutton.buttonScript.buttonText = "Stash";
				}
				handButtons.Add(newbutton.button);
			}
			foreach (Interaction action in manualActions){
				but newbutton = spawnButton(panel.handPanel);
				newbutton.buttonScript.action = action;
				newbutton.buttonScript.manualAction = true;
				newbutton.buttonText.text = action.actionName;
				handButtons.Add(newbutton.button);
			}
		}
	}

		

	public void UpdateWorldActions(){
		foreach (GameObject b in worldButtons){
			Destroy(b);
		}
		foreach (Interaction action in actions){
			but newbutton = spawnButton(panel.worldPanel);
			newbutton.buttonScript.action = action;
			newbutton.buttonScript.manualAction = false;
			newbutton.buttonText.text = action.actionName;
			worldButtons.Add(newbutton.button);
		}
	}

	public void UpdateInventoryButtons(){
		foreach (GameObject b in inventoryButtons)
			Destroy(b);
		foreach (GameObject item in inventory.items){
			but newbutton = spawnButton(panel.inventoryPanel);
			Item i = item.GetComponent<Item>();
			newbutton.buttonScript.itemName = i.itemName;
			newbutton.buttonScript.bType = ActionButtonScript.buttonType.Inventory;
			newbutton.buttonText.text = Toolbox.Instance.ScrubText( item.name );
			inventoryButtons.Add(newbutton.button);
		}
	}
	
	void Update(){
		if (inventory){
			if (oldInventoryItems != inventory.items.Count)
				UpdateInventoryButtons();
			oldInventoryItems = inventory.items.Count;
		}
	}

	public void InventoryCallback(Inventory inv, List<Interaction> actions){
		if (inv == inventory){
			manualActions = actions;
			if (inv.holding){
				holdingName = inv.holding.name;
			} else {
				holdingName = null;
				holdingVerb = null;
			}
		}
	}

	public void WipeWorldActions(){
		actions = new List<Interaction>();
		UpdateWorldActions();
		verbText = null;
		targetName = null;
	}

	public void WipeHandActions(){
		manualActions = new List<Interaction>();
		UpdateHandActions();
		holdingVerb = null;
		holdingName = null;
	}

	void LateUpdate(){
		if (doUpdate){
			doUpdate = false;
			if (inventory){
				inventory.UpdateActions();
				UpdateInventoryButtons();
			}
		}
	}

	public void MouseOverAction(ActionButtonScript button){
		if ( button.bType != ActionButtonScript.buttonType.Inventory ){

			if (button.manualAction == false){
					verbText = button.action.displayVerb;
			} else{
				if (button.buttonText == ""){
					holdingVerb = button.action.displayVerb;
				} else {
					holdingVerb = button.buttonText;
				}
			}
		} else {
			holdingVerb = "Retrieve "+button.itemName;
		}
	}

	public void MouseExitAction(ActionButtonScript button){
		if (button.manualAction == false){
			verbText = null;
		} else {
			holdingVerb = null;
		}
		if (button.bType == ActionButtonScript.buttonType.Inventory){
			holdingVerb = null;
		}
	}

	// make this all nicer with an enum
	public void InventoryButtonCallback(ActionButtonScript button){
		inventory.RetrieveItem(button.itemName);
	}
	public void ThrowCallback(){
		inventory.ThrowItem();
		WipeHandActions();
	}
	public void DropCallback(){
		inventory.DropItem();
		WipeHandActions();
	}
	public void StashCallback(){
		inventory.StashItem(inventory.holding.gameObject);
		WipeHandActions();
	}

	void UpdateText(){
		string update = "";
		if (targetName != null){
			if (verbText != null){
				update = verbText + " " + targetName;
			} else {
				update = targetName;
			}
		} 
		actionText.text = update;
		update = "";
		if (holdingName != null){
			if (holdingVerb != null){
				update = holdingVerb + " " + holdingName;
			} else {
				update = holdingName;
			}
		} 
		handText.text = update;
	}

}
