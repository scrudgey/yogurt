using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

[System.Serializable]
public class GameData {
    public string prefabName;
    public Gender defaultGender;
    public SkinColor defaultSkinColor;
    public float money;
    public List<string> collectedObjects;
    public List<string> collectedItems;
    public List<string> newCollectedItems;
    public List<string> collectedFood;
    public List<string> newCollectedFood;
    public List<string> collectedClothes;
    public List<string> newCollectedClothes;
    public int[] collectedChelas = new int[10];
    public int itemsCollectedToday;
    public int clothesCollectedToday;
    public int foodCollectedToday;
    public bool teleportedToday;
    public SerializableDictionary<string, bool> itemCheckedOut;
    public SerializableDictionary<string, bool> perks;
    public string lastSavedPlayerPath;
    public string lastSavedScenePath;
    public string saveDate;
    public System.DateTime saveDateTime;
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
    public SerializableDictionary<StatType, Stat> stats = new SerializableDictionary<StatType, Stat>();
    public List<Email> emails;
    public List<string> packages;
    public bool firstTimeLeavingHouse;
    public bool mayorCutsceneHappened;
    public bool visitedStudio;
    public bool finishedCommercial;
    public bool visitedMoon;
    public bool teleporterUnlocked;
    public string headSpriteSheet;
    public SkinColor headSkinColor;
    public string cosmicName = "";
    public bool loadedDay;
    public Int16 ghostsKilled;
    public bool mayorAwardToday;
    public bool mayorLibraryShuffled;
    public int gangMembersDefeated;

    public Commercial activeCommercial;
    public bool recordingCommercial;

    public List<System.Guid> toiletItems = new List<System.Guid>();
    public GameData() {
        days = 0;
        saveDate = System.DateTime.Now.ToString();
        saveDateTime = System.DateTime.Now;
    }
    public List<Achievement> CheckAchievements() {
        List<Achievement> completeAchievements = new List<Achievement>();
        foreach (Achievement achieve in achievements) {
            if (!achieve.complete) {
                bool pass = achieve.Evaluate(stats);
                if (pass) {
                    achieve.complete = true;
                    achieve.completedTime = System.DateTime.Now;
                    completeAchievements.Add(achieve);
                }
            }
        }
        return completeAchievements;
    }
}
public class GlobalSettings {
    private bool _musicOn = true;
    public bool musicOn {
        get { return _musicOn; }
        set {
            _musicOn = value;
            if (value) {
                MusicController.Instance.SetMusic(MusicController.Instance.nowPlayingMusic);
            } else {
                MusicController.Instance.StopTrack();
            }
        }
    }
    public float sfxVolume;
    public float musicVolume;
}
public partial class GameManager : Singleton<GameManager> {
    protected GameManager() { }
    static public Dictionary<string, string> sceneNames = new Dictionary<string, string>(){
        {"cave1", "deathtrap cave"},
        {"cave2", "deathtrap cave II"},
        {"forest", "forest"},
        {"house", "house"},
        {"moon1", "moon"},
        {"studio", "yogurt commercial studio"},
        {"volcano", "volcano"},
        {"room2", "item room"},
        {"moon_cave", "moon temple"},
        {"woods", "woods"},
        {"vampire_house", "mansion"},
        {"mountain", "mountain"},
        {"clearing", "clearing"},
        {"chamber", "meditation chamber"},
        {"dungeon", "oubliette"},
        {"potion", "apothecary"},
        {"cave3", "sewer"},
        {"moon_pool", "moon pool"},
        {"moon_town", "moon town"},
        {"neighborhood", "outdoors"},
        {"mayors_house", "mayor's house"},
        {"mayors_attic", "attic"},
        {"anti_mayors_house", "anti mayor's house"},
        {"tower", "tower"},
        {"cave4", "tomb"},
        {"apartment", "apartment"},
        {"boardroom", "yogurt HQ"},
    };
    public GameData data;
    public static GlobalSettings settings = new GlobalSettings();
    public string saveGameName = "test";
    private CameraControl cameraControl;
    public Camera cam;
    public GameObject playerObject;
    public float gravity = 3.0f;
    public float sceneTime;
    private bool awaitNewDayPrompt;
    public float timeSinceLastSave = 0f;
    private float intervalTimer;
    public Dictionary<HomeCloset.ClosetType, bool> closetHasNew = new Dictionary<HomeCloset.ClosetType, bool>();
    public AudioSource publicAudio;
    public bool playerIsDead;
    public bool debug = true;
    public bool failedLevelLoad = false;
    public Gender playerGender;

