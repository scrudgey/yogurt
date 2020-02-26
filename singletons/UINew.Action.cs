using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Easings;
using UnityEngine.EventSystems;

public partial class UINew : Singleton<UINew> {

    public void ShowActionsForWorldClick(GameObject clickedOn, GameObject clickSite) {
        // this method displays actions when the player clicks on something in the game world

        ClearWorldButtons();
        activeWorldButtons = new List<GameObject>();
        List<ActionButton> actionButtons = null;
        if (Controller.Instance.commandTarget != null) {
            actionButtons = CreateButtonsFromActions(Interactor.SelfOnOtherInteractions(Controller.Instance.commandTarget, clickedOn));
        } else {
            actionButtons = CreateButtonsFromActions(Interactor.SelfOnOtherInteractions(GameManager.Instance.playerObject, clickedOn));
        }

        foreach (ActionButton button in actionButtons)
            activeWorldButtons.Add(button.gameobject);
        activeWorldButtons.Add(CircularizeButtons(actionButtons, clickSite));
    }

    public void ShowActionsForPlayerClick(GameObject actor) {
        // this method displays buttons when clicking on the player

        ClearWorldButtons();

        Inventory inventory = actor.GetComponent<Inventory>();
        if (inventory && inventory.holding) {

            activeWorldButtons = new List<GameObject>();
            List<ActionButton> buttons = null;
            if (Controller.Instance.commandTarget != null) {
                buttons = CreateButtonsFromActions(Interactor.SelfOnSelfInteractions(Controller.Instance.commandTarget));
            } else {
                buttons = CreateButtonsFromActions(Interactor.SelfOnSelfInteractions(GameManager.Instance.playerObject));
            }

            foreach (ActionButton button in buttons) {
                activeWorldButtons.Add(button.gameobject);
            }

            ActionButton newbutton = CreateActionButton(inventory, "Drop", ActionButtonScript.buttonType.Drop);
            activeWorldButtons.Add(newbutton.gameobject);
            buttons.Add(newbutton);

            newbutton = CreateActionButton(inventory, "Throw", ActionButtonScript.buttonType.Throw);
            activeWorldButtons.Add(newbutton.gameobject);
            buttons.Add(newbutton);

            if (!inventory.holding.heavyObject) {
                newbutton = CreateActionButton(inventory, "Stash", ActionButtonScript.buttonType.Stash);
                activeWorldButtons.Add(newbutton.gameobject);
                buttons.Add(newbutton);
            }
            activeWorldButtons.Add(CircularizeButtons(buttons, GameManager.Instance.playerObject));
        } else {
            Controller.Instance.ResetLastLeftClicked();
        }
    }

    private ActionButton CreateActionButton(Inventory inventory, string name, ActionButtonScript.buttonType bType) {
        ActionButton newbutton = SpawnButton(null);
        newbutton.buttonScript.manualAction = true;
        newbutton.buttonScript.inventory = inventory;
        newbutton.buttonScript.bType = bType;
        newbutton.buttonText.text = name;
        newbutton.buttonScript.buttonText = name;
        return newbutton;
    }

