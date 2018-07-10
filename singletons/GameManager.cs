﻿using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

// [XmlRoot("GameData")]
[System.Serializable]
public class GameData {
    public float money;
    public List<string> collectedObjects;
    public List<string> collectedItems;
    public List<string> newCollectedItems;
    public List<string> collectedFood;
    public List<string> newCollectedFood;
    public List<string> collectedClothes;
    public List<string> newCollectedClothes;
    public int itemsCollectedToday;
    public int clothesCollectedToday;
    public int foodCollectedToday;
    public SerializableDictionary<string, bool> itemCheckedOut;
    public SerializableDictionary<string, bool> perks;
    public string lastSavedPlayerPath;
    public string lastSavedScenePath;
    public string saveDate;
    public float secondsPlayed;
    public string lastScene;
    public int days;
    public int deaths;
    public int deathCutscenesPlayed;
    public HashSet<Commercial> unlockedCommercials;
    public HashSet<Commercial> completeCommercials;
    public HashSet<Commercial> newUnlockedCommercials;
    public HashSet<string> unlockedScenes;
    public int entryID;
    public List<Achievement> achievements;
    public AchievementStats achievementStats = new AchievementStats();
    public List<Email> emails;
    public List<string> packages;
    public bool firstTimeLeavingHouse;
    public bool mayorCutsceneHappened;
    public bool teleporterUnlocked;
    public GameData() {
        days = 0;
        saveDate = System.DateTime.Now.ToString();
    }
}
public partial class GameManager : Singleton<GameManager> {
    protected GameManager() { }
    static public Dictionary<string, string> sceneNames = new Dictionary<string, string>(){
        {"cave1", "deathtrap cave"},
        {"cave2", "deathtrap cave II"},
        {"forest", "forest"},
        {"house", "house"},
        {"krazy1", "outdoors"},
        {"moon1", "moon"},
        {"studio", "yogurt commercial studio"},
        {"volcano", "volcano"},
        {"room2", "item room"},
        {"moon_cave", "moon cave"},
        {"woods", "woods"},
        {"vampire_house", "mansion"}
    };
    public GameData data;
    public string saveGameName = "test";
    private CameraControl cameraControl;
    public Camera cam;
    public GameObject playerObject;
    public float gravity = 3.0f;
    private Commercial _activeCommercial;
    public Commercial activeCommercial {
        get { return _activeCommercial; }
        set {
            _activeCommercial = value;
        }
    }
    public float sceneTime;
    private bool awaitNewDayPrompt;
    public float timeSinceLastSave = 0f;
    private float intervalTimer;
    public Dictionary<HomeCloset.ClosetType, bool> closetHasNew = new Dictionary<HomeCloset.ClosetType, bool>();
    public AudioSource publicAudio;
    public bool playerIsDead;
    public bool debug = true;
    void Start() {
        if (data == null) {
            data = InitializedGameData();
            // ReceiveEmail("duplicator");
            // ReceivePackage("duplicator");
            if (debug)
                data.mayorCutsceneHappened = true;
        }
        if (saveGameName == "test")
            MySaver.CleanupSaves();
        if (!InCutsceneLevel()) {
            NewGame(switchlevel: false);
        }
        publicAudio = Toolbox.Instance.SetUpAudioSource(gameObject);
        SceneManager.sceneLoaded += LevelWasLoaded;
        // these bits are for debug!
        if (SceneManager.GetActiveScene().name == "boardroom_cutscene") {
            CutsceneManager.Instance.InitializeCutscene<CutsceneBoardroom>();
            CutsceneManager.Instance.cutscene.Configure();
        }
    }
    void Update() {
        timeSinceLastSave += Time.deltaTime;
        intervalTimer += Time.deltaTime;
        if (!InCutsceneLevel()) {
            if (CutsceneManager.Instance.cutscene == null) {
                sceneTime += Time.deltaTime;
            }
        }
        if (intervalTimer > 3f) {
            data.achievementStats.secondsPlayed = timeSinceLastSave;
            CheckAchievements();
            intervalTimer = 0;
        }
        if (awaitNewDayPrompt && sceneTime > 2f) {
            awaitNewDayPrompt = false;
            UINew.Instance.ShowMenu(UINew.MenuType.newDayReport);
        }
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneTime > 0.1f && !data.unlockedScenes.Contains(sceneName) && !InCutsceneLevel()) {
            data.unlockedScenes.Add(sceneName);
            UINew.Instance.ShowSceneText("- " + GameManager.sceneNames[sceneName] + " -");
        }
    }
    public bool InCutsceneLevel() {
        if (SceneManager.GetActiveScene().buildIndex > 2) {
            return false;
        } else {
            return true;
        }
    }
    public void FocusIntrinsicsChanged(Intrinsics intrinsics) {
        Dictionary<BuffType, Buff> netBuffs = intrinsics.NetBuffs();
        if (netBuffs[BuffType.telepathy].boolValue) {
            cam.cullingMask |= 1 << LayerMask.NameToLayer("thoughts");
        } else {
            try {
                cam.cullingMask &= ~(1 << LayerMask.NameToLayer("thoughts"));
            }
            catch {
                Debug.Log(cam);
                Debug.Log("Weird telepathy culling mask issue");
            }
        }
        UINew.Instance.ClearStatusIcons();
        foreach (Buff buff in intrinsics.AllBuffs()) {
            if (buff.boolValue == true || buff.floatValue > 0) {
                UINew.Instance.AddStatusIcon(buff);
            }
        }
    }
    public void SetFocus(GameObject target) {
        if (playerObject != null) {
            Controllable oldControl = playerObject.GetComponent<Controllable>();
            oldControl.control = Controllable.ControlType.AI;
        }
        Toolbox.Instance.SwitchAudioListener(target);
        playerObject = target;
        Controllable targetControl = playerObject.GetComponent<Controllable>();
        Controller.Instance.focus = target.GetComponent<Controllable>();
        targetControl.control = Controllable.ControlType.player;
        cameraControl = FindObjectOfType<CameraControl>();
        if (cameraControl)
            cameraControl.focus = target;
        Intrinsics intrinsics = Toolbox.GetOrCreateComponent<Intrinsics>(target);
        FocusIntrinsicsChanged(intrinsics);
        // check collections for new focus outfit, holding, and hat
        Outfit playerOutfit = target.GetComponent<Outfit>();
        if (playerOutfit) {
            string prefabName = playerOutfit.wornUniformName;
            GameObject uniform = Instantiate(Resources.Load("prefabs/" + prefabName)) as GameObject;
            CheckItemCollection(uniform, playerObject);
            DestroyImmediate(uniform);
        }
        Head playerHead = target.GetComponentInChildren<Head>();
        if (playerHead) {
            if (playerHead.hat != null)
                CheckItemCollection(playerHead.hat.gameObject, playerObject);
        }
        Inventory playerInv = target.GetComponent<Inventory>();
        if (playerInv) {
            if (playerInv.holding)
                CheckItemCollection(playerInv.holding.gameObject, playerObject);
        }
        // refresh UI
        UINew.Instance.RefreshUI(active:true);
    }
    public void LeaveScene(string toSceneName, int toEntryNumber) {
        MySaver.Save();
        data.entryID = toEntryNumber;
        SceneManager.LoadScene(toSceneName);
    }
    void LevelWasLoaded(Scene scene, LoadSceneMode mode) {
        // Debug.Log("on level was loaded");
        Toolbox.Instance.numberOfLiveSpeakers = 0;
        sceneTime = 0f;
        if (InCutsceneLevel()) {
            InitializeNonPlayableLevel();
        } else {
            InitializePlayableLevel(loadLevel: true);
        }
    }
    public void InitializePlayableLevel(bool loadLevel = false) {
        // make sure all required parts are in place
        string[] requirements = new string[] { "Main Camera", "EventSystem", "NeoUICanvas" };
        foreach (string requirement in requirements) {
            GameObject go = GameObject.Find(requirement);
            if (!go) {
                string path = @"required/" + requirement;
                GameObject newgo = GameObject.Instantiate(Resources.Load(path)) as GameObject;
                newgo.name = Toolbox.Instance.CloneRemover(newgo.name);
            }
        }
        cam = GameObject.FindObjectOfType<Camera>();
        if (cam) {
            Toolbox.GetOrCreateComponent<CameraControl>(cam.gameObject);
        }
        UINew.Instance.ConfigureUIElements();
        if (loadLevel) {
            playerObject = MySaver.LoadScene();
        } else {
            if (data == null)
                data = InitializedGameData();
            // find or spawn the player character 
            playerObject = GameObject.Find("Tom");
            if (!playerObject) {
                playerObject = GameObject.Find("Tom(Clone)");
            }
            if (!playerObject) {
                playerObject = GameObject.Instantiate(Resources.Load("prefabs/Tom")) as GameObject;
            }
            data.entryID = 99;
        }
        SetFocus(playerObject);
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "krazy1") {
            GameObject packageSpawnPoint = GameObject.Find("packageSpawnPoint");
            if (data.firstTimeLeavingHouse) {
                foreach (string package in data.packages) {
                    if (!data.collectedItems.Contains(package)) {
                        GameObject packageObject = Instantiate(Resources.Load("prefabs/package"), packageSpawnPoint.transform.position, Quaternion.identity) as GameObject;
                        Package pack = packageObject.GetComponent<Package>();
                        pack.contents = package;
                    }
                }
            }
            if (!data.mayorCutsceneHappened) {
                // CutsceneManager.Instance.InitializeCutscene(CutsceneManager.CutsceneType.mayorTalk);
                CutsceneManager.Instance.InitializeCutscene<CutsceneMayor>();
                data.mayorCutsceneHappened = true;
                data.entryID = 0;
            }
            data.firstTimeLeavingHouse = false;
        }
        if (sceneName == "cave1" || sceneName == "cave2") {
            // CutsceneManager.Instance.InitializeCutscene(CutsceneManager.CutsceneType.fall);
            CutsceneManager.Instance.InitializeCutscene<CutsceneFall>();
        }
        if (sceneName == "space") {
            // CutsceneManager.Instance.InitializeCutscene(CutsceneManager.CutsceneType.space);
            CutsceneManager.Instance.InitializeCutscene<CutsceneSpace>();
        }
        if (sceneName == "moon1" && (data.entryID == 420 || data.entryID == 99)) {
            // CutsceneManager.Instance.InitializeCutscene(CutsceneManager.CutsceneType.moonLanding);
            CutsceneManager.Instance.InitializeCutscene<CutsceneMoonLanding>();
        }
        if (sceneName == "house" && !data.teleporterUnlocked) {
            GameObject.FindObjectOfType<Teleporter>().gameObject.SetActive(false);
        }
        PlayerEnter();
    }
    public void InitializeNonPlayableLevel() {
        string sceneName = SceneManager.GetActiveScene().name;
        UINew.Instance.RefreshUI();
        Toolbox.Instance.SwitchAudioListener(GameObject.Find("Main Camera"));
        if (sceneName == "morning_cutscene") {
            // CutsceneManager.Instance.InitializeCutscene(CutsceneManager.CutsceneType.newDay);
            CutsceneManager.Instance.InitializeCutscene<CutsceneNewDay>();
        }
        if (sceneName == "boardroom_cutscene") {
            // CutsceneManager.Instance.InitializeCutscene(CutsceneManager.CutsceneType.boardRoom);
            CutsceneManager.Instance.InitializeCutscene<CutsceneBoardroom>();
        }
    }
    void PlayerEnter() {
        if (playerObject == null)
            return;
        List<Doorway> doorways = new List<Doorway>(GameObject.FindObjectsOfType<Doorway>());
        // TODO: can probably make this nicer with LINQ
        foreach (Doorway doorway in doorways) {
            if ((doorway.entryID == data.entryID && !doorway.spawnPoint) || (doorway.spawnPoint && data.entryID == 99)) {
                doorway.Enter(playerObject);
                // if this is a bed entry, we've got a new day going on!
                if (data.entryID == -99) {
                    WakeUpInBed();
                }
            }
        }
    }
    void WakeUpInBed() {
        Bed bed = GameObject.FindObjectOfType<Bed>();
        if (bed) {
            playerObject.SetActive(false);
            Outfit outfit = playerObject.GetComponent<Outfit>();
            if (outfit != null) {
                outfit.initUniform = Resources.Load("prefabs/pajamas") as GameObject;
            }
            Inventory focusInv = playerObject.GetComponent<Inventory>();
            if (focusInv) {
                focusInv.ClearInventory();
                UINew.Instance.UpdateInventoryButton(focusInv);
            }
            Eater focusEater = playerObject.GetComponent<Eater>();
            if (focusEater) {
                focusEater.nutrition = 0;
                focusEater.nausea = 0;
            }
            Hurtable playerHurtable = playerObject.GetComponent<Hurtable>();
            if (playerHurtable) {
                playerHurtable.health = playerHurtable.maxHealth;
                playerHurtable.oxygen = playerHurtable.maxOxygen;
                // TODO: reset hitstate ?
            }
            MySaver.Save();
            awaitNewDayPrompt = CheckNewDayPrompt();
            bed.SleepCutscene();
        }
    }

    public bool CheckNewDayPrompt() {
        if (data.days <= 1)
            return false;
        // return data.newCollectedClothes.Count + data.newCollectedFood.Count + data.newCollectedItems.Count + data.newUnlockedCommercials.Count > 0;
        return data.itemsCollectedToday + data.foodCollectedToday + data.clothesCollectedToday + data.newUnlockedCommercials.Count > 0;
    }

    public void NewGame(bool switchlevel = true) {
        Debug.Log("New game");
        if (data == null)
            data = InitializedGameData();
        if (switchlevel) {
            NewDayCutscene();
        } else {
            InitializePlayableLevel();
        }
        sceneTime = 0f;
        timeSinceLastSave = 0f;
    }
    public void NewDay() {
        Debug.Log("New day");
        MySaver.CleanupSaves();
        MySaver.SaveObjectDatabase();
        List<string> keys = new List<string>(data.itemCheckedOut.Keys);
        foreach (string key in keys) {
            data.itemCheckedOut[key] = false;
        }
        DetermineClosetNews();
        SceneManager.LoadScene("house");
        sceneTime = 0f;
        data.entryID = -99;
        data.firstTimeLeavingHouse = true;
        activeCommercial = null;
    }
    public void DetermineClosetNews() {
        closetHasNew[HomeCloset.ClosetType.items] = false;
        closetHasNew[HomeCloset.ClosetType.all] = false;
        closetHasNew[HomeCloset.ClosetType.food] = false;
        closetHasNew[HomeCloset.ClosetType.clothing] = false;
        foreach (string name in data.newCollectedItems) {
            if (!data.itemCheckedOut[name]) {
                closetHasNew[HomeCloset.ClosetType.items] = true;
                closetHasNew[HomeCloset.ClosetType.all] = true;
            }
        }
        foreach (string name in data.newCollectedFood) {
            if (!data.itemCheckedOut[name]) {
                closetHasNew[HomeCloset.ClosetType.food] = true;
            }
        }
        foreach (string name in data.newCollectedClothes) {
            if (!data.itemCheckedOut[name]) {
                closetHasNew[HomeCloset.ClosetType.clothing] = true;
            }
        }
    }
    public void NewDayCutscene() {
        playerIsDead = false;
        data.days += 1;
        SceneManager.LoadScene("morning_cutscene");
        sceneTime = 0f;
        data.entryID = -99;
    }
    public void BoardRoomCutscene() {
        SceneManager.LoadScene("boardroom_cutscene");
        sceneTime = 0f;
        data.entryID = -99;
    }
    public void TitleScreen() {
        SceneManager.LoadScene("title");
    }
    public GameData InitializedGameData() {
        GameData data = new GameData();
        data.secondsPlayed = 0f;
        data.collectedItems = new List<string>();
        data.newCollectedItems = new List<string>();
        data.collectedObjects = new List<string>();
        data.collectedFood = new List<string>();
        data.newCollectedFood = new List<string>();
        data.collectedClothes = new List<string>();
        data.newCollectedClothes = new List<string>();
        data.packages = new List<string>();
        data.emails = new List<Email>();
        data.itemCheckedOut = new SerializableDictionary<string, bool>();
        data.perks = new SerializableDictionary<string, bool>(){
            {"vomit", false},
            {"eat_all", false},
            {"hypnosis", false}
        };
        data.collectedClothes.Add("blue_shirt");
        data.collectedObjects.Add("blue_shirt");
        data.itemCheckedOut["blue_shirt"] = false;
        data.collectedClothes.Add("pajamas");
        data.collectedObjects.Add("pajamas");
        data.itemCheckedOut["pajamas"] = false;
        data.firstTimeLeavingHouse = true;
        // initialize commercials
        // TODO: change this temporary hack into something more correct.
        data.unlockedCommercials = new HashSet<Commercial>();
        data.newUnlockedCommercials = new HashSet<Commercial>();
        data.unlockedScenes = new HashSet<string>();
        data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("eat1"));
        if (debug) {
            data.days = 5;
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("eat2"));
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("eggplant1"));
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("eggplant10"));
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("fireman"));
            data.perks["hypnosis"] = true;
            data.unlockedScenes.Add("moon_cave");
            data.teleporterUnlocked = true;
        }
        data.completeCommercials = new HashSet<Commercial>();
        // initialize achievements
        data.achievements = new List<Achievement>();
        GameObject[] achievementPrefabs = Resources.LoadAll("achievements/", typeof(GameObject))
            .Cast<GameObject>()
            .ToArray();
        foreach (GameObject prefab in achievementPrefabs) {
            if (debug && prefab.name == "StartGame")
                continue;
            AchievementComponent component = prefab.GetComponent<AchievementComponent>();
            if (component) {
                Achievement cloneAchievement = new Achievement(component.achivement);
                data.achievements.Add(cloneAchievement);
            }
        }
        return data;
    }

    // SAVING AND LOADING
    public string ObjectsSavePath() {
        string path = "";
        path = Path.Combine(Application.persistentDataPath, saveGameName);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        path = Path.Combine(path, saveGameName + ".xml");
        data.lastSavedScenePath = path;
        return path;
    }
    public string LevelSavePath() {
        string path = "";
        path = Path.Combine(Application.persistentDataPath, saveGameName);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        path = Path.Combine(path, SceneManager.GetActiveScene().name + "_state.xml");
        data.lastSavedScenePath = path;
        return path;
    }
    public string PlayerSavePath() {
        string path = "";
        path = Path.Combine(Application.persistentDataPath, saveGameName);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        string playerName = Toolbox.Instance.GetName(GameManager.Instance.playerObject);
        path = Path.Combine(path, "player_" + playerName + "_state.xml");
        data.lastSavedPlayerPath = path;
        // data.lastPlayerName = GameManager.Instance.playerObject.name;
        return path;
    }
    public void SaveGameData() {
        data.secondsPlayed += timeSinceLastSave;
        data.lastScene = SceneManager.GetActiveScene().name;

        var serializer = new XmlSerializer(typeof(GameData));
        string path = Path.Combine(Application.persistentDataPath, saveGameName);
        path = Path.Combine(path, "gameData.xml");
        FileStream sceneStream = File.Create(path);
        try{    
            serializer.Serialize(sceneStream, data);
        } catch(Exception e) {
            Debug.Log(e.ToString());
            Debug.Log(e.Source);
            Debug.Log(e.StackTrace);
            Debug.Log(e.Message);
        }
        sceneStream.Close();
        timeSinceLastSave = 0f;
    }
    public GameData LoadGameData(string gameName) {
        GameData data = null;
        var serializer = new XmlSerializer(typeof(GameData));
        string path = Path.Combine(Application.persistentDataPath, gameName);
        path = Path.Combine(path, "gameData.xml");
        if (File.Exists(path)) {
            try {
                var dataStream = new FileStream(path, FileMode.Open);
                data = serializer.Deserialize(dataStream) as GameData;
                dataStream.Close();
            }
            catch (Exception e) {
                Debug.Log("Error loading game data: " + path);
                Debug.Log(e.Message);
                Debug.Log(e.TargetSite);
            }
        }
        return data;
    }
    public void LoadGameDataIntoMemory(string gameName) {
        // Debug.Log("Loadsavegame into memory");
        data = LoadGameData(gameName);
        if (data.lastScene != null) {
            SceneManager.LoadScene(data.lastScene);
        } else {
            SceneManager.LoadScene("house");
        }
    }
    public void CheckItemCollection(GameObject obj, GameObject owner) {
        if (owner != playerObject)
            return;
        string filename = Toolbox.Instance.CloneRemover(obj.name);
        // filename = Toolbox.Instance.ReplaceUnderscore(filename);
        if (data.collectedObjects.Contains(filename))
            return;
        UnityEngine.Object testPrefab = Resources.Load("prefabs/" + filename);
        if (testPrefab != null) {
            data.collectedObjects.Add(filename);
            data.itemCheckedOut[filename] = true;
            UINew.Instance.PopupCollected(obj);
            Edible objectEdible = obj.GetComponent<Edible>();
            if (objectEdible != null) {
                if (!objectEdible.inedible) {
                    data.collectedFood.Add(filename);
                    data.newCollectedFood.Add(filename);
                    data.foodCollectedToday += 1;
                }
            }
            if (obj.GetComponent<Uniform>() || obj.GetComponent<Hat>()) {
                data.collectedClothes.Add(filename);
                data.newCollectedClothes.Add(filename);
                data.clothesCollectedToday += 1;
            }
            if (obj.GetComponent<Pickup>()) {
                data.collectedItems.Add(filename);
                data.newCollectedItems.Add(filename);
                data.itemsCollectedToday += 1;
            }
        }
    }
    public bool IsItemCollected(GameObject obj) {
        string filename = Toolbox.Instance.CloneRemover(obj.name);
        // filename = Toolbox.Instance.ReplaceUnderscore(filename);
        return data.collectedObjects.Contains(filename);
    }
    public void RetrieveCollectedItem(string name) {
        if (data.itemCheckedOut[name])
            return;
        Instantiate(Resources.Load("prefabs/" + name), playerObject.transform.position, Quaternion.identity);
        Instantiate(Resources.Load("particles/poof"), playerObject.transform.position, Quaternion.identity);
        publicAudio.PlayOneShot(Resources.Load("sounds/pop", typeof(AudioClip)) as AudioClip);
        data.itemCheckedOut[name] = true;
    }
    public void CheckAchievements() {
        if (InCutsceneLevel())
            return;
        foreach (Achievement achieve in data.achievements) {
            if (!achieve.complete) {
                bool pass = achieve.Evaluate(data.achievementStats);
                if (pass) {
                    achieve.complete = true;
                    UINew.Instance.PopupAchievement(achieve);
                }
            }
        }
    }
    public void ReceiveEmail(string emailName) {
        // Debug.Log("receiving email "+emailName);
        foreach (Email email in data.emails) {
            Debug.Log(email.filename);
            if (email.filename == emailName) {
                // Debug.Log("already received email. aborting...");
                return;
            }
        }
        Email newEmail = Email.LoadEmail(emailName);
        newEmail.read = false;
        data.emails.Add(newEmail);
        Computer computer = GameObject.FindObjectOfType<Computer>();
        if (computer != null) {
            computer.CheckBubble();
        }
    }
    public void ReceivePackage(string packageName) {
        if (data.packages.Contains(packageName)) {
            Debug.Log("already recieved package. aborting...");
        }
        data.packages.Add(packageName);
    }
    public void ShowDiaryEntry(string diaryName) {
        GameObject diaryObject = UINew.Instance.activeMenu;
        if (UINew.Instance.activeMenu == null){
            diaryObject = UINew.Instance.ShowMenu(UINew.MenuType.diary);
        } 
        Diary diary = diaryObject.GetComponent<Diary>();
        diary.loadDiaryName = diaryName;
    }
    public void PlayerDeath() {
        if (playerIsDead)
            return;
        playerIsDead = true;
        data.deaths += 1;
        // MySaver.Save();
        UINew.Instance.RefreshUI(active: false);
        Instantiate(Resources.Load("UI/deathMenu"));
        CameraControl camControl = FindObjectOfType<CameraControl>();
        camControl.audioSource.PlayOneShot(Resources.Load("sounds/xylophone/x4") as AudioClip);
        Toolbox.Instance.SwitchAudioListener(GameObject.Find("Main Camera"));
    }
}

