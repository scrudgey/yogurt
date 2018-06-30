﻿using UnityEngine;
using System.IO;
public class UIButtonCallbacks : MonoBehaviour {
    public void FightButtonClick() {
        Controller.Instance.focus.ToggleFightMode();
        // UINew.Instance.RefreshUI(active:true);
        UINew.Instance.UpdateButtons();
    }
    public void SpeakButtonClick() {
        UINew.Instance.ShowMenu(UINew.MenuType.speech);
    }
    public void InventoryButtonClick() {
        UINew.Instance.ShowInventoryMenu();
    }
    public void FinishButtonClick() {
        VideoCamera video = GameObject.FindObjectOfType<VideoCamera>();
        video.live = false;
        if (video) {
            GameManager.Instance.EvaluateCommercial(video.commercial);
        }
    }
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
        if (Controller.Instance.state != Controller.ControlState.hypnosisSelect) {
            Controller.Instance.state = Controller.ControlState.hypnosisSelect;
        } else {
            Controller.Instance.state = Controller.ControlState.normal;
        }
    }
    public void VomitButtonClick() {
        Eater eater = GameManager.Instance.playerObject.GetComponent<Eater>();
        eater.Vomit();
    }
}
