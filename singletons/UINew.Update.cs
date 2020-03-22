using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Easings;
using UnityEngine.EventSystems;

public partial class UINew : Singleton<UINew> {

    public void UpdateActionText(bool highlight, GameObject target, bool cursorOverButton) {
        if (GameManager.Instance.playerIsDead || GameManager.Instance.InCutsceneLevel()) {
            SetActionText("");
            return;
        }
        if (!activeMenu && CutsceneManager.Instance.cutscene == null) {
            if (activeWorldButtons.Count > 1) {
                if (cursorOverButton) {
                    SetActionText(actionButtonText);
                } else {
                    SetActionText(lastTarget);
                }
            } else {
                List<Controller.ControlState> selectStates = new List<Controller.ControlState>{
                    Controller.ControlState.swearSelect,
                    Controller.ControlState.insultSelect,
                    Controller.ControlState.hypnosisSelect};
                if (Controller.Instance.state == Controller.ControlState.commandSelect) {
                    string commandName = Toolbox.Instance.GetName(Controller.Instance.commandTarget);
                    if (target != null) {
                        SetActionText("Command " + commandName + " to...");
                    } else {
                        SetActionText("Command " + commandName + "...");
                    }
                } else if (selectStates.Contains(Controller.Instance.state)) {
                    if (target != null) {
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
                        }
                    } else {
                        switch (Controller.Instance.state) {
                            case Controller.ControlState.swearSelect:
                                SetActionText("Swear at ...");
                                break;
                            case Controller.ControlState.insultSelect:
                                SetActionText("Insult ...");
                                break;
                            case Controller.ControlState.hypnosisSelect:
                                SetActionText("Hypnotize ...");
                                break;

                        }
                    }
                } else if (target != null) {
                    lastTarget = Toolbox.Instance.GetName(target);
                    SetActionText(lastTarget);
                } else if (cursorOverButton) {
                    SetActionText(actionButtonText);
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
    public void UpdateObjectives() {
        if (GameManager.Instance.data == null || GameManager.Instance.data.activeCommercial == null || !GameManager.Instance.data.recordingCommercial) {
            ClearObjectives();
            return;
        }
        foreach (Transform child in objectivesContainer) {
            ObjectiveIndicator indicator = child.GetComponent<ObjectiveIndicator>();
            indicator.UpdateCheck();
        }
        foreach (VideoCamera vid in GameObject.FindObjectsOfType<VideoCamera>()) {
            vid.UpdateStatus();
        }
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

    void LateUpdate() {
        // handle pop ups, health bars, and display text

        actionTextObject.text = actionTextString;

        Poptext.LateUpdate();

        if (Controller.Instance.focusHurtable != null && !GameManager.Instance.playerIsDead) {
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
        } else {
            HealthBarOff();
            OxygenBarOff();
        }
        if (healthBarEasingDirection != EasingDirection.none) {
            healthBarEasingTimer += Time.unscaledDeltaTime;
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
            oxygenBarEasingTimer += Time.unscaledDeltaTime;
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

        bool highlight = false;
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider != null && !Controller.forbiddenTags.Contains(hit.collider.tag)) {
                highlight = true;
            }
        }
        GameObject target = null;
        if (highlight) {
            GameObject top = Controller.Instance.GetFrontObject(hits);
            target = Controller.Instance.GetBaseInteractive(top.transform);
        }
        bool cursorOverButton = EventSystem.current.IsPointerOverGameObject();

        UpdateCursor(highlight);
        UpdateActionText(highlight, target, cursorOverButton);
    }
}