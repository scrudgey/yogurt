using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

[XmlRoot("GameData")]
[System.Serializable]
public class GameData{
    public float money;
	public List<string> collectedObjects;
	public List<string> collectedItems;
	public List<string> collectedFood;
	public List<string> collectedClothes;
	public SerializableDictionary<string, bool> itemCheckedOut;
	public string lastSavedPlayerPath;
	public string lastSavedScenePath;
	public string lastPlayerName;
	public string saveDate;
	public float secondsPlayed;
	public string lastScene;
	public int days;
    public List<Commercial> unlockedCommercials;
    public List<Commercial> completeCommercials;
	public int entryID;
	public List<Achievement> achievements;
	public AchievementStats achievementStats = new AchievementStats();
	public GameData(){
		days = 0;
		saveDate = System.DateTime.Now.ToString();
	}
}
public partial class GameManager : Singleton<GameManager> {
	protected GameManager(){}
	public GameData data;
	public string saveGameName = "test";
	public string message = "smoke weed every day";	
	private CameraControl cameraControl;
	public Camera cam;
	public GameObject playerObject;
	public float gravity = 1.6f;
    public Commercial activeCommercial;
    private float sceneTime;
	public float timeSinceLastSave = 0f;
	private float intervalTimer;
    void Start(){
		// Cursor.SetCursor((Texture2D)Resources.Load("UI/cursor1"), Vector2.zero, CursorMode.Auto);
		if (data == null){
			data = InitializedGameData();
		}
		if (saveGameName == "test")
			MySaver.CleanupSaves();
		if (!InCutscene()){
            NewGame(switchlevel: false);
		}
	}
	void Update(){
		timeSinceLastSave += Time.deltaTime;
        sceneTime += Time.deltaTime;
		intervalTimer += Time.deltaTime;
		if (intervalTimer > 3f){
			data.achievementStats.secondsPlayed = timeSinceLastSave;
			CheckAchievements();
			intervalTimer = 0;
		}
	}
	public bool InCutscene(){
		if (SceneManager.GetActiveScene().buildIndex > 1){
            return false;
		} else {
			return true;
		}
	} 
	public void FocusIntrinsicsChanged(Intrinsic intrinsic){
		if (intrinsic.telepathy.boolValue){
			Toolbox.Instance.SendMessage(playerObject, this, new MessageSpeech("I can hear thoughts!"));
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
		Intrinsics intrinsics = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(target);
		FocusIntrinsicsChanged(intrinsics.NetIntrinsic());
		// change UI buttons?
		UINew.Instance.UpdateButtons();
	}

	public void LeaveScene(string toSceneName, int toEntryNumber){
		MySaver.Save();
		data.entryID = toEntryNumber;
		SceneManager.LoadScene(toSceneName);
	}
	void OnLevelWasLoaded(int level) {
		// Debug.Log("on level was loaded");
        sceneTime = 0f;
		if (InCutscene()){
			InitializeNonPlayableLevel();
		} else {
			InitializePlayableLevel(loadLevel: true);
		}
	}
	public void InitializePlayableLevel(bool loadLevel=false){
		// make sure all required parts are in place
		string[] requirements = new string[] {"Main Camera", "EventSystem", "NeoUICanvas"};
		foreach(string requirement in requirements){
			GameObject go = GameObject.Find(requirement);
			if (!go){
				string path = @"required/"+requirement;
				GameObject newgo = GameObject.Instantiate(Resources.Load(path)) as GameObject;
				newgo.name = Toolbox.Instance.ScrubText(newgo.name);
			}
		}
		cam = GameObject.FindObjectOfType<Camera>();
		if (cam){
			Toolbox.Instance.GetOrCreateComponent<CameraControl>(cam.gameObject);
		}
		UINew.Instance.ConfigureUIElements();

		if (loadLevel){
			playerObject = MySaver.LoadScene();
		} else {
			data = InitializedGameData();
			// find or spawn the player character 
			playerObject = GameObject.Find("Tom");	
			if (!playerObject){
				playerObject = GameObject.Find("Tom(Clone)");
			}
			if (!playerObject){
				playerObject = GameObject.Instantiate(Resources.Load("prefabs/Tom")) as GameObject;
			}
			data.entryID = 99;
		}
		SetFocus(playerObject);
		PlayerEnter();
	}
	public void InitializeNonPlayableLevel(){
		UINew.Instance.DisableAllUI();
	}
	void PlayerEnter(){
		if (playerObject == null)
			return;
		List<Doorway> doorways = new List<Doorway>(GameObject.FindObjectsOfType<Doorway>());
		// TODO: can probably make this nicer with LINQ
		foreach (Doorway doorway in doorways){
			if ((doorway.entryID == data.entryID && !doorway.spawnPoint) || (doorway.spawnPoint && data.entryID == 99)){
				Vector3 tempPos = doorway.transform.position;
				tempPos.y = tempPos.y - 0.05f;
				playerObject.transform.position = tempPos;
				// if this is a bed entry, we've got a new day going on!
				if (data.entryID == -99){
					Bed bed = GameObject.FindObjectOfType<Bed>();
					if (bed){
						bed.SleepCutscene();
						playerObject.SetActive(false);
						Outfit outfit = playerObject.GetComponent<Outfit>();
						AdvancedAnimation advancedAnimation = playerObject.GetComponent<AdvancedAnimation>();
						if (outfit != null && advancedAnimation != null){
							advancedAnimation.baseName = "pajamas";
							outfit.wornUniformName = "pajamas";
						}
						// kind of weird to stick this here instead of in the logical place in NewDay()
						// but we should save when the player is in bed, not during cutscene
						MySaver.Save();
					}
				}
			}
		}

	}

	public void NewGame(bool switchlevel=true){
		Debug.Log("New game");
		data = InitializedGameData();
        if (switchlevel){
			NewDayCutscene();
        } else {
			InitializePlayableLevel();
		}
        sceneTime = 0f;
		timeSinceLastSave = 0f;
	}
    public void NewDay(){
		Debug.Log("New day");
        MySaver.CleanupSaves();
		SceneManager.LoadScene("house");
        sceneTime = 0f;
        data.entryID = -99;
    }

	public void NewDayCutscene(){
		data.days += 1;
		SceneManager.LoadScene("morning_cutscene");
        sceneTime = 0f;
        data.entryID = -99;
	}
	public void TitleScreen(){
		SceneManager.LoadScene("title");
		// UINew.Instance.DisableAllUI();
	}
	public GameData InitializedGameData(){
		GameData data = new GameData();
		data.secondsPlayed = 0f;
		data.collectedItems = new List<string>();
		data.collectedObjects = new List<string>();
		data.collectedFood = new List<string>();
		data.collectedClothes = new List<string>();
		data.itemCheckedOut = new SerializableDictionary<string, bool>();
		data.collectedClothes.Add("blue_shirt");
		data.collectedObjects.Add("blue_shirt");
		data.itemCheckedOut["blue_shirt"] = false;
		data.collectedClothes.Add("pajamas");
		data.collectedObjects.Add("pajamas");
		data.itemCheckedOut["pajamas"] = false;

		// initialize commercials
		// TODO: change this temporary hack into something more correct.
        data.unlockedCommercials = new List<Commercial>();
        data.unlockedCommercials.Add(LoadCommercialByName("eat1"));
        data.completeCommercials = new List<Commercial>();

		// initialize achievements
		data.achievements = new List<Achievement>();
		GameObject[] achievementPrefabs = Resources.LoadAll("achievements/", typeof(GameObject))
			.Cast<GameObject>()
			.ToArray();
		foreach (GameObject prefab in achievementPrefabs){
			AchievementComponent component = prefab.GetComponent<AchievementComponent>();
			if (component){
				Achievement cloneAchievement = new Achievement(component.achivement);
				data.achievements.Add(cloneAchievement);
			}
		}
		return data;
	}


// SAVING AND LOADING
    public string LevelSavePath(){
		string path = "";
		path = Path.Combine(Application.persistentDataPath, saveGameName);
		if (!Directory.Exists(path))
		  Directory.CreateDirectory(path);
		path = Path.Combine(path, SceneManager.GetActiveScene().name+"_state.xml");
		data.lastSavedScenePath = path;
		return path;
	}
	public string PlayerSavePath(){
		string path = "";
		path = Path.Combine(Application.persistentDataPath, saveGameName);
		if (!Directory.Exists(path))
		  Directory.CreateDirectory(path);
		// if (GameManager.Instance.playerObject )
		path = Path.Combine(path, "player_"+GameManager.Instance.playerObject.name+"_state.xml");
		data.lastSavedPlayerPath = path;
		data.lastPlayerName = GameManager.Instance.playerObject.name;
		return path;
	}
	public void SaveGameData(){
		data.secondsPlayed += timeSinceLastSave;
		data.lastScene = SceneManager.GetActiveScene().name;

		var serializer = new XmlSerializer(typeof(GameData));
		string path = Path.Combine(Application.persistentDataPath, saveGameName);
		path = Path.Combine(path, "game.xml");
		FileStream sceneStream = File.Create(path);
		serializer.Serialize(sceneStream, data);
		sceneStream.Close();
		timeSinceLastSave = 0f;
	}
	public GameData LoadGameData(string gameName){
		GameData data = null;
		var serializer = new XmlSerializer(typeof(GameData));
		string path = Path.Combine(Application.persistentDataPath, gameName);
		path = Path.Combine(path, "game.xml");
		if (File.Exists(path)){
			try {
				var dataStream = new FileStream(path, FileMode.Open);
				// if (dataStream == null)
				// 	throw new System.ArgumentNullException("filestream could not open");
				data = serializer.Deserialize(dataStream) as GameData;
				dataStream.Close();
			} catch (Exception e){
				Debug.Log("Error loading game data: "+path);
				Debug.Log(e.Message);
				Debug.Log(e.TargetSite);
			}
		}
		return data;
	}
	public void LoadGameDataIntoMemory(string gameName){
		// Debug.Log("Loadsavegame into memory");
		data = LoadGameData(gameName);
		if (data.lastScene != null){
			SceneManager.LoadScene(data.lastScene);
		} else {
			SceneManager.LoadScene("house");
		}
	}
	public void CheckItemCollection(GameObject obj, GameObject owner){
		if (owner != playerObject)
			return;
		string filename = Toolbox.Instance.CloneRemover(obj.name);
		filename = Toolbox.Instance.ReplaceUnderscore(filename);
		if (data.collectedObjects.Contains(filename))
			return;
		UnityEngine.Object testPrefab = Resources.Load("prefabs/"+filename);
		if (testPrefab != null){
			data.collectedObjects.Add(filename);
			data.itemCheckedOut[filename] = false;
			UINew.Instance.PopupCollected(obj);
			if (obj.GetComponent<Uniform>()){
				data.collectedClothes.Add(filename);
			}
			if (obj.GetComponent<Edible>()){
				data.collectedFood.Add(filename);
			}
			if (obj.GetComponent<Pickup>()){
				data.collectedItems.Add(filename);
			}
		}
	}
	public void RetrieveCollectedItem(string name){
		if (data.itemCheckedOut[name])
		return;
		Instantiate(Resources.Load("prefabs/"+name), playerObject.transform.position, Quaternion.identity);
		data.itemCheckedOut[name] = true;
	}
	public void CheckAchievements(){
		if (InCutscene())
			return;
		foreach (Achievement achieve in data.achievements){
			if (!achieve.complete){
				bool pass = achieve.Evaluate(data.achievementStats);
				if (pass){
					achieve.complete = true;
					UINew.Instance.PopupAchievement(achieve);
				}
			}
		}
	}

}

