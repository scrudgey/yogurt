using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;

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
	public int days;
	public GameData(){
		days = 0;
	}
	public GameData(GameManager instance){
		saveDate = System.DateTime.Now;
		secondsPlayed = instance.timePlayed + instance.timeSinceLastSave;
		lastScene = SceneManager.GetActiveScene().name;
	}
}
public partial class GameManager : Singleton<GameManager> {

	protected GameManager(){}

	public GameData data;

	public string saveGameName = "test";
	public string message ="smoke weed every day";	
	private CameraControl cameraControl;
	public Camera cam;
	public GameObject playerObject;
	private int entryID;
	public float gravity = 1.6f;
	public List<string> collectedFood;
	public List<string> collectedClothes;
	public Dictionary<string, bool> itemCheckedOut;
	public float timeSinceLastSave = 0f;
	public float timePlayed;
    public List<Commercial> unlockedCommercials;
    public List<Commercial> completeCommercials;
    public Commercial activeCommercial;
    private float sceneTime;
	
    
    // BASIC UNITY ROUTINES
    void Start(){
		data = new GameData();
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
	public void CheckItemCollection(Inventory inv, GameObject owner){
		if (owner != playerObject)
			return;
		if (!inv.holding)
			return;
		string filename = Toolbox.Instance.CloneRemover(inv.holding.name);
		filename = Toolbox.Instance.ReplaceUnderscore(filename);
		if (data.collectedItems.Contains(filename))
			return;
		UnityEngine.Object testPrefab = Resources.Load("prefabs/"+filename);
		if (testPrefab != null){
			CollectItem(filename);
		}
	}
    public void CollectItem(string name){
		// TODO: add achievement-like popup effect here
		data.collectedItems.Add(name);
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
	public void LeaveScene(string toSceneName, int toEntryNumber){
		// call mysaver, tell it to save scene and player separately
		MySaver.Save();
		// unity load saved editor scene file
		entryID = toEntryNumber;
		SceneManager.LoadScene(toSceneName);
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

		if (entryID == 99){
			Bed bed = GameObject.FindObjectOfType<Bed>();
			if (bed){
				bed.SleepCutscene();
				playerObject.SetActive(false);
			}
		}
	}
	void PlayerEnter(){
		if (playerObject){
			List<Doorway> doorways = new List<Doorway>( GameObject.FindObjectsOfType<Doorway>() );
			// TODO: can probably make this nicer with LINQ
			foreach (Doorway doorway in doorways){
				if (doorway.entryID == entryID){
					Vector3 tempPos = doorway.transform.position;
					tempPos.y = tempPos.y - 0.05f;
					playerObject.transform.position = tempPos;
				}
			}
		}
	}

	public void NewDayCutscene(){
		MySaver.Save();
		
		data.days += 1;
		SceneManager.LoadScene("morning_cutscene");
        sceneTime = 0f;
        entryID = 99;
	}
    public void NewDay(){
        MySaver.CleanupSaves();
		SceneManager.LoadScene("house");
		// Bed bed = GameObject.FindObjectOfType<Bed>();
		// bed.SleepCutscene();
        sceneTime = 0f;
        entryID = 99;
    }

	// public void OnLevelWasLoaded(int level){

	// }
    public void NewGame(bool switchlevel=true){
        if (switchlevel){
			// SceneManager.LoadScene("house");
			NewDayCutscene();
        }
        sceneTime = 0f;
		timePlayed = 0f;
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
		itemCheckedOut = new Dictionary<string, bool>();

		// TODO: change this temporary hack into something more correct.
        unlockedCommercials = new List<Commercial>();
        unlockedCommercials.Add(LoadCommercialByName("eat1"));
        completeCommercials = new List<Commercial>();
		
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
		data = new GameData();
		GameData loadData = LoadGameData(gameName);
		data.collectedItems = new List<string>();
		itemCheckedOut = new Dictionary<string, bool>();
		if (data == null){
			// InitValues();
			NewGame();
		} else {
			data.collectedItems = loadData.collectedItems;
			foreach(string item in data.collectedItems){
				itemCheckedOut[item] = false;
			}
			data.lastSavedPlayerPath = loadData.lastSavedPlayerPath;
			data.lastSavedScenePath = loadData.lastSavedScenePath;
			data.lastPlayerName = loadData.lastPlayerName;
			timePlayed = loadData.secondsPlayed;
			timeSinceLastSave = 0f;
            data.money = loadData.money;
			if (loadData.lastScene != null){
				SceneManager.LoadScene("house");
			} else {
				SceneManager.LoadScene("house");
			}
		}
	}

}

