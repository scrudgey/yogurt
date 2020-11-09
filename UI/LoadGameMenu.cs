using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System;

public class LoadGameMenu : MonoBehaviour {
    public StartMenu startMenu;
    public UIButtonEffects effects;
    public Button cancelButton;
    public GameObject noSaveGameIndicator;

    public DateTime order(String name, Dictionary<String, GameData> datas) {
        if (name == "test" || name == "Unity") {
            return DateTime.Now;
        } else {
            return datas[name].saveDateTime;
        }
    }
    public void ConfigLoadMenu(StartMenu startMenu) {
        this.startMenu = startMenu;
        effects = GetComponent<UIButtonEffects>();
        effects.buttons = new List<Button> { cancelButton };
        GameObject saveGamePanel = transform.Find("Scroll View/Viewport/Content").gameObject;
        int children = saveGamePanel.transform.childCount;
        for (int i = 0; i < children; ++i) {
            if (saveGamePanel.transform.GetChild(i).gameObject.gameObject == noSaveGameIndicator)
                continue;
            Destroy(saveGamePanel.transform.GetChild(i).gameObject);
        }
        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath);
        List<DirectoryInfo> dirs = info.GetDirectories().ToList();

        Dictionary<String, GameData> datas = new Dictionary<String, GameData>();

        foreach (DirectoryInfo dir in dirs) {
            GameData data = GameManager.Instance.LoadGameData(dir.Name);
            // datas.Add(data);
            datas[dir.Name] = data;
        }

        dirs = dirs.OrderBy(d => order(d.Name, datas)).Reverse().ToList();
        foreach (DirectoryInfo dir in dirs) {
            if (dir.Name == "test" || dir.Name == "Unity")
                continue;
            if (noSaveGameIndicator != null) {
                Destroy(noSaveGameIndicator);
            }
            GameObject newSelector = spawnSaveGameSelector();
            SaveGameSelectorScript script = newSelector.GetComponent<SaveGameSelectorScript>();
            Button scriptButton = newSelector.GetComponent<Button>();
            effects.buttons.Add(scriptButton);
            newSelector.transform.SetParent(saveGamePanel.transform, false);
            script.Configure(startMenu, dir, datas[dir.Name]);
        }
        effects.Configure();
    }

    private GameObject spawnSaveGameSelector() {
        GameObject newobject = Instantiate(Resources.Load("UI/SaveGameSelector")) as GameObject;
        return newobject;
    }
}
