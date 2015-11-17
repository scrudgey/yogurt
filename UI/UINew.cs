using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UINew : Singleton<UINew> {
		
	private struct actionButton{
		public GameObject gameobject;
		public ActionButtonScript buttonScript;
		public Text buttonText;
	}
	public string MOTD = "smoke weed every day";
	private GameObject UICanvas;
	private GameObject lastLeftClicked;
	private List<GameObject> activeElements = new List<GameObject>();
	private List<GameObject> bottomElements = new List<GameObject>();

	void Start(){
		UICanvas = GameObject.Find("NeoUICanvas");
	}

	public void Clicked(GameObject clicked){
		lastLeftClicked = clicked;
		SetClickedActions();
	}

	private void SetClickedActions(){
		ClearButtons();
		activeElements = new List<GameObject>();
		List<Interaction> clickedActions = Interactor.GetInteractions(Controller.Instance.focus.gameObject, lastLeftClicked);
		List<actionButton> actionButtons = CreateButtonsFromActions(clickedActions);;
		foreach (actionButton button in actionButtons)
			activeElements.Add(button.gameobject);
		activeElements.Add(CircularizeButtons(actionButtons, lastLeftClicked));
	}

	public void ClearButtons(){
		foreach (GameObject element in activeElements)
			Destroy(element);
	}

	private List<actionButton> CreateButtonsFromActions(List<Interaction> interactions){
		List<actionButton> returnList = new List<actionButton>();
		foreach (Interaction interaction in interactions)
			returnList.Add(spawnButton(interaction));
		return returnList;
	}

	actionButton spawnButton(Interaction interaction){
		GameObject newButton = Instantiate( Resources.Load("UI/NeoActionButton"), Vector2.zero, Quaternion.identity) as GameObject;			
		ActionButtonScript buttonScript = newButton.GetComponent<ActionButtonScript>();
		Text buttonText = newButton.transform.FindChild("Text").GetComponent<Text>();
		actionButton returnbut;
		returnbut.gameobject = newButton;
		returnbut.buttonScript = buttonScript;
		returnbut.buttonText = buttonText;
		returnbut.buttonScript.action = interaction;
		returnbut.buttonText.text = interaction.actionName;
		return returnbut;
	}

	private GameObject CircularizeButtons(List<actionButton> buttons, GameObject target){
		
		float incrementAngle = (Mathf.PI * 2.5f) / buttons.Count; 
		float angle = 0f;

		GameObject buttonAnchor = Instantiate(Resources.Load("UI/ButtonAnchor"), UICanvas.transform.position, Quaternion.identity) as GameObject;
		Rigidbody2D firstBody = null;
		Rigidbody2D priorBody = null;
		int n = 0;
		foreach(actionButton button in buttons){
			Vector2 initLocation = (Vector2)buttonAnchor.transform.position + Toolbox.Instance.RotateZ(Vector2.right, angle);
			n++;
			// instantiate button
			button.gameobject.transform.position = initLocation;
			button.gameobject.transform.SetParent(UICanvas.transform, false);
			if (priorBody){
				SpringJoint2D spring = button.gameobject.AddComponent<SpringJoint2D>();
				spring.dampingRatio = 0.9f;
				spring.distance = 0.5f;
				spring.connectedBody = priorBody;
			}
			if (!firstBody)
				firstBody = button.gameobject.GetComponent<Rigidbody2D>();

			// set up spring connection to anchor
			SpringJoint2D anchorSpring = buttonAnchor.AddComponent<SpringJoint2D>();
			anchorSpring.distance = 0.25f;
			anchorSpring.dampingRatio = 0.9f;
			anchorSpring.frequency = 15f;
			anchorSpring.connectedBody = button.gameobject.GetComponent<Rigidbody2D>();

			// connect buttons in circular sequence
			if (n == buttons.Count && n > 2){
				SpringJoint2D finalSpring = button.gameobject.AddComponent<SpringJoint2D>();
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
		foreach (actionButton button in buttons){
			RectTransform rect = UICanvas.GetComponent<RectTransform>();
			Canvas canvas = UICanvas.GetComponent<Canvas>();
			Vector3 newpos = new Vector3(0f, 0f, 0f);

			button.gameobject.transform.SetParent(UICanvas.transform, false);
			RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, new Vector2(0.1f, 0.1f), canvas.worldCamera, out newpos);
			button.gameobject.transform.localPosition = newpos;
		}
	}

	public void InventoryCallback(Inventory inventory){
		if (inventory.gameObject != GameManager.Instance.playerObject)
			return;
		if (!inventory.holding)
			return;

		bottomElements = new List<GameObject>();

		List<Interaction> manualActions = Interactor.GetInteractions(inventory.holding.gameObject, Controller.Instance.focus.gameObject);
		foreach (Interaction inter in Interactor.ReportRightClickActions(Controller.Instance.focus.gameObject, inventory.holding.gameObject))
			if (!manualActions.Contains(inter))   // inverse double-count diode
				manualActions.Add(inter);
		Interaction defaultInteraction = Interactor.GetDefaultAction(manualActions);

		List<actionButton> manualButtons = CreateButtonsFromActions(manualActions);
		foreach (actionButton button in manualButtons){
			bottomElements.Add(button.gameobject);
			button.buttonScript.manualAction = true;
		}
		ArrangeButtonsOnScreenBottom(manualButtons);
	}

}