    static public List<ActionButton> CreateButtonsFromActions(HashSet<InteractionParam> interactions, bool removeColliders = false) {
        List<ActionButton> returnList = new List<ActionButton>();
        foreach (InteractionParam ip in interactions) {
            ActionButton newButton = SpawnButton(ip);
            if (removeColliders) {
                Destroy(newButton.gameobject.GetComponent<CircleCollider2D>());
                Destroy(newButton.gameobject.GetComponent<Rigidbody2D>());
            }
            returnList.Add(newButton);
        }
        return returnList;
    }
    static ActionButton SpawnButton(InteractionParam ip) {
        GameObject newButton = Instantiate(Resources.Load("UI/NeoActionButton"), Vector2.zero, Quaternion.identity) as GameObject;
        ActionButtonScript buttonScript = newButton.GetComponent<ActionButtonScript>();
        buttonScript.button = newButton.GetComponent<Button>();
        Text buttonText = newButton.transform.Find("Text").GetComponent<Text>();
        ActionButton returnbut;
        returnbut.gameobject = newButton;
        returnbut.buttonScript = buttonScript;
        returnbut.buttonText = buttonText;
        if (ip != null) {
            returnbut.buttonScript.action = ip.interaction;
            returnbut.buttonScript.parameters = ip.parameters;
            returnbut.buttonText.text = ip.interaction.actionName;
        }

        return returnbut;
    }
    private GameObject CircularizeButtons(List<ActionButton> buttons, GameObject target) {
        float incrementAngle = (Mathf.PI * 2f) / buttons.Count;
        float angle = 0f;
        RectTransform canvasRect = UICanvas.GetComponent<RectTransform>();
        Camera renderingCamera = UICanvas.GetComponent<Canvas>().worldCamera;
        buttonAnchor = Instantiate(Resources.Load("UI/ButtonAnchor"), UICanvas.transform.position, Quaternion.identity) as GameObject;
        Rigidbody2D firstBody = null;
        Rigidbody2D priorBody = null;
        int n = 0;
        Vector2 centerPosition = new Vector2(renderingCamera.pixelWidth / 2f, renderingCamera.pixelHeight / 2f);
        foreach (ActionButton button in buttons) {
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

    public void UpdateTopActionButtons() {
        // This method updates the buttons on the top action bar
        // top action buttons are everything self-self (including holding).

        if (activeMenuType != MenuType.none)
            return;
        if (GameManager.Instance.playerObject == null || GameManager.Instance.playerIsDead)
            return;

        ClearTopButtons();

        InteractionParam defaultAction = Controller.Instance.focus.UpdateDefaultInteraction();
        List<ActionButton> actionButtons = new List<ActionButton>();
        if (defaultAction != null) {
            actionButtons = CreateActionButtons(new HashSet<InteractionParam> { defaultAction });
        }

        // punch button
        if (Controller.Instance.focus.fightMode) {
            ShowPunchButton();
        } else {
            HidePunchButton();
            if (Controller.Instance.focus.defaultInteraction != null && Controller.Instance.focus.defaultInteraction.IsValid()) {
                foreach (ActionButton button in actionButtons) {
                    if (button.buttonScript.action == Controller.Instance.focus.defaultInteraction.interaction) {
                        GameObject indicator = Instantiate(Resources.Load("UI/defaultButtonIndicator")) as GameObject;
                        indicator.transform.SetParent(button.gameobject.transform, false);
                        indicator.transform.SetAsLastSibling();
                    }
                }
            }
        }

        // inventory button
        Inventory inv = GameManager.Instance.playerObject.GetComponent<Inventory>();
        if (inv) {
            if (inv.items.Count > 0) {
                inventoryButton.SetActive(true);
            } else {
                inventoryButton.SetActive(false);
            }
            if (activeMenuType == MenuType.inventory) {
                CloseActiveMenu();
                ShowInventoryMenu();
            }
        }

        // speech button
        if (GameManager.Instance.data.perks["swear"] && GameManager.Instance.playerObject.GetComponent<Speech>()) {
            speakButton.SetActive(true);
        }

        // fight button
        fightButton.SetActive(true);

        // hypnosis button
        hypnosisButton.SetActive(GameManager.Instance.data.perks["hypnosis"]);

        // vomit button
        if (GameManager.Instance.playerObject.GetComponent<Eater>()) {
            vomitButton.SetActive(GameManager.Instance.data.perks["vomit"]);
        }

        // teleport button
        if (GameManager.Instance.data.teleporterUnlocked && !GameManager.Instance.data.teleportedToday) {
            teleportButton.SetActive(true);
        } else {
            teleportButton.SetActive(false);
        }
    }
    private List<ActionButton> CreateActionButtons(HashSet<InteractionParam> manualActions) {

        List<ActionButton> manualButtons = CreateButtonsFromActions(manualActions, true);
        foreach (ActionButton button in manualButtons) {
            activeTopButtons.Add(button.gameobject);
            button.buttonScript.manualAction = true;
        }

        GameObject bottomBar = UICanvas.transform.Find("topdock").gameObject;
        List<string> newButtonList = new List<string>();
        foreach (ActionButton button in manualButtons) {
            RectTransform buttonRect = button.gameobject.GetComponent<RectTransform>();
            newButtonList.Add(button.buttonText.text);
            ContentSizeFitter buttonSizeFitter = button.gameobject.GetComponent<ContentSizeFitter>();
            if (buttonSizeFitter) {
                buttonSizeFitter.enabled = false;
            }
            button.gameobject.transform.SetParent(bottomBar.transform, false);
            button.gameobject.transform.SetSiblingIndex(3);
            buttonRect.sizeDelta = new Vector2(100, 40);
        }
        previousTopButtons = newButtonList;

        return manualButtons;
    }
}