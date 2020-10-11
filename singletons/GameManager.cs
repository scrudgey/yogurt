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
    public List<Liquid> collectedLiquids;
    public List<string> newCollectedItems;
    public List<string> collectedFood;
    public List<string> newCollectedFood;
    public List<string> collectedClothes;
    public List<string> newCollectedClothes;
    public List<Liquid> newCollectedLiquids;
    public SerializableDictionary<string, MutablePotionData> collectedPotions;
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
    public List<string> commercialsInitializedToday;
    public List<Email> emails = new List<Email>();
    public List<string> televisionShows;
    public HashSet<string> newTelevisionShows;
    public List<string> packages;
    public bool firstTimeLeavingHouse;
    public bool mayorCutsceneHappened;
    public bool visitedStudio;
    public bool visitedThroneRoom;
    public bool finishedCommercial;
    public bool visitedMoon;
    public bool teleporterUnlocked;
    public string headSpriteSheet;
    public SkinColor headSkinColor;
    public string cosmicName = "";
    public bool loadedDay;
    public Int16 ghostsKilled;
    public bool mayorAwardToday;
    public bool foughtSpiritToday;
    public bool mayorLibraryShuffled;
    public int gangMembersDefeated;
    public bool lichRevivalToday;
    public Commercial activeCommercial;
    public bool recordingCommercial;

    public List<System.Guid> toiletItems = new List<System.Guid>();
    public bool loadedSewerItemsToday = false;
    public GameData() {
        days = 0;
        saveDate = System.DateTime.Now.ToString();
        saveDateTime = System.DateTime.Now;
    }

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
        // {"hell1", "hell"},
        {"venus1", "venus"},
        {"fountain", "ruins"},
        {"gravy_studio", "gravy commercial studio"},
        {"hells_kitchen", "hell's kitchen"},
        {"hells_landing", "hell's landing"},
        {"venus_temple", "venus temple"},
        {"hells_vomit_ratchet", "hell's utility tunnel"},
        {"lower_hell", "lower hell"},
    };
    public GameData data;
    public string saveGameName = "test";
    private CameraControl cameraControl;
    public Camera cam;
    public GameObject playerObject;
    public Vector3 lastPlayerPosition;
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
    public bool loadingSavedGame = false;
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
        InitSettings();
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
        if (playerObject != null) {
            lastPlayerPosition = playerObject.transform.position;
        }
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
        Hurtable hurtable = playerObject.GetComponent<Hurtable>();
        Duplicatable duplicatable = playerObject.GetComponent<Duplicatable>();

        MessageDamage message = new MessageDamage(100f, damageType.physical);
        Vector2 rand = UnityEngine.Random.insideUnitCircle;
        message.force = new Vector3(rand.x, rand.y, 2f).normalized;

        if (head != null) {
            GameObject headGibs = Resources.Load("prefabs/gibs/headGibsContainer") as GameObject;
            foreach (Gibs gib in headGibs.GetComponents<Gibs>()) {
                Gibs newGib = head.gameObject.AddComponent<Gibs>();
                newGib.CopyFrom(gib);
                newGib.Emit(message);
            }
            AudioClip boom = Resources.Load("sounds/explosion/cannon") as AudioClip;
            PlayPublicSound(boom);
            Destroy(head.gameObject);
            IncrementStat(StatType.deathByExplodingHead, 1);
        }

        if (hurtable != null) {
            hurtable.Die(message, damageType.physical);
        } else if (duplicatable != null) {
            duplicatable.Nullify();
        } else {
            Destroy(playerObject);
            PlayerDeath();
        }
    }
    public bool InCutsceneLevel() {
        return SceneManager.GetActiveScene().buildIndex <= 3;
        // return SceneManager.GetActiveScene().buildIndex <= 4;
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
        UINew.Instance.UpdateStatusIcons(intrinsics);
    }
    public void SetFocus(GameObject target) {
        // if (playerObject != null) {
        // Controllable oldControl = playerObject.GetComponent<Controllable>();
        // oldControl.control = Controllable.ControlType.AI;
        // }
        Toolbox.Instance.SwitchAudioListener(target);
        playerObject = target;
        // Controllable targetControl = playerObject.GetComponent<Controllable>();
        InputController.Instance.focus = target.GetComponent<Controllable>();
        // targetControl.control = Controllable.ControlType.player;
        cameraControl = FindObjectOfType<CameraControl>();
        if (cameraControl)
            cameraControl.focus = target;
        Intrinsics intrinsics = Toolbox.GetOrCreateComponent<Intrinsics>(target);
        FocusIntrinsicsChanged(intrinsics);
        // check collections for new focus outfit, holding, and hat
        Outfit playerOutfit = target.GetComponent<Outfit>();
        if (playerOutfit) {
            playerGender = playerOutfit.gender;
            if (!playerOutfit.nude) {
                string prefabName = playerOutfit.wornUniformName;
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
        Collider2D collider = target.GetComponent<Collider2D>();
        foreach (CameraZoomZone zoomZone in GameObject.FindObjectsOfType<CameraZoomZone>()) {
            zoomZone.ForceRecalculate(collider);
        }
        Hurtable hurtable = target.GetComponent<Hurtable>();
        if (hurtable != null) {
            playerIsDead = hurtable.hitState == Controllable.HitState.dead;
        }
        // refresh UI
        UINew.Instance.RefreshUI(active: true);
    }
    public void LeaveScene(string toSceneName, int toEntryNumber) {
        Time.timeScale = 0f;

        UINew.Instance.FadeOut(() => DoLeaveScene(toSceneName, toEntryNumber));
    }
    public void SceneWasLoaded(Scene scene, LoadSceneMode mode) {
        UINew.Instance.FadeIn(() => DoSceneWasLoaded(scene, mode));
        Toolbox.Instance.numberOfLiveSpeakers = 0;
        publicAudio.Stop();
        sceneTime = 0f;
        Flammable.audioPlayingFires = new HashSet<Flammable>(); // probably unnecessary
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
        Time.timeScale = 0f;
    }
    public void DoLeaveScene(string toSceneName, int toEntryNumber) {
        Time.timeScale = 1f;

        MySaver.Save();
        data.entryID = toEntryNumber;
        SceneManager.LoadScene(toSceneName);
    }
    public void DoSceneWasLoaded(Scene scene, LoadSceneMode mode) {
        Time.timeScale = 1f;
    }
    void ResetGameState() {
        try {
            // FileInfo playerFile = new FileInfo(GameManager.Instance.data.lastSavedPlayerPath);
            // if (playerFile.Exists) {
            //     playerFile.Delete();
            // }
            MySaver.BackupFailedSave();
            MySaver.CleanupSaves();

            string housePath = Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName, "apartment_state.xml");
            string[] paths = new string[] { housePath, GameManager.Instance.data.lastSavedPlayerPath };

            foreach (string path in paths) {
                FileInfo info = new FileInfo(path);
                if (!info.Exists)
                    continue;
                info.Delete();
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

        // check here if it's a new day load
        UINew.Instance.ConfigureUIElements();
        if (loadLevel) {
            playerObject = MySaver.LoadScene(newDayLoad: data.entryID == -99);
            if (data.entryID == -99) {
                // reset apartment by deleting all controllables
                foreach (Controllable controllable in GameObject.FindObjectsOfType<Controllable>()) {
                    if (controllable.gameObject != playerObject) {
                        Destroy(controllable.gameObject);
                    }
                }
            }
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
        InputController.Instance.state = InputController.ControlState.normal;
        UINew.Instance.RefreshUI(active: true);
        // do not enter if we are loading the scene from main menu
        if (!loadingSavedGame) {
            SpecificSceneInitialize(sceneName);
            PlayerEnter();
        }
        loadingSavedGame = false;

        Vector3 position = playerObject.transform.position;
        position.z = -10;
        cam.transform.position = position;


        if (data.activeCommercial != null) {
            CheckCommercialInitialization(data.activeCommercial, sceneName);
        }
        if (playerIsDead) {
            UINew.Instance.RefreshUI(active: false);
            Instantiate(Resources.Load("UI/deathMenu"));
            CameraControl camControl = FindObjectOfType<CameraControl>();
            camControl.audioSource.PlayOneShot(Resources.Load("sounds/xylophone/x4") as AudioClip);
            Toolbox.Instance.SwitchAudioListener(GameObject.Find("Main Camera"));
        }
    }
    public void SpecificSceneInitialize(string sceneName) {
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
        if (sceneName == "cave1" || sceneName == "cave2" || (sceneName == "cave3" && data.entryID == 3) || (sceneName == "cave4" && data.entryID == 0)) {
            CutsceneManager.Instance.InitializeCutscene<CutsceneFall>();
        }
        if (sceneName == "dungeon" && data.entryID != 2) {
            CutsceneManager.Instance.InitializeCutscene<CutsceneDungeonFall>();
        }
        if (sceneName == "space") {
            CutsceneManager.Instance.InitializeCutscene<CutsceneSpace>();
        }
        if (sceneName == "portal") {
            CutsceneManager.Instance.InitializeCutscene<CutscenePortal>();
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
        if (sceneName == "cave3" && !data.loadedSewerItemsToday) {
            data.loadedSewerItemsToday = true;
            Collider2D toiletZone = GameObject.Find("toiletZone").GetComponent<Collider2D>();
            Bounds bounds = toiletZone.bounds;
            MySaver.LoadObjects(data.toiletItems);
            MySaver.HandleLoadedPersistents(data.toiletItems, newDayLoad: false);
            foreach (MyMarker marker in GameObject.FindObjectsOfType<MyMarker>()) {
                if (data.toiletItems.Contains(marker.id)) {
                    marker.transform.position = bounds.center + new Vector3(
                        (UnityEngine.Random.value - 0.5f) * bounds.size.x,
                        (UnityEngine.Random.value - 0.5f) * bounds.size.y,
                        (UnityEngine.Random.value - 0.5f) * bounds.size.z
                    );
                }
            }
        }
        if (sceneName == "devils_throneroom") {
            if (!data.visitedThroneRoom) {
                StartCoroutine(CutsceneManager.Instance.waitAndStartCutscene<CutsceneThroneroom>(2));
                data.visitedThroneRoom = true;
            }
        }
        if (data.days >= 2) {
            UnlockTVShow("vampire1");
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
        InputController.Instance.state = InputController.ControlState.cutscene;
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
                    GameObject.Instantiate(Resources.Load("particles/teleportEntryEffect"), playerObject.transform.position, Quaternion.identity);
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

        // apartment normally does not reset. here, we want to reset certain items every morning.
        // if the item no longer exists, create it.
        // however, the item state will persist intra-day.

        // normally, a scene state persists by deleting persistent objects upon scene load and then creating them based on save information.
        // in our case, we want to:
        //      when it's not a new day, delete all persistent objects in apartment, and load state like normal.
        //      when it is a new day, don't delete select items (keep around their default state)
        //          do not load corresponding persistent state (this is a tricky part)
        //              add a new flag to persistent objects and markers.

        // detect new day on load

        // toilet
        // piggybank
        // fridge

        // people
        // https://github.com/scrudgey/yogurt/issues/123


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
            Head head = playerObject.GetComponentInChildren<Head>();
            if (head != null && head.hat != null) {
                Hat hat = head.RemoveHat();
                DestroyImmediate(hat.gameObject);
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
                // empty the stomachs
                while (focusEater.eatenQueue.Count > 0) {
                    GameObject eaten = focusEater.eatenQueue.First.Value;
                    focusEater.eatenQueue.RemoveFirst();
                    DestroyImmediate(eaten);
                }
            }
            if (playerHurtable) {
                playerHurtable.Reset();
            }
            Flammable playerFlammable = playerObject.GetComponent<Flammable>();
            if (playerFlammable) {
                playerFlammable.onFire = false;
                playerFlammable.heat = 0;
                playerFlammable.SetBurnTimer();
            }
            Intrinsics playerIntrinsics = playerObject.GetComponent<Intrinsics>();
            if (playerIntrinsics) {
                playerIntrinsics.liveBuffs = new List<Buff>();

                Dictionary<BuffType, Buff> netBuffs = playerIntrinsics.NetBuffs();
                if (!netBuffs[BuffType.undead].active()) {
                    HeadAnimation headAnim = playerObject.GetComponentInChildren<HeadAnimation>();
                    AdvancedAnimation advAnim = playerObject.GetComponent<AdvancedAnimation>();

                    if (headAnim && headAnim.skinColor == SkinColor.undead) {
                        // Debug.Log("setting head to " + data.defaultSkinColor.ToString());
                        headAnim.skinColor = data.defaultSkinColor;
                        headAnim.LoadSprites();
                    }
                    if (advAnim && advAnim.skinColor == SkinColor.undead) {
                        // Debug.Log("setting adv anim to " + data.defaultSkinColor.ToString());
                        advAnim.skinColor = data.defaultSkinColor;
                        advAnim.LoadSprites();
                    }
                }
                FocusIntrinsicsChanged(playerIntrinsics);
            }

            data.teleportedToday = false;
            MySaver.Save();

            if (data.days > 1) {
                awaitNewDayPrompt = data.itemsCollectedToday + data.foodCollectedToday + data.clothesCollectedToday + data.newUnlockedCommercials.Count > 0;
            } else {
                awaitNewDayPrompt = false;
                // show WASD
                GameObject.Instantiate(Resources.Load("UI/WASD"), new Vector3(-0.73f, -0.703f, 0f), Quaternion.identity);
            }

            bed.SleepCutscene();
        }
        Computer computer = GameObject.FindObjectOfType<Computer>();
        if (computer != null) {
            computer.CheckBubble();
        }
        // instantiate original player character if missing?
        // not prefabname
        // data.prefabName 
        // List<string> origPrefabs = new List<string>() { "Tom", "Tina", "Brf", "Brm", "Blf", "Blm" };
        Debug.Log(data.prefabName.ToLower());
        Debug.Log(Toolbox.Instance.CloneRemover(playerObject.name).ToLower());
        if (!data.prefabName.ToLower().Contains(Toolbox.Instance.CloneRemover(playerObject.name).ToLower())) {
            Debug.Log("instantiating player prefab on apartment newday");
            GameObject origPlayer = InstantiatePlayerPrefab();
            GameObject spawnPoint = GameObject.Find("SpawnPoint");
            origPlayer.transform.position = spawnPoint.transform.position;
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
        if (!debug) {
            ReceiveEmail("start");
            // UnlockTVShow("vampire1");
        } else {
            UnlockTVShow("vampire1");
            UnlockTVShow("vampire2");
        }
        sceneTime = 0f;
        timeSinceLastSave = 0f;
    }
    public void NewDay() {
        // Debug.Log("New day");
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
        data.foughtSpiritToday = false;
        data.mayorLibraryShuffled = false;
        data.gangMembersDefeated = 0;
        data.activeCommercial = null;
        data.loadedSewerItemsToday = false;
        data.lichRevivalToday = false;
        data.commercialsInitializedToday = new List<string>();
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
        data.collectedLiquids = new List<Liquid>();
        data.newCollectedLiquids = new List<Liquid>();
        data.newCollectedFood = new List<string>();
        data.collectedClothes = new List<string>();
        data.newCollectedClothes = new List<string>();
        data.collectedPotions = new SerializableDictionary<string, MutablePotionData>();
        foreach (PotionData potionData in PotionComponent.LoadAllPotions()) {
            data.collectedPotions[potionData.name] = new MutablePotionData(potionData);
        }
        data.commercialsInitializedToday = new List<string>();

        data.packages = new List<string>();
        data.emails = new List<Email>();
        data.itemCheckedOut = new SerializableDictionary<string, bool>();
        data.perks = new SerializableDictionary<string, bool>(){
            {"vomit", false},
            {"eat_all", false},
            {"hypnosis", false},
            {"swear", false},
            {"potion", false},
            // {"burn", false},
            {"beverage", false},
            {"resurrection", false}
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

        data.televisionShows = new List<string>();
        data.newTelevisionShows = new HashSet<string>();

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
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("nullify1"));
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("nullify2"));
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("dungeon"));
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("hell"));
            data.unlockedCommercials.Add(Commercial.LoadCommercialByFilename("satan"));
            data.perks["hypnosis"] = true;
            data.perks["vomit"] = true;
            data.perks["eat_all"] = true;
            data.perks["swear"] = true;
            data.perks["potion"] = false;
            data.perks["burn"] = false;
            data.perks["resurrection"] = false;
            data.perks["beverage"] = true;
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
            data.visitedThroneRoom = true;
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
        data.achievements = AchievementManager.LoadAchievements();
        data.stats = new SerializableDictionary<StatType, Stat>();
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
        loadingSavedGame = true;
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
    public void CheckLiquidCollection(Liquid l, GameObject owner) {
        Debug.Log("check liquid");
        if (owner != playerObject)
            return;
        foreach (Liquid atomicLiquid in l.atomicLiquids) {
            // again, not redefining equality because i'm lazy / kinda buzzed
            bool match = false;
            foreach (Liquid collectedLiquid in data.collectedLiquids) {
                if (collectedLiquid.Equals(atomicLiquid)) {
                    match = true;
                    continue;
                }
            }
            if (match) continue;
            Debug.Log($"collecting {atomicLiquid.name}");
            Liquid newAtomicLiquid = new Liquid(atomicLiquid);
            data.collectedLiquids.Add(newAtomicLiquid);
            data.newCollectedLiquids.Add(newAtomicLiquid);
        }

        foreach (Liquid collectedLiquid in data.collectedLiquids) {
            if (collectedLiquid.Equals(l)) {
                CheckLiquidAchievement();
                return;
            }
        }
        Debug.Log($"collecting {l.name}");
        Liquid newLiquid = new Liquid(l);
        data.collectedLiquids.Add(new Liquid(newLiquid));
        data.newCollectedLiquids.Add(new Liquid(newLiquid));
        CheckLiquidAchievement();
    }
    public void CheckLiquidAchievement() {

        int collectedRegWater = 0;
        int collectedRiverWater = 0;
        int collectedMoonWater = 0;
        int collectedToiletWater = 0;
        foreach (Liquid liquid in data.collectedLiquids) {
            if (liquid.name.ToLower() == "water")
                collectedRegWater = 1;
            if (liquid.name.ToLower() == "river water")
                collectedRiverWater = 1;
            if (liquid.name.ToLower() == "toilet water")
                collectedToiletWater = 1;
            if (liquid.name.ToLower() == "moon water")
                collectedMoonWater = 1;
        }
        int totalWaters = collectedRegWater + collectedRiverWater + collectedToiletWater + collectedMoonWater;
        SetStat(StatType.typesOfWaterCollected, totalWaters);
    }
    public void CheckItemCollection(GameObject obj, GameObject owner) {
        if (owner != playerObject)
            return;
        string filename = Toolbox.Instance.CloneRemover(obj.name);
        if (filename.ToLower() == "droplet" || filename.ToLower() == "puddle")
            return;
        if (filename.ToLower().Contains("cosmic_nullifier")) {
            ReceiveEmail("nullify1");
            UnlockCommercial("nullify1");
        }
        if (filename.ToLower() == "strength_potion") {
            ReceiveEmail("strength");
        }
        if (GameManager.Instance.data.perks["vomit"] &&
        GameManager.Instance.data.perks["eat_all"] &&
        filename.ToLower() == "camcorder") {
            GameManager.Instance.UnlockCommercial("dungeon");
            GameManager.Instance.ReceiveEmail("dungeon_start");
        }
        if (data.collectedObjects.Contains(filename))
            return;
        UnityEngine.Object testPrefab = Resources.Load("prefabs/" + filename);
        if (testPrefab != null) {
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
            Pickup pickup = obj.GetComponent<Pickup>();
            if (pickup != null && !pickup.heavyObject) {
                data.collectedItems.Add(filename);
                data.newCollectedItems.Add(filename);
                data.itemsCollectedToday += 1;
            }

            if (pickup == null || !pickup.heavyObject) {
                if (filename.ToLower().Contains("cosmic_nullifier")) {
                    ShowDiaryEntry("nullifier");
                }
                data.collectedObjects.Add(filename);
                data.itemCheckedOut[filename] = true;
                Poptext.PopupCollected(obj);
            }
        }
    }
    public bool IsItemCollected(GameObject obj) {
        if (data == null)
            return false;
        string filename = Toolbox.Instance.CloneRemover(obj.name);
        return data.collectedObjects.Contains(filename);
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
        List<Achievement> completeAchievements = AchievementManager.Instance.CheckAchievements(data);
        foreach (Achievement achievement in completeAchievements) {
            Poptext.PopupAchievement(achievement);
        }
    }
    public void ReceiveEmail(string emailName) {
        if (data == null)
            return;
        // Debug.Log("receiving email " + emailName);
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
    public void UnlockTVShow(string showName) {
        if (data.televisionShows.Contains(showName))
            return;

        data.televisionShows.Add(showName);
        data.newTelevisionShows.Add(showName);
        Television tv = GameObject.FindObjectOfType<Television>();
        if (tv != null) {
            tv.CheckBubble();
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
            "Pontifex Prime",
            "Hapax Legomenon"
        };
        return names[UnityEngine.Random.Range(0, names.Count)];
    }
}

