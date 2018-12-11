using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections.Generic;

public class StartMenu : MonoBehaviour {
    public AudioClip music;
    public GameObject logo;
    public GameObject mainMenu;
    public GameObject newGameMenu;
    public LoadGameMenu loadGameMenu;
    public SaveInspector saveInspector;
    public ItemCollectionInspector itemCollectionInspector;
    public AchievementBrowser achievementBrowser;
    public GameObject prompt;
    public GameObject alert;
    private enum menuState { anykey, main, startNew, load }
    private menuState state;

    void Start() {
        AudioSource source = GetComponent<AudioSource>();
        source.clip = music;
        source.Play();
        prompt.SetActive(true);
        newGameMenu.SetActive(false);
        loadGameMenu.gameObject.SetActive(false);
        saveInspector.gameObject.SetActive(false);
        itemCollectionInspector.gameObject.SetActive(false);
        achievementBrowser.gameObject.SetActive(false);
        mainMenu.SetActive(false);
        alert.SetActive(false);
        logo.SetActive(true);
        state = menuState.anykey;
    }

    void Update() {
        if (Input.anyKey && state == menuState.anykey) {
            state = menuState.main;
            mainMenu.SetActive(true);
            prompt.SetActive(false);
        }
    }
    private void SwitchMenu(menuState switchTo) {
        newGameMenu.SetActive(false);
        loadGameMenu.gameObject.SetActive(false);
        mainMenu.SetActive(false);
        state = switchTo;
        switch (switchTo) {
            case menuState.startNew:
                OpenNewGameMenu();
                break;
            case menuState.load:
                loadGameMenu.gameObject.SetActive(true);
                loadGameMenu.ConfigLoadMenu(this);
                break;
            default:
                mainMenu.SetActive(true);
                state = menuState.main;
                break;
        }
    }

    private void OpenNewGameMenu() {
        newGameMenu.SetActive(true);
        InputField input = newGameMenu.transform.Find("InputField").GetComponent<InputField>();
        input.ActivateInputField();
        input.text = SuggestANAme();
    }


    public void StartButton() {
        SwitchMenu(menuState.startNew);
    }
    public void LoadButton() {
        SwitchMenu(menuState.load);
    }
    public void NewGameCancel() {
        SwitchMenu(menuState.main);
    }
    public void LoadGameCancel() {
        SwitchMenu(menuState.main);
    }
    public void ContinueButton() {
        Debug.Log("pressed Continue");
    }
    public void SettingsButton() {
        Debug.Log("pressed Settings");
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
    }
    public void NewGameOK() {
        InputField field = newGameMenu.transform.Find("InputField").gameObject.GetComponent<InputField>();
        string newName = field.text;
        if (newName.Length == 0) {
            ShowAlert("Bad name!!!");
        } else {
            GameManager.Instance.saveGameName = newName;
            GameManager.Instance.NewGame();
        }
    }
    public void InspectSaveGame(SaveGameSelectorScript saveGame) {
        loadGameMenu.gameObject.SetActive(false);
        saveInspector.gameObject.SetActive(true);
        saveInspector.Initialize(this, saveGame);
    }
    public void InspectItemCollection(GameData data){
        saveInspector.gameObject.SetActive(false);
        itemCollectionInspector.gameObject.SetActive(true);
        itemCollectionInspector.Initialize(this, data);
        // ite
    }
    public void CloseItemCollectionInspector(){
        itemCollectionInspector.gameObject.SetActive(false);
        saveInspector.gameObject.SetActive(true);
    }
    public void InspectAchievements(GameData data){
        saveInspector.gameObject.SetActive(false);
        achievementBrowser.gameObject.SetActive(true);
        achievementBrowser.Initialize(data);
    }
    public void CloseAchievementBrowser(){
        achievementBrowser.gameObject.SetActive(false);
        saveInspector.gameObject.SetActive(true);
    }
    public void ShowAlert(string text) {
        alert.SetActive(true);
        Text alertText = alert.transform.Find("Text").GetComponent<Text>();
        alertText.text = text;
    }
    public string SuggestANAme() {
        List<string> names = new List<string>(){
            "Shemp",
            "Corona",
            "Frog",
            "Crogus",
            "Smitty",
            "Scrummy Bingus",
            "Fang",
            "Ziplock McBaggins",
            "Questor",
            "Bigfoot",
            "Quangle Cringleberry",
            "Wicks Cherrycoke",
            "Sauncho Smilax",
            "Moe",
            "Tyrone Slothrop",
            "Voorwerp",
            "Scrauncho",
            "Bengis",
            "Pingy",
            "Scrints",
            "Chungus",
            "Beppo"
        };
        return names[UnityEngine.Random.Range(0, names.Count)];
    }
}
