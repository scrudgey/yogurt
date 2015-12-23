﻿using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
// using System;

[XmlRoot("GameData")]
public class GameData{
	public List<string> collectedItems;
	public string lastSavedPlayerPath;
	public string lastSavedScenePath;
	public string lastPlayerName;
	public System.DateTime saveDate;
	public float secondsPlayed;
	public string lastScene;
	public GameData(){
		
	}
	public GameData(GameManager instance){
		collectedItems = instance.collectedItems;
		lastSavedPlayerPath = instance.lastSavedPlayerPath;
		lastPlayerName = instance.lastPlayerName;
		saveDate = System.DateTime.Now;
		secondsPlayed = instance.timePlayed + instance.timeSinceLastSave;
		lastScene = Application.loadedLevelName;
	}
}
public class GameManager : Singleton<GameManager> {

	protected GameManager(){}
	public string saveGameName = "test";
	public string message ="smoke weed every day";	
	private CameraControl cameraControl;
	private Camera cam;
	public GameObject playerObject;
	public string lastSavedPlayerPath;
	public string lastPlayerName;
	public string lastSavedScenePath;
	private int entryID;
	public float gravity = 1.6f;
	public List<string> collectedItems;
	public List<string> collectedFood;
	public List<string> collectedClothes;
	public Dictionary<string, bool> itemCheckedOut;
	public float timeSinceLastSave = 0f;
	public float timePlayed;
	
	public void CheckItemCollection(Inventory inv, GameObject owner){
		if (owner != playerObject)
			return;
		if (!inv.holding)
			return;
		string filename = Toolbox.Instance.CloneRemover(inv.holding.name);
		filename = Toolbox.Instance.ReplaceUnderscore(filename);
		if (collectedItems.Contains(filename))
			return;
		UnityEngine.Object testPrefab = Resources.Load("prefabs/"+filename);
		if (testPrefab != null){
			CollectItem(filename);
		}
	}
	
	public string LevelSavePath(){
		string path = "";
		path = Path.Combine(Application.persistentDataPath, saveGameName);
		if (!Directory.Exists(path))
		Directory.CreateDirectory(path);
		path = Path.Combine(path, Application.loadedLevelName+"_state.xml");
		lastSavedScenePath = path;
		return path;
	}
	
	public string PlayerSavePath(){
		string path = "";
		path = Path.Combine(Application.persistentDataPath, saveGameName);
		if (!Directory.Exists(path))
		Directory.CreateDirectory(path);
		path = Path.Combine(path, "player_"+GameManager.Instance.playerObject.name+"_state.xml");
		lastSavedPlayerPath = path;
		lastPlayerName = GameManager.Instance.playerObject.name;
		return path;
	}
	
	public void SaveGameData(){
			var serializer = new XmlSerializer(typeof(GameData));
			string path = Path.Combine(Application.persistentDataPath, saveGameName);
			path = Path.Combine(path, "game.xml");
			GameData data = new GameData(this);
			FileStream sceneStream = File.Create(path);
			serializer.Serialize(sceneStream, data);
			sceneStream.Close();
			timePlayed += timeSinceLastSave;
			timeSinceLastSave = 0f;
	}
	public GameData LoadGameData(string gameName){
		GameData data = null;
		var serializer = new XmlSerializer(typeof(GameData));
		string path = Path.Combine(Application.persistentDataPath, gameName);
		path = Path.Combine(path, "game.xml");
		if (File.Exists(path)){
			var dataStream = new FileStream(path, FileMode.Open);
			data = serializer.Deserialize(dataStream) as GameData;
			dataStream.Close();
		}
		return data;
	}
	
	public void LoadGameDataIntoMemory(string gameName){
		GameData data = LoadGameData(gameName);
		collectedItems = new List<string>();
		itemCheckedOut = new Dictionary<string, bool>();
		if (data == null){
			InitValues();
			NewGame();
		} else {
			collectedItems = data.collectedItems;
			foreach(string item in collectedItems){
				itemCheckedOut[item] = false;
			}
			lastSavedPlayerPath = data.lastSavedPlayerPath;
			lastSavedScenePath = data.lastSavedScenePath;
			lastPlayerName = data.lastPlayerName;
			timePlayed = data.secondsPlayed;
			timeSinceLastSave = 0f;
			if (data.lastScene != null){
				Application.LoadLevel(data.lastScene);
			} else {
				NewGame();
			}
		}
	}
	
