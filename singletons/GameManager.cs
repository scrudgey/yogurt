using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
// using System.Text.RegularExpressions;
// using System;

[XmlRoot("GameData")]
public class GameData{
    public float money;
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
        money = instance.money;
	}
}
public class GameManager : Singleton<GameManager> {

	protected GameManager(){}
	public string saveGameName = "test";
	public string message ="smoke weed every day";	
	private CameraControl cameraControl;
	public Camera cam;
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
    public List<Commercial> unlockedCommercials;
    public List<Commercial> completeCommercials;
    public Commercial activeCommercial;
    private float sceneTime;
    private bool doScriptPrompt = false;
    public float money;
	
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
			// InitValues();
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
            money = data.money;
			if (data.lastScene != null){
				Application.LoadLevel(data.lastScene);
			} else {
                Application.LoadLevel("house");
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
		Instantiate(Resources.Load("prefabs/"+name), playerObject.transform.position, Quaternion.identity);
		itemCheckedOut[name] = true;
	}
	public void FocusIntrinsicsChanged(Intrinsic intrinsic){
		if (intrinsic.telepathy.boolValue){
			playerObject.SendMessage("Say", "I can hear thoughts!", SendMessageOptions.DontRequireReceiver);
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
        // unlockedCommercials = listAllCommercials();
		// Cursor.SetCursor((Texture2D)Resources.Load("UI/cursor1"), Vector2.zero, CursorMode.Auto);
		if (Application.loadedLevelName != "title"){
            NewGame(switchlevel: false);
		}
	}
    
    public List<Commercial> listAllCommercials(){
        List<Commercial> passList = new List<Commercial>();
        Object[] XMLObjects = Resources.LoadAll("data/commercials");
        List<TextAsset> xmlList = new List<TextAsset>();
        for (int i = 0; i < XMLObjects.Length; i++){
            xmlList.Add((TextAsset)XMLObjects[i]);
        }
        var serializer = new XmlSerializer(typeof(Commercial));
        foreach (TextAsset asset in xmlList){
            var reader = new System.IO.StringReader(asset.text);
            Commercial newCommercial = serializer.Deserialize(reader) as Commercial;
            passList.Add(newCommercial);
        }
        return passList;
    }
    
    public Commercial LoadCommercialByName(string filename){
        Commercial commercial = null;
        TextAsset xml = Resources.Load("data/commercials/"+filename) as TextAsset;
        var serializer = new XmlSerializer(typeof(Commercial));
        var reader = new System.IO.StringReader(xml.text);
        commercial = serializer.Deserialize(reader) as Commercial;
        return commercial;
    }

	public void LeaveScene(string toSceneName, int toEntryNumber){
		// call mysaver, tell it to save scene and player separately
		MySaver.Save();
		// unity load saved editor scene file
		entryID = toEntryNumber;
		Application.LoadLevel(toSceneName);
	}
	
	void OnLevelWasLoaded(int level) {
        sceneTime = 0f;
        GameObject player = MySaver.LoadScene();
            // initialize values re: player object focus
        cam = GameObject.FindObjectOfType<Camera>();
        if (player){
            SetFocus(player);
            Intrinsics intrinsics = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(player);
            FocusIntrinsicsChanged(intrinsics.NetIntrinsic());
            PlayerEnter();
        }	
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
    
    public void NewDay(){
        MySaver.CleanupSaves();
        Application.LoadLevel("house");
        doScriptPrompt = true;
        sceneTime = 0f;
        entryID = 99;
    }
    
    public void NewGame(bool switchlevel=true){
        if (switchlevel){
    		Application.LoadLevel("house");
            doScriptPrompt = true;
        }
        sceneTime = 0f;
        
        // TODO: add a default player condition here
		playerObject = GameObject.Find("Tom");	
        if (!playerObject){
            playerObject = GameObject.Find("Tom(Clone)");
        }
		collectedItems = new List<string>();
		itemCheckedOut = new Dictionary<string, bool>();
        // unlockedCommercials = listAllCommercials();
        unlockedCommercials = new List<Commercial>();
        unlockedCommercials.Add(LoadCommercialByName("eat1"));
        completeCommercials = new List<Commercial>();
		timePlayed = 0f;
		timeSinceLastSave = 0f;
		if (!playerObject){
			playerObject = GameObject.Instantiate(Resources.Load("prefabs/Tom")) as GameObject;
		}
		cam = GameObject.FindObjectOfType<Camera>();
		SetFocus(playerObject);
	}
    void ScriptPrompt(){
        GameObject menu = Instantiate(Resources.Load("UI/ScriptSelector")) as GameObject;
        menu.GetComponent<Canvas>().worldCamera = cam;
    }
	void Update(){
		timeSinceLastSave += Time.deltaTime;
        sceneTime += Time.deltaTime;
        if (sceneTime > 2f && doScriptPrompt){
            doScriptPrompt = false;
            ScriptPrompt();
        }
	}

    public void UnlockCommercial(string filename){
        Commercial unlocked = LoadCommercialByName(filename);
        unlockedCommercials.Add(unlocked);
    }
    public void EvaluateCommercial(Commercial commercial){
        bool success = false;
        if (activeCommercial != null){
            success = commercial.Evaluate(activeCommercial);
        }
        if (success){
            //process reward
            money += activeCommercial.reward;
            foreach (string unlock in activeCommercial.unlockUponCompletion){
               UnlockCommercial(unlock);
            }
            GameObject report = Instantiate(Resources.Load("UI/CommercialReport")) as GameObject;
            report.GetComponent<CommercialReportMenu>().Report(activeCommercial);
            if (activeCommercial.name != "freestyle")
                completeCommercials.Add(activeCommercial);
        } else {
            // do something to display why the commercial is not done yet
        }
    }

}

