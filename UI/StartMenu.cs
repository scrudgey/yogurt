﻿using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class StartMenu : MonoBehaviour {
    public GameObject logo;
    public GameObject mainMenu;
    public GameObject newGameMenu;
    public LoadGameMenu loadGameMenu;
    public SaveInspector saveInspector;
    public ItemCollectionInspector itemCollectionInspector;
    public AchievementBrowser achievementBrowser;
    public StatsBrowser statsBrowser;
    public DeleteInspector deleteInspector;
    public GameObject settingsMenu;
    public GameObject prompt;
    public GameObject alert;
    private enum menuState { anykey, main, startNew, load, settings }
    private menuState state;
    private Gender selectedGender;
    private SkinColor selectedSkinColor;
    public Sprite maleSprite;
    public Sprite femaleSprite;
    public Sprite PortraitTom;
    public Sprite PortraitTina;
    public Sprite PortraitBrm;
    public Sprite PortraitBrf;
    public Sprite PortraitBlm;
    public Sprite PortraitBlf;

    public Image previewPrefab;
    public Image previewPortrait;

    // public MyControls controls;
    public AudioClip menuOpenSound;
    public AudioClip menuOpenMoreSound;
    public AudioClip menuClosedSound;
    public AudioClip effectSound;
    public AudioClip flushSound;
    bool keypressedThisFrame;
    void Awake() {
        InputController.Instance.EscapeAction.action.performed += _ => keypressedThisFrame = true;
    }


    void Start() {
        AudioSource source = GetComponent<AudioSource>();
        source.Play();
        prompt.SetActive(true);
        newGameMenu.SetActive(false);
        loadGameMenu.gameObject.SetActive(false);
        saveInspector.gameObject.SetActive(false);
        itemCollectionInspector.gameObject.SetActive(false);
        achievementBrowser.gameObject.SetActive(false);
        statsBrowser.gameObject.SetActive(false);
        deleteInspector.gameObject.SetActive(false);
        mainMenu.SetActive(false);
        alert.SetActive(false);
        // logo.SetActive(true);
        state = menuState.anykey;
    }
    void Update() {
        if (Keyboard.current.anyKey.isPressed && state == menuState.anykey) {
            state = menuState.main;
            mainMenu.SetActive(true);
            prompt.SetActive(false);
            GameManager.Instance.PlayPublicSound(menuOpenSound);
        }
        if (keypressedThisFrame) {
            if (saveInspector.gameObject.activeInHierarchy) {
                saveInspector.gameObject.SetActive(false);
                loadGameMenu.gameObject.SetActive(true);
                loadGameMenu.ConfigLoadMenu(this);
                return;
            }
            if (achievementBrowser.gameObject.activeInHierarchy) {
                CloseAchievementBrowser();
            }
            if (statsBrowser.gameObject.activeInHierarchy) {
                CloseStatsBrowser();
            }
            if (itemCollectionInspector.gameObject.activeInHierarchy) {
                CloseItemCollectionInspector();
            }
            if (loadGameMenu.gameObject.activeInHierarchy) {
                loadGameMenu.gameObject.SetActive(false);
                mainMenu.SetActive(true);
                state = menuState.main;
            }
            if (newGameMenu.gameObject.activeInHierarchy) {
                newGameMenu.SetActive(false);
                mainMenu.SetActive(true);
                state = menuState.main;
            }
            if (settingsMenu.gameObject.activeInHierarchy) {
                settingsMenu.SetActive(false);
                mainMenu.SetActive(true);
                state = menuState.main;
            }
        }
        keypressedThisFrame = false;
    }
    private void SwitchMenu(menuState switchTo) {
        newGameMenu.SetActive(false);
        loadGameMenu.gameObject.SetActive(false);
        mainMenu.SetActive(false);
        saveInspector.gameObject.SetActive(false);
        settingsMenu.SetActive(false);
        deleteInspector.gameObject.SetActive(false);
        switch (switchTo) {
            case menuState.startNew:
                OpenNewGameMenu();
                break;
            case menuState.load:
                loadGameMenu.gameObject.SetActive(true);
                loadGameMenu.ConfigLoadMenu(this);
                break;
            case menuState.settings:
                settingsMenu.gameObject.SetActive(true);
                break;
            default:
                mainMenu.SetActive(true);
                state = menuState.main;
                break;
        }
        state = switchTo;
    }

    private void OpenNewGameMenu() {
        newGameMenu.SetActive(true);
        if (state != menuState.settings) {
            RandomizePlayer();
        } else {
            RandomizeName();
        }
        GameManager.Instance.PlayPublicSound(menuOpenSound);
    }


    public void StartButton() {
        SwitchMenu(menuState.startNew);
    }
    public void LoadButton() {
        SwitchMenu(menuState.load);
        GameManager.Instance.PlayPublicSound(menuOpenSound);
    }
    public void NewGameCancel() {
        SwitchMenu(menuState.main);
        GameManager.Instance.PlayPublicSound(menuClosedSound);
    }
    public void LoadGameCancel() {
        SwitchMenu(menuState.main);
        GameManager.Instance.PlayPublicSound(menuClosedSound);
    }
    public void DeleteGame(SaveGameSelectorScript saveGame) {
        // Debug.Log($"delete {saveGame.saveName}");
        MySaver.DeleteSave(saveGame.saveName);
        deleteInspector.gameObject.SetActive(false);
        SwitchMenu(menuState.load);
        GameManager.Instance.PlayPublicSound(flushSound);
    }
    public void SettingsButton() {
        SwitchMenu(menuState.settings);
        GameManager.Instance.PlayPublicSound(menuOpenSound);
    }
    public void CloseSettingsMenu() {
        SwitchMenu(menuState.main);
        GameManager.Instance.PlayPublicSound(menuClosedSound);
    }
    public void ContinueButton() {
        // TODO: catch if there is no save game
        Dictionary<string, GameData> dataDirs = new Dictionary<string, GameData>();
        List<string> datas = new List<string>();
        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath);
        DirectoryInfo[] dirs = info.GetDirectories();
        foreach (DirectoryInfo dir in dirs) {
            if (dir.Name == "test" || dir.Name == "Unity" || dir.Name == "crashdump")
                continue;
            GameData data = GameManager.Instance.LoadGameData(dir.Name);
            datas.Add(dir.Name);
            dataDirs[dir.Name] = data;
        }
        if (datas.Count == 0)
            return;
        datas.Sort((d1, d2) => dataDirs[d2].saveDateTime.CompareTo(dataDirs[d1].saveDateTime));
        Debug.Log(datas[0]);
        GameManager.Instance.LoadGameDataIntoMemory(datas[0]);
    }

    public void QuitButton() {
        Application.Quit();
    }
    public void AlertOK() {
        alert.SetActive(false);
        switch (state) {
            case menuState.load:
                SwitchMenu(menuState.load);
                break;
            case menuState.startNew:
                SwitchMenu(menuState.startNew);
                break;
            default:
                SwitchMenu(menuState.main);
                break;
        }
        GameManager.Instance.PlayPublicSound(menuClosedSound);
    }
    public void NewGameOK() {
        InputField field = newGameMenu.transform.Find("InputField").gameObject.GetComponent<InputField>();
        string newName = field.text;
        if (newName.Length == 0 || newName == "test" || newName == "Unity" || newName == "crashdump") {
            ShowAlert("Bad name!!!");
            return;
        }
        string path = Path.Combine(Application.persistentDataPath, newName);
        if (Directory.Exists(path)) {
            ShowAlert(newName + " already exists...?");
            return;
        }
        GameManager.Instance.data = GameManager.Instance.InitializedGameData();
        GameManager.Instance.data.defaultGender = selectedGender;
        GameManager.Instance.data.defaultSkinColor = selectedSkinColor;
        if (selectedGender == Gender.male) {
            switch (selectedSkinColor) {
                default:
                case SkinColor.light:
                    GameManager.Instance.data.prefabName = "Tom";
                    break;
                case SkinColor.dark:
                    GameManager.Instance.data.prefabName = "Brm";
                    break;
                case SkinColor.darker:
                    GameManager.Instance.data.prefabName = "Blm";
                    break;
            }
        } else {
            switch (selectedSkinColor) {
                default:
                case SkinColor.light:
                    GameManager.Instance.data.prefabName = "Tina";
                    break;
                case SkinColor.dark:
                    GameManager.Instance.data.prefabName = "Brf";
                    break;
                case SkinColor.darker:
                    GameManager.Instance.data.prefabName = "Blf";
                    break;
            }
        }
        GameManager.Instance.saveGameName = newName;
        GameManager.Instance.SaveGameData();
        GameManager.Instance.NewGame();
    }
    public void InspectSaveGame(SaveGameSelectorScript saveGame) {
        loadGameMenu.gameObject.SetActive(false);
        saveInspector.gameObject.SetActive(true);
        saveInspector.Initialize(this, saveGame);
        GameManager.Instance.PlayPublicSound(menuOpenMoreSound);
    }
    public void DeleteSaveGamePrompt(SaveGameSelectorScript saveGame) {
        loadGameMenu.gameObject.SetActive(false);
        deleteInspector.gameObject.SetActive(true);
        deleteInspector.Initialize(this, saveGame);
        GameManager.Instance.PlayPublicSound(menuOpenMoreSound);
    }
    public void InspectItemCollection(GameData data) {
        saveInspector.gameObject.SetActive(false);
        itemCollectionInspector.gameObject.SetActive(true);
        itemCollectionInspector.Initialize(this, data);
        GameManager.Instance.PlayPublicSound(menuOpenMoreSound);
    }
    public void CloseItemCollectionInspector() {
        itemCollectionInspector.gameObject.SetActive(false);
        saveInspector.gameObject.SetActive(true);
        // GameManager.Instance.PlayPublicSound(menuClosedSound);
    }
    public void InspectAchievements(GameData data) {
        saveInspector.gameObject.SetActive(false);
        achievementBrowser.gameObject.SetActive(true);
        achievementBrowser.Initialize(data);
        GameManager.Instance.PlayPublicSound(menuOpenMoreSound);
    }
    public void CloseAchievementBrowser() {
        achievementBrowser.gameObject.SetActive(false);
        saveInspector.gameObject.SetActive(true);
        // GameManager.Instance.PlayPublicSound(menuClosedSound);
    }
    public void InspectStats(GameData data) {
        saveInspector.gameObject.SetActive(false);
        statsBrowser.gameObject.SetActive(true);
        statsBrowser.Initialize(data);
        GameManager.Instance.PlayPublicSound(menuOpenMoreSound);
    }
    public void CloseStatsBrowser() {
        statsBrowser.gameObject.SetActive(false);
        saveInspector.gameObject.SetActive(true);
        // GameManager.Instance.PlayPublicSound(menuClosedSound);
    }

    public void ShowAlert(string text) {
        alert.SetActive(true);
        Text alertText = alert.transform.Find("Text").GetComponent<Text>();
        alertText.text = text;
        GameManager.Instance.PlayPublicSound(menuOpenMoreSound);
    }
    public void RandomizePlayer() {
        InputField input = newGameMenu.transform.Find("InputField").GetComponent<InputField>();

        Array genderValues = Enum.GetValues(typeof(Gender));
        List<SkinColor> skinValues = new List<SkinColor>() { SkinColor.light, SkinColor.dark, SkinColor.darker };

        Gender randomGender = (Gender)genderValues.GetValue(UnityEngine.Random.Range(0, genderValues.Length));
        SkinColor randomSkin = skinValues[UnityEngine.Random.Range(0, skinValues.Count)];

        GenderCallback(randomGender.ToString());
        SkinCallback(randomSkin.ToString());

        input.ActivateInputField();
        input.text = Toolbox.SuggestWeirdName();
    }
    public void RandomizeName() {
        InputField input = newGameMenu.transform.Find("InputField").GetComponent<InputField>();

        input.ActivateInputField();
        input.text = Toolbox.SuggestWeirdName();
    }

    public void GenderCallback(string gender) {
        switch (gender) {
            case "female":
                selectedGender = Gender.female;
                break;
            case "male":
                selectedGender = Gender.male;
                break;
            default:
                Debug.Log("malformed gender callback");
                goto case "male";
        }
        UpdatePlayerPreview();
    }
    public void SkinCallback(string skinColor) {
        switch (skinColor) {
            case "light":
                selectedSkinColor = SkinColor.light;
                break;
            case "dark":
                selectedSkinColor = SkinColor.dark;
                break;
            case "darker":
                selectedSkinColor = SkinColor.darker;
                break;
            default:
                Debug.Log("malformed skincolor callback");
                goto case "light";
        }
        UpdatePlayerPreview();
    }
    public void UpdatePlayerPreview() {
        // TODO: update portrait

        Sprite newSprite = null;
        if (selectedGender == Gender.male) {
            switch (selectedSkinColor) {
                default:
                case SkinColor.light:
                    previewPortrait.sprite = PortraitTom;
                    break;
                case SkinColor.dark:
                    previewPortrait.sprite = PortraitBrm;
                    break;
                case SkinColor.darker:
                    previewPortrait.sprite = PortraitBlm;
                    break;
            }
            newSprite = maleSprite;
        } else {
            switch (selectedSkinColor) {
                default:
                case SkinColor.light:
                    previewPortrait.sprite = PortraitTina;
                    break;
                case SkinColor.dark:
                    previewPortrait.sprite = PortraitBrf;
                    break;
                case SkinColor.darker:
                    previewPortrait.sprite = PortraitBlf;
                    break;
            }
            newSprite = femaleSprite;
        }

        previewPrefab.sprite = Toolbox.ApplySkinToneToSprite(newSprite, selectedSkinColor);
    }
}
