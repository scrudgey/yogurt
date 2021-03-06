﻿using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
public class UIButtonCallbacks : MonoBehaviour {
    public void FightButtonClick() {
        if (InputController.Instance.state == InputController.ControlState.inMenu)
            return;
        InputController.Instance.controller.ToggleFightMode();
        UINew.Instance.UpdateTopActionButtons();
    }
    public void SpeakButtonClick() {
        UINew.Instance.ShowMenu(UINew.MenuType.speech);
    }
    public void InventoryButtonClick() {
        UINew.Instance.ShowInventoryMenu();
    }
    public void SaveButtonClick() {
        MySaver.Save();
        MySaver.SaveObjectDatabase();
    }
    public void LoadButtonClick() {
        GameManager.Instance.SetFocus(MySaver.LoadScene());
    }
    public Boolean faded;
    public void TestButtonClick() {

        // Speech speech = GameObject.FindObjectOfType<Speech>();
        // MessageSpeech message = new MessageSpeech("what is this janky-ass bullshit? an ass? assholes!");
        // speech.Say(message);

        EnumerateCollectibles();

        // GameObject target = GameObject.Find("blue_shirt");
        // if (target != null) {
        //     PhysicalBootstrapper pb = target.GetComponent<PhysicalBootstrapper>();
        //     Godhead.BlessItem(pb);
        // }

        // string path = Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName);
        // DirectoryInfo dataDir = new DirectoryInfo(path);
        // dataDir.Delete(true);

        // Debug.Log("test");
        // Grammar g = new Grammar();
        // g.Load("insult");
        // Debug.Log(g.Parse("{main}"));

        // if (faded) {
        //     UINew.Instance.FadeIn(() => { Debug.Log("done faded"); });
        // } else {
        //     UINew.Instance.FadeOut(() => { Debug.Log("done faded"); });
        // }
        // faded = !faded;

        // GameObject keg = GameObject.Find("gunpowder_keg");
        // Godhead.BlessItem(keg.GetComponent<PhysicalBootstrapper>(), doInit: false);
    }
    public void HypnosisButtonClick() {
        if (InputController.Instance.state == InputController.ControlState.inMenu)
            return;
        if (InputController.Instance.state != InputController.ControlState.hypnosisSelect) {
            InputController.Instance.state = InputController.ControlState.hypnosisSelect;
        } else {
            InputController.Instance.state = InputController.ControlState.normal;
        }
    }
    public void VomitButtonClick() {
        if (InputController.Instance.state == InputController.ControlState.inMenu)
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
        GameManager.Instance.SetMusicOn(selected);
    }


    public void EnumerateCollectibles() {
        GameObject[] prefabs = Resources.LoadAll("prefabs/", typeof(GameObject))
            .Cast<GameObject>()
            .ToArray();
        HashSet<string> items = new HashSet<string>();
        HashSet<string> food = new HashSet<string>();
        HashSet<string> clothes = new HashSet<string>();
        foreach (GameObject prefab in prefabs) {
            Edible objectEdible = prefab.GetComponent<Edible>();
            if (objectEdible != null) {
                food.Add(prefab.name);
            }
            if (prefab.GetComponent<Uniform>() || prefab.GetComponent<Hat>()) {
                clothes.Add(prefab.name);
            }
            if (prefab.GetComponent<Pickup>()) {
                items.Add(prefab.name);
            }
        }
        Debug.Log("number of items: " + items.Count().ToString());
        Debug.Log("number of food: " + food.Count().ToString());
        Debug.Log("number of clothes: " + food.Count().ToString());
        // commercials
        TextAsset[] commercials = Resources.LoadAll("data/commercials/", typeof(TextAsset))
            .Cast<TextAsset>()
            .ToArray();
        Debug.Log("number of commercials: " + commercials.Count().ToString());
        // achievements
        GameObject[] achievementPrefabs = Resources.LoadAll("achievements/", typeof(GameObject))
            .Cast<GameObject>()
            .ToArray();
        Debug.Log("number of achievements: " + achievementPrefabs.Count().ToString());
    }
}
