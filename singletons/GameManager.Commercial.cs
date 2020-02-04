using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections;

public partial class GameManager : Singleton<GameManager> {

    public List<Commercial> listAllCommercials() {
        List<Commercial> passList = new List<Commercial>();
        UnityEngine.Object[] XMLObjects = Resources.LoadAll("data/commercials");
        List<TextAsset> xmlList = new List<TextAsset>();
        for (int i = 0; i < XMLObjects.Length; i++) {
            xmlList.Add((TextAsset)XMLObjects[i]);
        }
        var serializer = new XmlSerializer(typeof(Commercial));
        foreach (TextAsset asset in xmlList) {
            using (var reader = new System.IO.StringReader(asset.text)) {
                Commercial newCommercial = serializer.Deserialize(reader) as Commercial;
                passList.Add(newCommercial);
            }
        }
        return passList;
    }

    public void UnlockCommercial(string filename) {
        //TODO: do not unlock same commercial twice
        // Debug.Log("unlocking "+filename);
        Commercial unlocked = Commercial.LoadCommercialByFilename(filename);
        foreach (Commercial commercial in data.unlockedCommercials) {
            if (commercial.name == unlocked.name) {
                // Debug.Log("already unlocked. skipping...");
                return;
            }
        }
        data.unlockedCommercials.Add(unlocked);
        data.newUnlockedCommercials.Add(unlocked);
    }
    public void EvaluateCommercial() {
        bool success = false;
        if (GameManager.Instance.data.activeCommercial != null) {
            success = GameManager.Instance.data.activeCommercial.Evaluate();
        }
        if (success) {
            //process reward
            CommercialCompleted();
        } else {
            // do something to display why the commercial is not done yet
            Debug.Log("commercial did not pass.");
            Debug.Log("loaded commercial:");
            Debug.Log(GameManager.Instance.data.activeCommercial);
        }
    }
    public void CommercialCompleted() {
        Commercial commercial = GameManager.Instance.data.activeCommercial;
        data.completeCommercials.Add(commercial);
        foreach (string unlock in commercial.unlockUponCompletion) {
            UnlockCommercial(unlock);
        }
        GameObject report = UINew.Instance.ShowMenu(UINew.MenuType.commercialReport);
        CommercialReportMenu menu = report.GetComponent<CommercialReportMenu>();
        menu.commercial = commercial;
        report.GetComponent<CommercialReportMenu>().Report(commercial);
        if (commercial.unlockItem != "") {
            ReceivePackage(commercial.unlockItem);
        }
        if (commercial.email != "") {
            ReceiveEmail(commercial.email);
        }
        UINew.Instance.ClearObjectives();
        UINew.Instance.RefreshUI(active: false);
    }
    public void StartCommercial(Commercial commercial) {
        GameManager.Instance.data.activeCommercial = commercial;
        // GameManager.Instance.data.recordingCommercial = true;
        SetRecordingStatus(true);
        foreach (VideoCamera vid in GameObject.FindObjectsOfType<VideoCamera>()) {
            vid.UpdateStatus();
        }
        if (commercial.name == "1950s Greaser Beatdown") {
            CutsceneManager.Instance.InitializeCutscene<CutsceneScorpion>();
        }
        foreach (Objective objective in GameManager.Instance.data.activeCommercial.objectives) {
            UINew.Instance.AddObjective(objective);
        }
    }
    public void SetRecordingStatus(bool value) {
        data.recordingCommercial = value;
        if (GameManager.onRecordingChange != null)
            GameManager.onRecordingChange(value);
    }
}

