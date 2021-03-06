﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class TeleportMenu : MonoBehaviour {
    public Transform buttonList;
    public Button teleportButton;
    public SceneButton selectedButton;
    public Teleporter teleporter;
    public UIButtonEffects effects;
    public List<Button> builtInButtons;
    public Text descriptionText;
    // public Image image;
    void Start() {
        effects = GetComponent<UIButtonEffects>();
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
    }
    public void SetReferences() {
        buttonList = transform.Find("main/Scroll View/Viewport/Content");
        descriptionText = transform.Find("main/desc").GetComponent<Text>();
        descriptionText.gameObject.SetActive(false);
        foreach (Transform child in buttonList) {
            Destroy(child.gameObject);
        }
        teleportButton = transform.Find("main/buttonBar/teleport").GetComponent<Button>();
        teleportButton.interactable = false;
    }
    public void PopulateSceneList() {
        effects.buttons = new List<Button>(builtInButtons);
        SetReferences();
        List<string> sceneList = GameManager.Instance.data.unlockedScenes.ToList<string>();
        sceneList.OrderBy(scene => GameManager.sceneNames[scene]);
        foreach (string scene in sceneList.OrderBy(scene => GameManager.sceneNames[scene])) {
            GameObject buttonObject = Instantiate(Resources.Load("UI/SceneButton")) as GameObject;
            buttonObject.transform.SetParent(buttonList, false);
            SceneButton button = buttonObject.GetComponent<SceneButton>();
            effects.buttons.Add(buttonObject.GetComponent<Button>());
            button.SetValues(this, scene);
        }
        effects.Configure();
    }
    public void SceneButtonCallback(SceneButton button) {
        descriptionText.gameObject.SetActive(true);
        teleportButton.interactable = true;
        // update image with selected scene info
        descriptionText.text = "Teleport to: " + GameManager.sceneNames[button.scene_name];
        selectedButton = button;
    }
    public void TeleportButtonCallback() {
        // teleport to selected scene
        if (selectedButton != null) {
            GameManager.Instance.data.teleportedToday = true;
            UINew.Instance.CloseActiveMenu();
            InputController.Instance.suspendInput = true;
            if (teleporter != null) {
                teleporter.DoTeleport(selectedButton.scene_name);
            } else {
                GameObject teleporterFX = Instantiate(Resources.Load("prefabs/TeleportFX")) as GameObject;
                teleporter = teleporterFX.GetComponent<Teleporter>();
                teleporterFX.transform.SetParent(GameManager.Instance.playerObject.transform, false);
                teleporterFX.transform.localPosition = Vector3.zero;
                teleporter.DoTeleport(selectedButton.scene_name);
            }
        }
    }
    public void CancelButtonCallback() {
        UINew.Instance.CloseActiveMenu();
    }
}
