using UnityEngine;
using System.IO;
using UnityEngine.UI;
public class UIButtonCallbacks : MonoBehaviour {
    public void FightButtonClick() {
        if (Controller.Instance.state == Controller.ControlState.inMenu)
            return;
        Controller.Instance.focus.ToggleFightMode();
        UINew.Instance.UpdateButtons();
    }
    public void SpeakButtonClick() {
        UINew.Instance.ShowMenu(UINew.MenuType.speech);
    }
    public void InventoryButtonClick() {
        UINew.Instance.ShowInventoryMenu();
    }
    // public void FinishButtonClick() {
    //     VideoCamera video = GameObject.FindObjectOfType<VideoCamera>();
    //     video.live = false;
    //     if (video) {
    //         GameManager.Instance.EvaluateCommercial(video.commercial);
    //     }
    // }
    public void SaveButtonClick() {
        MySaver.Save();
        MySaver.SaveObjectDatabase();
    }
    public void LoadButtonClick() {
        GameManager.Instance.SetFocus(MySaver.LoadScene());
    }
    public void TestButtonClick() {
        string path = Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName);
        DirectoryInfo dataDir = new DirectoryInfo(path);
        dataDir.Delete(true);
        // Debug.Log("test");
        // Grammar g = new Grammar();
        // g.Load("insult");
        // Debug.Log(g.Parse("{main}"));
    }
    public void HypnosisButtonClick() {
        if (Controller.Instance.state == Controller.ControlState.inMenu)
            return;
        if (Controller.Instance.state != Controller.ControlState.hypnosisSelect) {
            Controller.Instance.state = Controller.ControlState.hypnosisSelect;
        } else {
            Controller.Instance.state = Controller.ControlState.normal;
        }
    }
    public void VomitButtonClick() {
        if (Controller.Instance.state == Controller.ControlState.inMenu)
            return;
        Eater eater = GameManager.Instance.playerObject.GetComponent<Eater>();
        eater.Vomit();
    }
    public void TeleportButtonClick() {
        GameObject menuObject = UINew.Instance.ShowMenu(UINew.MenuType.teleport);
        TeleportMenu menu = menuObject.GetComponent<TeleportMenu>();
        menu.PopulateSceneList();
    }

    public void MusicToggleChanged(bool selected) {
        GameManager.settings.musicOn = selected;
    }
}
