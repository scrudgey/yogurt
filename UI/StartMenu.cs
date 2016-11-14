using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections.Generic;

public class StartMenu : MonoBehaviour {
	public AudioClip music;
	private GameObject mainMenu;
	private GameObject newGameMenu;
	private GameObject loadGameMenu;
	private GameObject prompt;
	private GameObject alert;
	
	private enum menuState{anykey, main, startNew, load}
	private menuState state;
	
	void Start(){
		AudioSource source = GetComponent<AudioSource>();
		source.clip = music;
		source.Play();
		mainMenu = transform.Find("MainMenu").gameObject;
		newGameMenu = transform.Find("NewGameMenu").gameObject;
		loadGameMenu = transform.Find("LoadGameMenu").gameObject;
		alert = transform.Find("Alert").gameObject;
		prompt = transform.Find("prompt").gameObject;
		newGameMenu.SetActive(false);
		loadGameMenu.SetActive(false);
		mainMenu.SetActive(false);
		alert.SetActive(false);
		state = menuState.anykey;
	}

	void Update(){
		if (Input.anyKey && state == menuState.anykey){
			state = menuState.main;
			mainMenu.SetActive(true);
			prompt.SetActive(false);
		}
	}
	private void SwitchMenu(menuState switchTo){
		newGameMenu.SetActive(false);
		loadGameMenu.SetActive(false);
		mainMenu.SetActive(false);
		state = switchTo;
		switch (switchTo)
		{
			case menuState.startNew:
			OpenNewGameMenu();
			break;
			case menuState.load:
			loadGameMenu.SetActive(true);
			ConfigLoadMenu();
			break;
			default:
			mainMenu.SetActive(true);
			state = menuState.main;
			break;
		}
	}

	private void OpenNewGameMenu(){
		newGameMenu.SetActive(true);
		InputField input = newGameMenu.transform.Find("InputField").GetComponent<InputField>();
		// InputField inputField = someGameObject.GetComponent<InputField>();
		input.ActivateInputField();
		input.text = SuggestANAme();
	}

	private void ConfigLoadMenu(){
		GameObject saveGamePanel = loadGameMenu.transform.Find("Scroll View/Viewport/Content").gameObject;
		int children = saveGamePanel.transform.childCount;
		for (int i = 0; i < children; ++i)
			Destroy(saveGamePanel.transform.GetChild(i).gameObject);
		DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath);
		DirectoryInfo[] dirs = info.GetDirectories();
		foreach(DirectoryInfo dir in dirs){
			GameObject newSelector = spawnSaveGameSelector();
			newSelector.transform.SetParent(saveGamePanel.transform, false);
			SaveGameSelectorScript script = newSelector.GetComponent<SaveGameSelectorScript>();
			script.ConfigValues();
			script.nameText.text = dir.Name;
			script.saveName = dir.Name;
			if (dir.Name != "test"){
				GameData data = GameManager.Instance.LoadGameData(dir.Name);
                TimeSpan t = TimeSpan.FromSeconds(0f);
				if (data != null){
					t = TimeSpan.FromSeconds(data.secondsPlayed);
					script.dateText.text = data.saveDate;
				}
				script.timeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}s", 
											t.Hours, 
											t.Minutes, 
											t.Seconds).ToString();
			}
		}
	}
	
	private GameObject spawnSaveGameSelector(){
		GameObject newobject = Instantiate(Resources.Load("UI/SaveGameSelector")) as GameObject;
		return newobject;
	}
	
	public void StartButton(){
		SwitchMenu(menuState.startNew);
	}
	public void LoadButton(){
		SwitchMenu(menuState.load);
	}
	public void NewGameCancel(){
		SwitchMenu(menuState.main);
	}
	public void LoadGameCancel(){
		SwitchMenu(menuState.main);
	}
	public void ContinueButton(){
		Debug.Log("pressed Continue");
	}
	public void SettingsButton(){
		Debug.Log("pressed Settings");
	}
	public void QuitButton(){
		Application.Quit();
	}
	

	
	public void AlertOK(){
		alert.SetActive(false);
		switch (state)
		{
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
	
	public void NewGameOK(){
		InputField field = newGameMenu.transform.Find("InputField").gameObject.GetComponent<InputField>();
		string newName = field.text;
		if (newName.Length == 0){
			ShowAlert("Bad name!!!");
		} else {
			GameManager.Instance.saveGameName = newName;
			GameManager.Instance.NewGame();
		}
	}
	public void LoadGameSelect(SaveGameSelectorScript saveGame){
		GameManager.Instance.saveGameName = saveGame.saveName;
		GameManager.Instance.LoadGameDataIntoMemory(saveGame.saveName);
	}
	public void ShowAlert(string text){
		alert.SetActive(true);
		Text alertText = alert.transform.Find("Text").GetComponent<Text>();
		alertText.text = text;
	}

	public string SuggestANAme(){
		List<string> names = new List<string>(){
			"Shemp",
			"Corona",
			"Frog",
			"Crogus",
			"Smitty",
			"Scrummy Bingus",
			"Isis"
		};
		return names[UnityEngine.Random.Range( 0, names.Count )];
	}
}
