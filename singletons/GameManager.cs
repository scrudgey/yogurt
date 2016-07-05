using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;

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
	public System.DateTime saveDate;
	public float secondsPlayed;
	public string lastScene;
	public int days;
    public List<Commercial> unlockedCommercials;
    public List<Commercial> completeCommercials;
	public int entryID;
	public GameData(){
		days = 0;
		saveDate = System.DateTime.Now;
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
	
    
    // BASIC UNITY ROUTINES
    void Start(){
		if (data == null){
			data = new GameData();
		}
		if (saveGameName == "test")
			MySaver.CleanupSaves();
		// Cursor.SetCursor((Texture2D)Resources.Load("UI/cursor1"), Vector2.zero, CursorMode.Auto);
		if (SceneManager.GetActiveScene().name != "title"){
            NewGame(switchlevel: false);
		}
	}

    void Update(){
		timeSinceLastSave += Time.deltaTime;
        sceneTime += Time.deltaTime;
	}
 
    // ITEM COLLECTIONS
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
	public void LeaveScene(string toSceneName, int toEntryNumber){
		// call mysaver, tell it to save scene and player separately
		MySaver.Save();
		// unity load saved editor scene file
		data.entryID = toEntryNumber;
		SceneManager.LoadScene(toSceneName);
	}
	void OnLevelWasLoaded(int level) {
		Debug.Log("on level was loaded");
		// TODO: check if the loaded level is a cutscene.
        sceneTime = 0f;
		if (level <= 1)
			return;
        GameObject player = MySaver.LoadScene();
		// initialize values re: player object focus
        cam = GameObject.FindObjectOfType<Camera>();
        if (player){
            SetFocus(player);
            Intrinsics intrinsics = Toolbox.Instance.GetOrCreateComponent<Intrinsics>(player);
            FocusIntrinsicsChanged(intrinsics.NetIntrinsic());
            PlayerEnter();
        }	

		// bed entry on new day
		if (data.entryID == 99){
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
			}
		}
	}
	void PlayerEnter(){
		if (playerObject){
			List<Doorway> doorways = new List<Doorway>( GameObject.FindObjectsOfType<Doorway>() );
			// TODO: can probably make this nicer with LINQ
			foreach (Doorway doorway in doorways){
				if (doorway.entryID == data.entryID){
					Vector3 tempPos = doorway.transform.position;
					tempPos.y = tempPos.y - 0.05f;
					playerObject.transform.position = tempPos;
				}
			}
		}
	}

	public void NewDayCutscene(){
		data.days += 1;
		SceneManager.LoadScene("morning_cutscene");
        sceneTime = 0f;
        data.entryID = 99;
	}
    public void NewDay(){
		Debug.Log("New day");
        MySaver.CleanupSaves();
		SceneManager.LoadScene("house");
        sceneTime = 0f;
        data.entryID = 99;
		MySaver.Save();
    }

    public void NewGame(bool switchlevel=true){
		Debug.Log("New game");
        if (switchlevel){
			NewDayCutscene();
        }
        sceneTime = 0f;
		data.secondsPlayed = 0f;
		timeSinceLastSave = 0f;
        // TODO: add a default player condition here
		playerObject = GameObject.Find("Tom");	
        if (!playerObject){
            playerObject = GameObject.Find("Tom(Clone)");
        }
		if (!playerObject){
			playerObject = GameObject.Instantiate(Resources.Load("prefabs/Tom")) as GameObject;
		}
		if (playerObject){
			foreach(Doorway door in GameObject.FindObjectsOfType<Doorway>()){
				if (door.spawnPoint){
					Vector3 tempPos = door.transform.position;
					tempPos.y = tempPos.y - 0.05f;
					playerObject.transform.position = tempPos;
				}
			}
		}
		data.collectedItems = new List<string>();
		data.collectedObjects = new List<string>();
		data.collectedFood = new List<string>();
		data.collectedClothes = new List<string>();
		data.itemCheckedOut = new SerializableDictionary<string, bool>();

		data.collectedClothes.Add("blue_shirt");
		data.collectedObjects.Add("blue_shirt");
		data.itemCheckedOut["blue_shirt"] = false;

		// TODO: change this temporary hack into something more correct.
        data.unlockedCommercials = new List<Commercial>();
        data.unlockedCommercials.Add(LoadCommercialByName("eat1"));
        data.completeCommercials = new List<Commercial>();
		
		cam = GameObject.FindObjectOfType<Camera>();
		SetFocus(playerObject);
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
		Debug.Log("saving to "+path);
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
			var dataStream = new FileStream(path, FileMode.Open);
			data = serializer.Deserialize(dataStream) as GameData;
			dataStream.Close();
		}
		return data;
	}
	public void LoadGameDataIntoMemory(string gameName){
		Debug.Log("Loadsavegame into memory");
		data = LoadGameData(gameName);
		if (data.lastScene != null){
			// SceneManager.LoadScene("house");
			SceneManager.LoadScene(data.lastScene);
		} else {
			SceneManager.LoadScene("house");
		}
	}

}