    public delegate void BooleanObserver(bool value);
    public static BooleanObserver onRecordingChange;

    public void PlayPublicSound(AudioClip clip) {
        if (clip == null)
            return;
        if (publicAudio == null) {
            cam = GameObject.FindObjectOfType<Camera>();
            publicAudio = Toolbox.GetOrCreateComponent<AudioSource>(gameObject);
            if (cam) {
                Toolbox.GetOrCreateComponent<CameraControl>(cam.gameObject);
            }
        }
        publicAudio.PlayOneShot(clip);
    }
    public void Start() {
        if (data == null) {
            data = InitializedGameData();
            // ReceiveEmail("duplicator");
            // ReceiveEmail("golf_club");
            // ReceivePackage("kaiser_helmet");
            // ReceivePackage("duplicator");
            // ReceivePackage("golf_club");
        }
        if (saveGameName == "test")
            MySaver.CleanupSaves();
        if (!InCutsceneLevel()) {
            NewGame(switchlevel: false);
        } else {
            InitializeNonPlayableLevel();
        }
        SceneManager.sceneLoaded += SceneWasLoaded;
        // these bits are for debug!
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "boardroom_cutscene") {
            CutsceneManager.Instance.InitializeCutscene<CutsceneBoardroom>();
            CutsceneManager.Instance.cutscene.Configure();
        }
        if (scene.name == "anti_mayor_cutscene") {
            CutsceneManager.Instance.InitializeCutscene<CutsceneAntiMayor>();
            CutsceneManager.Instance.cutscene.Configure();
        }