	public void CollectItem(string name){
		// TODO: add achievement-like popup effect here
		// Debug.Log("collecting "+name);
		collectedItems.Add(name);
		itemCheckedOut[name] = false;
	}
	public void RetrieveCollectedItem(string name){
		if (itemCheckedOut[name])
		return;
		// GameObject newObject = Instantiate(Resources.Load("prefabs/"+name), playerObject.transform.position, Quaternion.identity) as GameObject;
		Instantiate(Resources.Load("prefabs/"+name), playerObject.transform.position, Quaternion.identity);
		itemCheckedOut[name] = true;
	}
	public void FocusIntrinsicsChanged(Intrinsic intrinsic){
		if (intrinsic.telepathy.boolValue){
			playerObject.SendMessage("Say","I can hear thoughts!",SendMessageOptions.DontRequireReceiver);
			cam.cullingMask |= 1 << LayerMask.NameToLayer("thoughts");
		} else {
			try {
				cam.cullingMask &=  ~(1 << LayerMask.NameToLayer("thoughts"));
			} catch {
				Debug.Log(cam);
				Debug.Log("Weird telepathy culling mask issue");
			}
		}
	}

	public void SetFocus(GameObject target){
		playerObject = target;
		Controller.Instance.focus = target.GetComponent<Controllable>();
		cameraControl = FindObjectOfType<CameraControl>();
		if (cameraControl)
			cameraControl.focus = target;
	}

	void Start(){
		MySaver.CleanupSaves();
		// Cursor.SetCursor((Texture2D)Resources.Load("UI/cursor1"), Vector2.zero, CursorMode.Auto);
		if (Application.loadedLevelName != "title"){
			InitValues();
		}
	}
	
	public void InitValues(){
		// TODO: add a default player condition here
		playerObject = GameObject.Find("Tom");		
		collectedItems = new List<string>();
		itemCheckedOut = new Dictionary<string, bool>();
		timePlayed = 0f;
		timeSinceLastSave = 0f;
		if (!playerObject){
			playerObject = GameObject.Instantiate(Resources.Load("prefabs/Tom")) as GameObject;
		}
		cam = GameObject.FindObjectOfType<Camera>();
		SetFocus(playerObject);
	}

	void PostLoad(GameObject playerObject){
//		playerObject = GameObject.Find(lastPlayerName);	
		cam = GameObject.FindObjectOfType<Camera>();
		if (playerObject){
			SetFocus(playerObject);
			Intrinsics intrinsics = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(playerObject);
			FocusIntrinsicsChanged(intrinsics.NetIntrinsic());
		}	
	}

	public void LeaveScene(string toSceneName,int toEntryNumber){
		// call mysaver, tell it to save scene and player separately
		MySaver.Save();
		// unity load saved editor scene file
		entryID = toEntryNumber;
		Application.LoadLevel(toSceneName);
	}
	
	public void NewGame(){
		Application.LoadLevel("house");
	}
	
	void OnLevelWasLoaded(int level) {
		// call scene load routine & load player
		GameObject player = MySaver.LoadScene();
		// initialize values re: player object focus
		PostLoad(player);
		PlayerEnter();
	}
	void PlayerEnter(){
		if (playerObject){
			List<Doorway> doorways = new List<Doorway>( GameObject.FindObjectsOfType<Doorway>() );
			foreach (Doorway doorway in doorways){
				if (doorway.entryID == entryID){
					Vector3 tempPos = doorway.transform.position;
					tempPos.y = tempPos.y - 0.05f;
					playerObject.transform.position = tempPos;
				}
			}
		}
	}
	
	void Update(){
		timeSinceLastSave += Time.deltaTime;
	}



}

