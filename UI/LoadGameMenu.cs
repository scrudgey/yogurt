﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class LoadGameMenu : MonoBehaviour {
    public StartMenu startMenu;
    public UIButtonEffects effects;
    public void ConfigLoadMenu(StartMenu startMenu) {
        this.startMenu = startMenu;
        effects = GetComponent<UIButtonEffects>();
        GameObject saveGamePanel = transform.Find("Scroll View/Viewport/Content").gameObject;
        int children = saveGamePanel.transform.childCount;
        for (int i = 0; i < children; ++i)
            Destroy(saveGamePanel.transform.GetChild(i).gameObject);
        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath);
        DirectoryInfo[] dirs = info.GetDirectories();
        foreach (DirectoryInfo dir in dirs) {
            if (dir.Name == "test" || dir.Name == "Unity")
                continue;
            GameData data = GameManager.Instance.LoadGameData(dir.Name);
            GameObject newSelector = spawnSaveGameSelector();
            SaveGameSelectorScript script = newSelector.GetComponent<SaveGameSelectorScript>();
            Button scriptButton = newSelector.GetComponent<Button>();
            effects.buttons.Add(scriptButton);
            newSelector.transform.SetParent(saveGamePanel.transform, false);
            script.Configure(startMenu, dir, data);
        }
    }

    private GameObject spawnSaveGameSelector() {
        GameObject newobject = Instantiate(Resources.Load("UI/SaveGameSelector")) as GameObject;
        return newobject;
    }
}