        MusicController.Instance.SceneChange(scene.name);
    }
    void Update() {
        if (data == null)
            return;
        string sceneName = SceneManager.GetActiveScene().name;
        timeSinceLastSave += Time.deltaTime;
        intervalTimer += Time.deltaTime;
        if (!InCutsceneLevel()) {
            if (CutsceneManager.Instance.cutscene == null) {
                sceneTime += Time.deltaTime;
            }
        }
        if (intervalTimer > 3f) {
            SetStat(StatType.secondsPlayed, data.secondsPlayed);
            intervalTimer = 0;
        }
        if (awaitNewDayPrompt && sceneTime > 2f) {
            awaitNewDayPrompt = false;
            UINew.Instance.ShowMenu(UINew.MenuType.newDayReport);
        }
        if (sceneTime > 0.1f && !data.unlockedScenes.Contains(sceneName) && !InCutsceneLevel() && GameManager.sceneNames.ContainsKey(sceneName)) {
            data.unlockedScenes.Add(sceneName);
            UINew.Instance.ShowSceneText("- " + GameManager.sceneNames[sceneName] + " -");
        }
    }
    public void ExplodeHead() {
        Head head = playerObject.GetComponentInChildren<Head>();
        if (head != null) {
            AudioClip boom = Resources.Load("sounds/explosion/cannon") as AudioClip;
            PlayPublicSound(boom);

            GameObject headGibs = Resources.Load("prefabs/gibs/headGibsContainer") as GameObject;
            MessageDamage message = new MessageDamage(100f, damageType.physical);
            Vector2 rand = UnityEngine.Random.insideUnitCircle;
            message.force = new Vector3(rand.x, rand.y, 2f).normalized;
            foreach (Gibs gib in headGibs.GetComponents<Gibs>()) {
                Gibs newGib = head.gameObject.AddComponent<Gibs>();
                newGib.CopyFrom(gib);
                newGib.Emit(message);
            }

            Destroy(head.gameObject);
            Hurtable hurtable = playerObject.GetComponent<Hurtable>();
            hurtable.Die(damageType.physical);
        }
    }
    public bool InCutsceneLevel() {
        return SceneManager.GetActiveScene().buildIndex <= 3;
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
        foreach (KeyValuePair<BuffType, Buff> buff in intrinsics.NetBuffs()) {
            if (buff.Value.active()) {
                UINew.Instance.AddStatusIcon(buff.Value);
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
            playerGender = playerOutfit.gender;
            string prefabName = playerOutfit.wornUniformName;
            if (prefabName != "nude" && prefabName != "nude_female") {
                GameObject uniform = Instantiate(Resources.Load("prefabs/" + prefabName)) as GameObject;
                CheckItemCollection(uniform, playerObject);
                DestroyImmediate(uniform);
            } else {
                playerOutfit.GoNude();
            }
        }
        Head playerHead = target.GetComponentInChildren<Head>();
        if (playerHead) {
            if (playerHead.hat != null)
                CheckItemCollection(playerHead.hat.gameObject, playerObject);
            HeadAnimation headAnim = playerHead.GetComponent<HeadAnimation>();
            if (headAnim) {
                data.headSpriteSheet = headAnim.spriteSheet;
                data.headSkinColor = headAnim.skinColor;
            }
        }
        Inventory playerInv = target.GetComponent<Inventory>();
        if (playerInv) {
            if (playerInv.holding)
                CheckItemCollection(playerInv.holding.gameObject, playerObject);
        }
        // refresh UI
        UINew.Instance.RefreshUI(active: true);
    }
    public void LeaveScene(string toSceneName, int toEntryNumber) {
        MySaver.Save();
        data.entryID = toEntryNumber;
        SceneManager.LoadScene(toSceneName);
    }
    void SceneWasLoaded(Scene scene, LoadSceneMode mode) {
        // Debug.Log("on level was loaded");
        Toolbox.Instance.numberOfLiveSpeakers = 0;
        publicAudio.Stop();
        sceneTime = 0f;
        if (InCutsceneLevel()) {
            InitializeNonPlayableLevel();
        } else {
            try {
                InitializePlayableLevel(loadLevel: true);
                failedLevelLoad = false;
            }
            catch (Exception e) {
                if (failedLevelLoad) {
                    Debug.Break();
                }
                // TODO: default / corrupt save recovery
                Debug.Log("Load failed, defaulting to new day");
                Debug.LogException(e);
                failedLevelLoad = true;
                ResetGameState();
                return;
            }
        }

        MusicController.Instance.SceneChange(scene.name);
    }
    void ResetGameState() {
        try {
            FileInfo playerFile = new FileInfo(GameManager.Instance.data.lastSavedPlayerPath);
            if (playerFile.Exists) {
                playerFile.Delete();
            }
            MySaver.CleanupSaves();

            string housePath = Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName);
            housePath = Path.Combine(housePath, "apartment_state.xml");
            string[] paths = new string[] { housePath, GameManager.Instance.data.lastSavedPlayerPath };

            foreach (string path in paths) {
                if (!System.IO.File.Exists(path))
                    return;
                FileInfo info = new FileInfo(path);
                File.Delete(info.FullName);
            }
            NewGame();
        }
        catch (Exception e) {
            Debug.LogError("reset game state failed");
            Debug.LogError(e.Message);
            Debug.LogError(e.StackTrace);
        }
    }
    public void InitializePlayableLevel(bool loadLevel = false) {
        string sceneName = SceneManager.GetActiveScene().name;

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
        publicAudio = Toolbox.GetOrCreateComponent<AudioSource>(gameObject);
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
            playerObject = GameObject.Find(data.prefabName);
            if (!playerObject) {
                playerObject = GameObject.Find(data.prefabName + "(Clone)");
            }
            if (!playerObject) {
                playerObject = InstantiatePlayerPrefab();
            }
            data.entryID = 99;
        }

        SetFocus(playerObject);
        Controller.Instance.state = Controller.ControlState.normal;
        UINew.Instance.RefreshUI(active: true);

        if (sceneName == "neighborhood") {
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
                CutsceneManager.Instance.InitializeCutscene<CutsceneMayor>();
                data.mayorCutsceneHappened = true;
                data.entryID = 0;
            }
            data.firstTimeLeavingHouse = false;
        }
        if (sceneName == "studio" && !data.visitedStudio) {
            data.visitedStudio = true;
            VideoCamera videoCamera = GameObject.FindObjectOfType<VideoCamera>();
            if (videoCamera != null) {
                CameraTutorialText ctt = videoCamera.GetComponent<CameraTutorialText>();
                if (ctt != null)
                    ctt.Enable();
            }
            ShowDiaryEntry("diaryStudio");
        }
        if (sceneName == "cave1" || sceneName == "cave2" || (sceneName == "cave3" && data.entryID == 3) || sceneName == "cave4") {
            CutsceneManager.Instance.InitializeCutscene<CutsceneFall>();
        }
        if (sceneName == "dungeon" && data.entryID != 2) {
            CutsceneManager.Instance.InitializeCutscene<CutsceneDungeonFall>();
        }
        if (sceneName == "space") {
            CutsceneManager.Instance.InitializeCutscene<CutsceneSpace>();
        }
        if (sceneName == "moon1" && (data.entryID == 420 || data.entryID == 99)) {
            CutsceneManager.Instance.InitializeCutscene<CutsceneMoonLanding>();
        }
        if (sceneName == "mayors_house" && data.ghostsKilled >= 3 && !data.collectedItems.Contains("key_to_city") && !data.mayorAwardToday) {
            GameObject mayor = GameObject.Find("Mayor");
            if (mayor != null) {
                Speech mayorSpeech = mayor.GetComponent<Speech>();
                if (mayorSpeech != null)
                    mayorSpeech.defaultMonologue = "mayor_award";
            }
        }
        if (sceneName == "cave3" && data.toiletItems.Count > 0) {
            Collider2D toiletZone = GameObject.Find("toiletZone").GetComponent<Collider2D>();
            Bounds bounds = toiletZone.bounds;
            MySaver.LoadObjects(data.toiletItems);
            foreach (MyMarker marker in GameObject.FindObjectsOfType<MyMarker>()) {
                if (data.toiletItems.Contains(marker.id)) {
                    marker.transform.position = bounds.center + new Vector3(
                        (UnityEngine.Random.value - 0.5f) * bounds.size.x,
                        (UnityEngine.Random.value - 0.5f) * bounds.size.y,
                        (UnityEngine.Random.value - 0.5f) * bounds.size.z
                    );
                }
            }
            data.toiletItems = new List<System.Guid>();
        }
        PlayerEnter();
        if (playerIsDead) {
            UINew.Instance.RefreshUI(active: false);
            Instantiate(Resources.Load("UI/deathMenu"));
            CameraControl camControl = FindObjectOfType<CameraControl>();
            camControl.audioSource.PlayOneShot(Resources.Load("sounds/xylophone/x4") as AudioClip);
            Toolbox.Instance.SwitchAudioListener(GameObject.Find("Main Camera"));
        }
    }
    public GameObject InstantiatePlayerPrefab() {
        // Debug.Log("instantiate player");
        GameObject obj = GameObject.Instantiate(Resources.Load("prefabs/" + data.prefabName)) as GameObject;
        Toolbox.SetSkinColor(obj, data.defaultSkinColor);
        return obj;
    }
    public void InitializeNonPlayableLevel() {
        string sceneName = SceneManager.GetActiveScene().name;
        Controller.Instance.state = Controller.ControlState.cutscene;
        UINew.Instance.RefreshUI();
        Toolbox.Instance.SwitchAudioListener(GameObject.Find("Main Camera"));
        if (sceneName == "morning_cutscene") {
            CutsceneManager.Instance.InitializeCutscene<CutsceneNewDay>();
        }
        if (sceneName == "boardroom_cutscene") {
            CutsceneManager.Instance.InitializeCutscene<CutsceneBoardroom>();
        }
        if (sceneName == "anti_mayor_cutscene") {
            CutsceneManager.Instance.InitializeCutscene<CutsceneAntiMayor>();
        }
    }
    void PlayerEnter() {
        if (playerObject == null)
            return;
        List<Doorway> doorways = new List<Doorway>(GameObject.FindObjectsOfType<Doorway>());
        // TODO: can probably make this nicer with LINQ
        foreach (Doorway doorway in doorways) {
            if ((doorway.entryID == data.entryID && !doorway.spawnPoint) || (doorway.spawnPoint && data.entryID == 99)) {
                // if this is a bed entry, we've got a new day going on!
                if (data.entryID == -99) {
                    WakeUpInBed();
                }
                doorway.Enter(playerObject);
                if (data.entryID == 420) {
                    // teleport entry
                    AudioClip teleportEnter = Resources.Load("sounds/clown/clown4") as AudioClip;
                    PlayPublicSound(teleportEnter);
                    GameObject.Instantiate(Resources.Load("prefabs/fx/teleportEntryEffect"), playerObject.transform.position, Quaternion.identity);
                    Toolbox.Instance.AudioSpeaker(teleportEnter, playerObject.transform.position);
                }
            }
        }
    }
    void WakeUpInBed() {
        Hurtable playerHurtable = playerObject.GetComponent<Hurtable>();
        if (playerHurtable.hitState == Controllable.HitState.dead) {
            Destroy(playerObject);
            playerObject = InstantiatePlayerPrefab();
            SetFocus(playerObject);
        }
        foreach (Puddle puddle in GameObject.FindObjectsOfType<Puddle>()) {
            Destroy(puddle.gameObject);
        }
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
                UINew.Instance.UpdateTopActionButtons();
            }
            Eater focusEater = playerObject.GetComponent<Eater>();
            if (focusEater) {
                focusEater.nutrition = 0;
                focusEater.nausea = 0;
                while (focusEater.eatenQueue.Count > 0) {
                    GameObject eaten = focusEater.eatenQueue.Dequeue();
                    Destroy(eaten);
                }
            }
            if (playerHurtable) {
                playerHurtable.Reset();
            }
            Flammable playerFlammable = playerObject.GetComponent<Flammable>();
            if (playerFlammable) {
                playerFlammable.onFire = false;
                playerFlammable.heat = 0;
                playerFlammable.burnTimer = 0f;
            }
            Intrinsics playerIntrinsics = playerObject.GetComponent<Intrinsics>();
            if (playerIntrinsics) {
                playerIntrinsics.liveBuffs = new List<Buff>();

                Dictionary<BuffType, Buff> netBuffs = playerIntrinsics.NetBuffs();
                if (!netBuffs[BuffType.undead].active()) {
                    HeadAnimation headAnim = playerObject.GetComponentInChildren<HeadAnimation>();
                    AdvancedAnimation advAnim = playerObject.GetComponent<AdvancedAnimation>();

                    if (headAnim && headAnim.skinColor == SkinColor.undead) {
                        Debug.Log("setting head to " + data.defaultSkinColor.ToString());
                        headAnim.skinColor = data.defaultSkinColor;
                        headAnim.LoadSprites();
                    }
                    if (advAnim && advAnim.skinColor == SkinColor.undead) {
                        Debug.Log("setting adv anim to " + data.defaultSkinColor.ToString());
                        advAnim.skinColor = data.defaultSkinColor;
                        advAnim.LoadSprites();
                    }
                }
            }

            data.teleportedToday = false;
            MySaver.Save();

            if (data.days > 1) {
                awaitNewDayPrompt = data.itemsCollectedToday + data.foodCollectedToday + data.clothesCollectedToday + data.newUnlockedCommercials.Count > 0;
            } else {
                awaitNewDayPrompt = false;
                // show WASD
                GameObject.Instantiate(Resources.Load("UI/WASD"), new Vector3(0.183f, -0.703f, 0f), Quaternion.identity);
            }

            bed.SleepCutscene();
        }
        Computer computer = GameObject.FindObjectOfType<Computer>();
        if (computer != null) {
            computer.CheckBubble();
        }
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
        if (!debug)
            ReceiveEmail("start");
        sceneTime = 0f;
        timeSinceLastSave = 0f;
    }
    public void NewDay() {
        Debug.Log("New day");
        data.loadedDay = false;
        MySaver.CleanupSaves();
        MySaver.SaveObjectDatabase();
        List<string> keys = new List<string>(data.itemCheckedOut.Keys);
        foreach (string key in keys) {
            data.itemCheckedOut[key] = false;
        }
        DetermineClosetNews();
        SceneManager.LoadScene("apartment");
        sceneTime = 0f;
        data.entryID = -99;
        data.firstTimeLeavingHouse = true;
        data.mayorAwardToday = false;
        data.mayorLibraryShuffled = false;
        data.gangMembersDefeated = 0;
        data.activeCommercial = null;
        // data.recordingCommercial = false;
        SetRecordingStatus(false);
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
    public void AntiMayorCutscene() {
        SceneManager.LoadScene("anti_mayor_cutscene");
        sceneTime = 0f;
        data.entryID = -99;
    }
    public void TitleScreen() {
        data = null;
        SceneManager.LoadScene("title");
    }
    public GameData InitializedGameData() {
        GameData data = new GameData();
        data.defaultGender = Gender.male;
        data.defaultSkinColor = SkinColor.light;
        data.prefabName = "Tom";
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
            {"hypnosis", false},
            {"swear", false},
            {"potion", false},
            {"burn", false},
        };
        data.collectedClothes.Add("blue_shirt");
        data.collectedClothes.Add("pajamas");

        data.collectedObjects.Add("glass_jar");
        data.collectedObjects.Add("blue_shirt");
        data.collectedObjects.Add("pajamas");
        data.collectedObjects.Add("egg");

        data.collectedFood.Add("egg");

        data.itemCheckedOut["pajamas"] = false;
        data.itemCheckedOut["egg"] = false;
        data.itemCheckedOut["blue_shirt"] = false;
        data.itemCheckedOut["glass_jar"] = false;
        data.firstTimeLeavingHouse = true;
        // initialize commercials
        // TODO: change this temporary hack into something more correct.
        data.unlockedCommercials = new HashSet<Commercial>();
        data.newUnlockedCommercials = new HashSet<Commercial>();
        data.unlockedScenes = new HashSet<string>();
        data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("eat1"));
        data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("gravy1"));
        data.unlockedScenes.Add("studio");
        if (debug) {
            data.days = 5;
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("eat2"));
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("eggplant1"));
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("eggplant10"));
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("fireman"));
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("badboy"));
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("mayor"));
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("moon"));
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("vampire"));
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("boardroom"));
            data.perks["hypnosis"] = true;
            data.perks["vomit"] = true;
            data.perks["eat_all"] = true;
            data.perks["swear"] = true;
            data.perks["potion"] = false;
            data.perks["burn"] = false;
            data.collectedObjects.Add("crown");
            data.collectedClothes.Add("crown");
            data.itemCheckedOut["crown"] = false;
            foreach (string sceneName in sceneNames.Keys) {
                data.unlockedScenes.Add(sceneName);
            }
            data.teleporterUnlocked = true;
            data.mayorCutsceneHappened = true;
            data.visitedStudio = true;
            data.visitedMoon = true;
            data.finishedCommercial = true;

            data.collectedObjects.Add("package");
            data.itemCheckedOut["package"] = false;
            data.collectedObjects.Add("cosmic_nullifier");
            data.itemCheckedOut["cosmic_nullifier"] = false;
            data.cosmicName = GameManager.Instance.CosmicName();
        }
        data.recordingCommercial = false;
        data.completeCommercials = new HashSet<Commercial>();
        // initialize achievements
        data.achievements = new List<Achievement>();
        data.stats = new SerializableDictionary<StatType, Stat>();
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
        return path;
    }
    public void SaveGameData() {

        data.secondsPlayed += timeSinceLastSave;
        data.lastScene = SceneManager.GetActiveScene().name;
        data.saveDateTime = System.DateTime.Now;
        data.saveDate = System.DateTime.Now.ToString();

        var serializer = new XmlSerializer(typeof(GameData));
        string path = Path.Combine(Application.persistentDataPath, saveGameName);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        path = Path.Combine(path, "gameData.xml");
        try {
            using (FileStream sceneStream = File.Create(path)) {
                serializer.Serialize(sceneStream, data);
            }
        }
        catch (Exception e) {
            Debug.Log(e.ToString());
            Debug.Log(e.Source);
            Debug.Log(e.StackTrace);
            Debug.Log(e.Message);
        }
        // sceneStream.Close();
        timeSinceLastSave = 0f;

        SaveCommercial();
    }
    public void SaveCommercial() {
        if (data.activeCommercial == null)
            return;
        var serializer = new XmlSerializer(typeof(Commercial));
        string path = Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName);
        string outName = "commercial" + System.Guid.NewGuid().ToString() + ".xml";
        path = Path.Combine(path, outName);
        using (FileStream sceneStream = File.Create(path)) {
            serializer.Serialize(sceneStream, data.activeCommercial);
        }
    }
    public void LoadGameDataIntoMemory(string gameName) {
        MySaver.objectDataBase = null;
        saveGameName = gameName;
        // Debug.Log("Loadsavegame into memory");
        data = LoadGameData(gameName);
        if (data.lastScene != null) {
            SceneManager.LoadScene(data.lastScene);
        } else {
            SceneManager.LoadScene("apartment");
        }
        data.loadedDay = true;
        if (data.activeCommercial != null && data.recordingCommercial) {
            GameManager.Instance.StartCommercial(data.activeCommercial);
        }
    }
    public GameData LoadGameData(string gameName) {
        GameData data = null;
        var serializer = new XmlSerializer(typeof(GameData));
        string path = Path.Combine(Application.persistentDataPath, gameName);
        path = Path.Combine(path, "gameData.xml");
        if (File.Exists(path)) {
            try {
                using (var dataStream = new FileStream(path, FileMode.Open)) {
                    data = serializer.Deserialize(dataStream) as GameData;
                }
            }
            catch (Exception e) {
                Debug.Log("Error loading game data: " + path);
                Debug.Log(e.Message);
                Debug.Log(e.TargetSite);
            }
        }
        return data;
    }

    public void CheckItemCollection(GameObject obj, GameObject owner) {
        if (owner != playerObject)
            return;
        string filename = Toolbox.Instance.CloneRemover(obj.name);
        if (filename.ToLower() == "droplet" || filename.ToLower() == "puddle")
            return;
        // filename = Toolbox.Instance.ReplaceUnderscore(filename);
        if (data.collectedObjects.Contains(filename))
            return;
        UnityEngine.Object testPrefab = Resources.Load("prefabs/" + filename);
        if (testPrefab != null) {
            data.collectedObjects.Add(filename);
            data.itemCheckedOut[filename] = true;
            Poptext.PopupCollected(obj);
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
        if (data == null)
            return false;
        string filename = Toolbox.Instance.CloneRemover(obj.name);
        return data.collectedObjects.Contains(filename);
    }
    public void RetrieveCollectedItem(string name, HomeCloset.ClosetType closetType) {
        if (data.itemCheckedOut[name])
            return;
        GameObject item = Instantiate(Resources.Load("prefabs/" + name), playerObject.transform.position, Quaternion.identity) as GameObject;
        Instantiate(Resources.Load("particles/poof"), playerObject.transform.position, Quaternion.identity);
        publicAudio.PlayOneShot(Resources.Load("sounds/pop", typeof(AudioClip)) as AudioClip);
        data.itemCheckedOut[name] = true;
        if (closetType == HomeCloset.ClosetType.clothing) {
            Outfit playerOutfit = playerObject.GetComponent<Outfit>();
            Uniform itemUniform = item.GetComponent<Uniform>();
            if (playerOutfit != null && itemUniform != null) {
                // playerInventory.GetItem(itemPickup);
                playerOutfit.DonUniform(itemUniform);
                return;
            }

            Head playerHead = playerObject.GetComponentInChildren<Head>();
            Hat itemHat = item.GetComponent<Hat>();
            if (playerHead != null && itemHat != null) {
                playerHead.DonHat(itemHat);
                return;
            }

        } else {
            Inventory playerInventory = playerObject.GetComponent<Inventory>();
            Pickup itemPickup = item.GetComponent<Pickup>();
            if (playerInventory != null && itemPickup != null) {
                playerInventory.GetItem(itemPickup);
            }
        }
    }
    public void IncrementStat(StatType statType, float value) {
        // change stat
        if (!data.stats.ContainsKey(statType))
            data.stats[statType] = new Stat(statType);
        data.stats[statType].value += value;
        CheckStats();
    }
    public void SetStat(StatType statType, float value) {
        if (!data.stats.ContainsKey(statType))
            data.stats[statType] = new Stat(statType);
        data.stats[statType].value = value;
        CheckStats();
    }
    private void CheckStats() {
        // check achievements
        if (InCutsceneLevel())
            return;
        List<Achievement> completeAchievements = data.CheckAchievements();
        foreach (Achievement achievement in completeAchievements) {
            Poptext.PopupAchievement(achievement);
        }
    }
    public void ReceiveEmail(string emailName) {
        // Debug.Log("receiving email "+emailName);
        foreach (Email email in data.emails) {
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
            return;
        }
        data.packages.Add(packageName);
    }
    public void ShowDiaryEntry(string diaryName) {
        GameObject diaryObject = UINew.Instance.activeMenu;
        if (UINew.Instance.activeMenu == null) {
            diaryObject = UINew.Instance.ShowMenu(UINew.MenuType.diary);
        } else {
            UINew.Instance.CloseActiveMenu();
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

    public string CosmicName() {
        List<string> names = new List<string>(){
            "Stupor Mundi",
            "Northstar Megarainbow",
            "Hyperplane Godhead",
            "Ignotum P. Ignotius",
            "Magna Morti",
            "Quadriceps Potentia",
            "Pontifex Prime"
        };
        return names[UnityEngine.Random.Range(0, names.Count)];
    }
}

