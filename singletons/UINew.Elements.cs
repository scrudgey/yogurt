using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Easings;
using UnityEngine.EventSystems;

public partial class UINew : Singleton<UINew> {
    public void Hit() {
        hitIndicator.Hit();
    }
    public void HealthBarOn() {
        if (Controller.Instance.focusHurtable == null)
            return;
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
        if (Controller.Instance.focusHurtable == null)
            return;
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
        if (Controller.Instance.focusHurtable == null)
            return;
        if (Controller.Instance.focusHurtable.health >= Controller.Instance.focusHurtable.maxHealth) {
            if (oxygenBarEasingDirection == EasingDirection.none) {
                if (topRightRectTransform.anchoredPosition.y < 40f) {
                    oxygenBarEasingTimer = 0f;
                    oxygenBarEasingDirection = EasingDirection.up;
                }
            }
        }
    }
    public void ShowPunchButton() {
        // set fight button to stop fight
        fightButtonText.text = "Stop Fighting";
        punchButton.SetActive(true);
    }
    public void HidePunchButton() {
        // set fight button to fight
        fightButtonText.text = "Fight";
        punchButton.SetActive(false);
    }

    public void SetActionText(string text) {
        actionTextString = text;
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
    public void ShowSceneText(string content) {
        // reset alpha visibility here
        sceneNameText.GetComponent<FadeInText>().Reset();
        sceneNameText.enabled = true;
        sceneNameText.gameObject.SetActive(true);
        sceneNameText.text = content;
    }

    public void ClearTopButtons() {
        foreach (GameObject element in activeTopButtons)
            Destroy(element);
        activeTopButtons = new List<GameObject>();
    }
    public void ClearWorldButtons() {
        foreach (GameObject element in activeWorldButtons)
            Destroy(element);
        activeWorldButtons = new List<GameObject>();
    }
    public void ClearObjectives() {
        foreach (Transform child in objectivesContainer) {
            Destroy(child.gameObject);
        }
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
    public ObjectiveIndicator AddObjective(Objective objective) {
        GameObject objectiveObject = GameManager.Instantiate(Resources.Load("UI/objective")) as GameObject;
        objectiveObject.transform.SetParent(objectivesContainer, false);
        ObjectiveIndicator script = objectiveObject.GetComponent<ObjectiveIndicator>();
        script.objective = objective;
        script.description.text = objective.desc;
        return script;
    }
}