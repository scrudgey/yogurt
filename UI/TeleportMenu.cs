using UnityEngine;
using UnityEngine.UI;

public class TeleportMenu : MonoBehaviour {
    public Transform buttonList;
    public Button teleportButton;
    public SceneButton selectedButton;
    public Teleporter teleporter;
    void Start() {
        GetComponent<Canvas>().worldCamera = GameManager.Instance.cam;
    }
    public void SetReferences() {
        buttonList = transform.Find("main/list");
        teleportButton = transform.Find("main/buttonBar/teleport").GetComponent<Button>();
        teleportButton.interactable = false;
    }
    public void PopulateSceneList() {
        SetReferences();
        foreach (string scene in GameManager.Instance.data.unlockedScenes) {
            GameObject buttonObject = Instantiate(Resources.Load("UI/SceneButton")) as GameObject;
            buttonObject.transform.SetParent(buttonList, false);
            SceneButton button = buttonObject.GetComponent<SceneButton>();
            button.SetValues(this, scene);
        }
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
