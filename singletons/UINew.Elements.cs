using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Easings;
using UnityEngine.EventSystems;
using System;
using UnityEngine.InputSystem;

public partial class UINew : Singleton<UINew> {
    public void Hit() {
        hitIndicator.Hit();
    }
    public void HealthBarOn() {
        if (InputController.Instance.focusHurtable == null)
            return;
        if (InputController.Instance.focusHurtable.oxygen >= InputController.Instance.focusHurtable.maxOxygen) {
            if (healthBarEasingDirection == EasingDirection.none) {
                if (topRightRectTransform.anchoredPosition.y > 10f) {
                    healthBarEasingTimer = 0f;
                    healthBarEasingDirection = EasingDirection.down;
                }
            }
        }
    }
    public void HealthBarOff() {
        if (InputController.Instance.focusHurtable == null)
            return;
        if (InputController.Instance.focusHurtable.oxygen >= InputController.Instance.focusHurtable.maxOxygen) {
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
        if (InputController.Instance.focusHurtable == null)
            return;
        if (InputController.Instance.focusHurtable.health >= InputController.Instance.focusHurtable.maxHealth) {
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
        Vector3 mousePos = Mouse.current.position.ReadValue();
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

    /*
    *   Objectives
    */
    public void ClearObjectives() {
        foreach (Transform child in objectivesContainer) {
            Destroy(child.gameObject);
        }
    }
    public ObjectiveIndicator AddObjective(Objective objective) {
        GameObject objectiveObject = GameManager.Instantiate(Resources.Load("UI/objective")) as GameObject;
        objectiveObject.transform.SetParent(objectivesContainer, false);
        ObjectiveIndicator script = objectiveObject.GetComponent<ObjectiveIndicator>();
        script.objective = objective;
        script.description.text = objective.desc;
        return script;
    }

    /*
    *   Status icons
    */
    public void UpdateStatusIcons(Intrinsics intrinsics) {
        HashSet<BuffType> existingBuffs = ClearStatusIcons();
        foreach (KeyValuePair<BuffType, Buff> buff in intrinsics.NetBuffs()) {
            if (buff.Value.active()) {

                GameObject icon = UINew.Instance.AddStatusIcon(buff.Value);
                ParticleSystem particles = icon.GetComponentInChildren<ParticleSystem>();
                if (!existingBuffs.Contains(buff.Value.type)) {
                    particles.Play();
                } else {
                    particles.Stop();
                    Outline outline = icon.GetComponent<Outline>();
                    OutlineFader fader = icon.GetComponent<OutlineFader>();
                    Destroy(outline);
                    Destroy(fader);
                }
            }
        }
    }
    public HashSet<BuffType> ClearStatusIcons() {
        HashSet<BuffType> existingBuffs = new HashSet<BuffType>();
        foreach (Transform child in iconDock) {
            UIStatusIcon statusIcon = child.GetComponent<UIStatusIcon>();
            existingBuffs.Add(statusIcon.buff.type);
            Destroy(child.gameObject);
        }
        return existingBuffs;
    }
    public GameObject AddStatusIcon(Buff buff) {
        GameObject icon = Instantiate(Resources.Load("UI/StatusIcon")) as GameObject;
        UIStatusIcon statusIcon = icon.GetComponent<UIStatusIcon>();
        statusIcon.Initialize(buff.type, buff);
        statusIcon.transform.SetParent(iconDock, false);
        return icon;
    }
    public void FadeOut(Action callback, float fadeOutTime = 0.5f) {
        Debug.Log("fade out");
        fader.FadeOut(callback, Color.black, fadeTime: fadeOutTime);
    }
    public void FadeIn(Action callback) {
        Debug.Log("fade in");
        fader.FadeIn(callback, Color.black);
    }
    public void WhiteIn(Action callback) {
        Debug.Log("white in");
        fader.FadeIn(callback, Color.white);
    }
    public void Blackout() {
        Debug.Log("blackout");
        fader.Black();
    }
    public void WhiteOut() {
        Debug.Log("whiteout");
        fader.White();
    }
    public void ClearFaderModules() {
        Debug.Log("clearfadermodules");
        fader.ClearAllModules();
    }
    public void Clear() {
        Debug.Log("clear");
        fader.Clear();
    }
    public void FadeInCredits(string title, string contents, bool left = true) {
        if (left) {
            creditsLeftContent.text = contents;
            creditsLeftTitle.text = title;

            creditsLeftContent.GetComponent<FadeInOut>().FadeOut(() => { }, Color.white);
            creditsLeftTitle.GetComponent<FadeInOut>().FadeOut(() => { }, Color.white);
        } else {
            creditsRightContent.text = contents;
            creditsRightTitle.text = title;

            creditsRightContent.GetComponent<FadeInOut>().FadeOut(() => { }, Color.white);
            creditsRightTitle.GetComponent<FadeInOut>().FadeOut(() => { }, Color.white);
        }
    }
    public void FadeOutCredits(bool left = true) {
        if (left) {
            // creditsLeftContent.text = "";
            // creditsLeftTitle.text = "";

            creditsLeftContent.GetComponent<FadeInOut>().FadeIn(() => { }, Color.white);
            creditsLeftTitle.GetComponent<FadeInOut>().FadeIn(() => { }, Color.white);
        } else {
            // creditsRightContent.text = "";
            // creditsRightTitle.text = "";

            creditsRightContent.GetComponent<FadeInOut>().FadeIn(() => { }, Color.white);
            creditsRightTitle.GetComponent<FadeInOut>().FadeIn(() => { }, Color.white);
        }
    }
    public void FadeInMoreCredits(string left, string right) {
        creditsLeftMoreContent.text = left;
        creditsRightMoreContent.text = right;
    }
    public void FadeOutMoreCredits() {
        // creditsLeftMoreContent.text = "";
        // creditsRightMoreContent.text = "";
        creditsLeftMoreContent.color = Color.clear;
        creditsRightMoreContent.color = Color.clear;
    }
}