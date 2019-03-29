using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TeleportMenu : MonoBehaviour {
    public Transform buttonList;
    public Button teleportButton;
    public SceneButton selectedButton;
    public Teleporter teleporter;
    public UIButtonEffects effects;
    public List<Button> builtInButtons;
    void Start() {
        effects = GetComponent<UIButtonEffects>();
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
    }
    public void SetReferences() {
        buttonList = transform.Find("main/list");
        teleportButton = transform.Find("main/buttonBar/teleport").GetComponent<Button>();
        teleportButton.interactable = false;
    }
    public void PopulateSceneList() {
        effects.buttons = new List<Button>(builtInButtons);
        SetReferences();
        foreach (string scene in GameManager.Instance.data.unlockedScenes) {
            GameObject buttonObject = Instantiate(Resources.Load("UI/SceneButton")) as GameObject;
            buttonObject.transform.SetParent(buttonList, false);
            SceneButton button = buttonObject.GetComponent<SceneButton>();
            effects.buttons.Add(buttonObject.GetComponent<Button>());
            button.SetValues(this, scene);
        }
        effects.Configure();
    }
    public void SceneButtonCallback(SceneButton button) {
        teleportButton.interactable = true;
        // update image with selected scene info
        selectedButton = button;
    }
    public void TeleportButtonCallback() {
        // teleport to selected scene
        if (selectedButton != null) {
            teleporter.DoTeleport(selectedButton.scene_name);
            UINew.Instance.CloseActiveMenu();
            Controller.Instance.suspendInput = true;
        }
    }
    public void CancelButtonCallback() {
        UINew.Instance.CloseActiveMenu();
    }
}
