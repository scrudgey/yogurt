using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections;
using UnityEngine.SceneManagement;

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
        if (data == null)
            data = InitializedGameData();
        //TODO: do not unlock same commercial twice
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
        foreach (string unlock in commercial.unlockUponCompletion) {
            UnlockCommercial(unlock);
        }
        if (commercial.unlockItem != "") {
            ReceivePackage(commercial.unlockItem);
        }
        if (commercial.email != "") {
            ReceiveEmail(commercial.email);
        }
        if (commercial.hallucination != "") {
            data.queuedMagicianSequences.Add(commercial.hallucination);
            Debug.Log(data.queuedMagicianSequences);
        }
        UINew.Instance.ClearObjectives();
        UINew.Instance.RefreshUI(active: false);

        GameObject report = UINew.Instance.ShowMenu(UINew.MenuType.commercialReport);
        NeoCommercialReportMenu menu = report.GetComponent<NeoCommercialReportMenu>();
        menu.commercial = commercial;
        report.GetComponent<NeoCommercialReportMenu>().Report(commercial);
        data.completeCommercials.Add(commercial);
        data.setupSabotage = false;
    }
    public void StartCommercial(Commercial commercial) {
        GameManager.Instance.data.baddiesDefeated = 0;
        GameManager.Instance.data.activeCommercial = commercial;
        SetRecordingStatus(true);
        foreach (VideoCamera vid in GameObject.FindObjectsOfType<VideoCamera>()) {
            vid.UpdateStatus();
        }
        foreach (Objective objective in GameManager.Instance.data.activeCommercial.objectives) {
            UINew.Instance.AddObjective(objective);
        }

        CheckCommercialInitialization(GameManager.Instance.data.activeCommercial, SceneManager.GetActiveScene().name);
    }
    public void SetRecordingStatus(bool value) {
        data.recordingCommercial = value;
        if (GameManager.onRecordingChange != null)
            GameManager.onRecordingChange(value);
        if (!value) {
            data.setupSabotage = false;
        }
    }
    public void CheckCommercialInitialization(Commercial commercial, string sceneName) {
        if (data.commercialsInitializedToday.Contains(commercial.name))
            return;

        if (commercial.name == "1950s Greaser Beatdown" || commercial.name == "Combat II" || commercial.name == "Combat III" || commercial.name == "Combat IV") {
            if (CutsceneScorpion.FindValidDoorway() != null) {
                data.commercialsInitializedToday.Add(commercial.name);
                // CutsceneManager.Instance.InitializeCutscene<CutsceneScorpion>();
                CutsceneManager.Instance.InitializeCutscene(new CutsceneScorpion(commercial.name));
            }
        }

        if (commercial.name == "Nullify Hate" && sceneName == "studio") {
            data.commercialsInitializedToday.Add(commercial.name);
            Transform point1 = GameObject.Find("effigyPoint1").transform;
            GameObject.Instantiate(Resources.Load("prefabs/effigy"), point1.position, Quaternion.identity);
        }

        if (commercial.name == "Eradicate Hate and Ignorance" && sceneName == "studio") {
            data.commercialsInitializedToday.Add(commercial.name);
            Transform point1 = GameObject.Find("effigyPoint1").transform;
            Transform point2 = GameObject.Find("effigyPoint2").transform;
            GameObject.Instantiate(Resources.Load("prefabs/effigy"), point1.position, Quaternion.identity);
            GameObject.Instantiate(Resources.Load("prefabs/effigy_ignorance"), point2.position, Quaternion.identity);
        }

        if (commercial.gremlin) {
            GameObject player = GameManager.Instance.playerObject;
            GameObject zone = GameObject.FindWithTag("zombieSpawnZone");
            Collider2D zombieZonezone = zone.GetComponent<Collider2D>();
            for (int i = 0; i < 5; i++) {
                Vector3 position = Toolbox.RandomPointInBox(zombieZonezone.bounds, player.transform.position);
                GameObject.Instantiate(Resources.Load("prefabs/gremlin"), position, Quaternion.identity);
            }
            Vector3 objectPosition = Toolbox.RandomPointInBox(zombieZonezone.bounds, player.transform.position);
            GameObject.Instantiate(Resources.Load("prefabs/VX_nerve_gas_ventilator"), objectPosition, Quaternion.identity);

            objectPosition = Toolbox.RandomPointInBox(zombieZonezone.bounds, player.transform.position);
            GameObject.Instantiate(Resources.Load("prefabs/baseball_bat"), objectPosition, Quaternion.identity);

            objectPosition = Toolbox.RandomPointInBox(zombieZonezone.bounds, player.transform.position);
            GameObject.Instantiate(Resources.Load("prefabs/golf_club"), objectPosition, Quaternion.identity);

            for (int i = 0; i < 3; i++) {
                objectPosition = Toolbox.RandomPointInBox(zombieZonezone.bounds, player.transform.position);
                GameObject.Instantiate(Resources.Load("prefabs/hand_grenade"), objectPosition, Quaternion.identity);

                objectPosition = Toolbox.RandomPointInBox(zombieZonezone.bounds, player.transform.position);
                GameObject.Instantiate(Resources.Load("prefabs/crossbow"), objectPosition, Quaternion.identity);
            }
        }
    }
}

