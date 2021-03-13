using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
using static System.Guid;

public class MySaver {
    public static SerializableDictionary<Guid, PersistentObject> objectDataBase;
    private static Dictionary<Guid, List<Guid>> referenceTree = new Dictionary<Guid, List<Guid>>();
    public static Dictionary<Guid, GameObject> loadedObjects = new Dictionary<Guid, GameObject>();
    public static Dictionary<GameObject, Guid> savedObjects = new Dictionary<GameObject, Guid>();

    // this parameter keeps track of persistent objects that have been disabled, because GameObject.FindObjectsOfType<MyMarker>
    // won't find disabled objects. Hence we must keep track of them ourselves. This HashSet is reset at the start of each new level / load .
    public static HashSet<GameObject> disabledPersistents = new HashSet<GameObject>();
    public static List<Type> LoadOrder = new List<Type>{
        typeof(Intrinsics),
        typeof(Outfit),
        typeof(Inventory)
    };
    public static int CompareComponent(Component x, Component y) {
        if (LoadOrder.Contains(x.GetType()) && LoadOrder.Contains(y.GetType())) {
            return LoadOrder.IndexOf(x.GetType()) - LoadOrder.IndexOf(y.GetType());
        } else if (LoadOrder.Contains(x.GetType())) {
            return 1;
        } else if (LoadOrder.Contains(y.GetType())) {
            return -1;
        } else {
            return 0;
        }
    }
    public static void BackupFailedSave() {
        string path = Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName);
        string destPath = Path.Combine(Application.persistentDataPath, "crashdump");
        DirectoryCopy(path, destPath, true);
    }
    public static void DeleteSave(string saveGameName) {
        string path = Path.Combine(Application.persistentDataPath, saveGameName);
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(path);
        if (!dir.Exists) {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + path);
        }
        dir.Delete(true);
    }
    public static void CleanupSaves() {
        string path = Path.Combine(Application.persistentDataPath, GameManager.Instance.saveGameName);
        if (!System.IO.Directory.Exists(path))
            return;
        DirectoryInfo info = new DirectoryInfo(path);
        FileInfo[] fileInfo = info.GetFiles();
        foreach (FileInfo file in fileInfo) {
            // don't delete the object database, the gamedata, or the house state
            if (file.Name == GameManager.Instance.saveGameName + ".xml")
                continue;
            if (file.Name == "gameData.xml")
                continue;
            if (file.FullName == GameManager.Instance.data.lastSavedPlayerPath)
                continue;
            if (file.Name == LoadoutEditor.loadoutFileName)
                continue;
            if (file.Name != "apartment_state.xml") {
                // Debug.Log($"deleting file {file.FullName}");
                File.Delete(file.FullName);
            }
        }
        // GARBAGE COLLECTION
        // TODO: maybe a smarter algorithm here?
        var listSerializer = new XmlSerializer(typeof(List<Guid>));
        List<Guid> playerIDs = new List<Guid>();
        string playerPath = GameManager.Instance.data.lastSavedPlayerPath;
        Stack<Guid> removeEntries = new Stack<Guid>();
        if (File.Exists(playerPath)) {
            using (var playerStream = new FileStream(playerPath, FileMode.Open)) {
                playerIDs = listSerializer.Deserialize(playerStream) as List<Guid>;
            }
        }
        if (objectDataBase != null) {
            foreach (KeyValuePair<Guid, PersistentObject> kvp in objectDataBase) {
                // if the object is not in the toilet, apartment, or on the player, we will remove it.
                if (kvp.Value.sceneName != "apartment" && !GameManager.Instance.data.toiletItems.Contains(kvp.Key) && !playerIDs.Contains(kvp.Key)) {
                    removeEntries.Push(kvp.Key);
                }
            }
        }
        while (removeEntries.Count > 0) {
            Guid entry = removeEntries.Pop();
            objectDataBase.Remove(entry);
        }
    }
    public static void AddComponentToPersistent(GameObject target, Component component) {
        if (objectDataBase == null)
            return;
        MyMarker marker = target.GetComponent<MyMarker>();
        if (marker == null)
            return;
        if (objectDataBase.ContainsKey(marker.id)) {
            PersistentObject persistentObject = objectDataBase[marker.id];
            ISaveable saveable = component as ISaveable;
            if (saveable != null) {
                PersistentComponent persist = new PersistentComponent(persistentObject);
                persistentObject.persistentComponents[component.GetType().ToString()] = persist;
            }
        }
        // either get the existing persistent in the database, or make a new one

    }
    public static void Save() {
        referenceTree = new Dictionary<Guid, List<Guid>>();
        savedObjects = new Dictionary<GameObject, Guid>();
        var listSerializer = new XmlSerializer(typeof(List<Guid>));
        string objectsPath = GameManager.Instance.ObjectsSavePath();
        string scenePath = GameManager.Instance.LevelSavePath();
        string playerPath = GameManager.Instance.PlayerSavePath();

        GameManager.Instance.data.lastSavedPlayerPath = playerPath;

        if (File.Exists(objectsPath)) {
            if (objectDataBase == null) {
                var persistentSerializer = new XmlSerializer(typeof(SerializableDictionary<Guid, PersistentObject>));
                Debug.Log("loading existing " + objectsPath + " ...");
                // System.IO.Stream objectsStream = new FileStream(objectsPath, FileMode.Open);
                using (System.IO.Stream objectsStream = new FileStream(objectsPath, FileMode.Open)) {
                    objectDataBase = persistentSerializer.Deserialize(objectsStream) as SerializableDictionary<Guid, PersistentObject>;
                }
            }
        } else {
            // Debug.Log("NOTE: creating new object database!");
            objectDataBase = new SerializableDictionary<Guid, PersistentObject>();
        }
        // retrieve all persistent objects
        HashSet<GameObject> objectList = new HashSet<GameObject>();
        Dictionary<GameObject, PersistentObject> persistents = new Dictionary<GameObject, PersistentObject>();
        // add those objects which are disabled and would therefore not be found by our first search
        foreach (GameObject disabledPersistent in disabledPersistents) {
            objectList.Add(disabledPersistent);
            // Debug.Log($"saving disabled persistent: {disabledPersistent}");
        }
        foreach (MyMarker mark in GameObject.FindObjectsOfType<MyMarker>()) {
            objectList.Add(mark.gameObject);
            // Debug.Log($"saving regular marked object: {mark.gameObject}");
        }
        Dictionary<GameObject, Guid> objectIDs = new Dictionary<GameObject, Guid>();
        HashSet<Guid> savedIDs = new HashSet<Guid>();
        // create a persistent for each gameobject with the appropriate data
        foreach (GameObject gameObject in objectList) {
            MyMarker marker = gameObject.GetComponent<MyMarker>();
            PersistentObject persistent;
            // either get the existing persistent in the database, or make a new one
            if (objectDataBase.ContainsKey(marker.id)) {
                // Debug.Log($"updating persistent object {gameObject}: {marker.id}");
                persistent = objectDataBase[marker.id];
                persistent.Update(gameObject);
            } else {

                // Debug.Log($"creating new persistent object {gameObject}: {marker.id}");
                // creation of new persistent object.
                // it will take the id of the MyMarker.
                // TODO: make this myMarker.ToPersistentObject();
                persistent = new PersistentObject(gameObject);

                // marker.id = persistent.id;
                // critical: this is the only place we add persistent objects to the database.
                objectDataBase[marker.id] = persistent;
                loadedObjects[marker.id] = gameObject;
            }
            persistents[gameObject] = persistent;
            objectIDs.Add(gameObject, persistent.id);
            savedIDs.Add(persistent.id);
            savedObjects[gameObject] = persistent.id;
        }
        // invoke the data handling here - this will populate all the component data, and assign a unique id to everything.
        foreach (KeyValuePair<GameObject, PersistentObject> kvp in persistents) {
            kvp.Value.HandleSave(kvp.Key);
        }
        // separate lists of persistent objects for the scene and the player
        HashSet<Guid> playerTree = new HashSet<Guid>();
        if (GameManager.Instance.playerObject != null) {
            RecursivelyAddTree(playerTree, objectIDs[GameManager.Instance.playerObject]);
            foreach (PersistentObject childPersistent in persistents[GameManager.Instance.playerObject].persistentChildren.Values) {
                // add the child object's referents to tree
                RecursivelyAddTree(playerTree, childPersistent.id);
                playerTree.Remove(childPersistent.id);
            }
            using (FileStream sceneStream = File.Create(scenePath)) {
                // foreach (Guid savedGuid in savedIDs) {
                //     Debug.Log($"saving {savedGuid} to {scenePath}...");
                // }
                listSerializer.Serialize(sceneStream, savedIDs.ToList().Except(playerTree.ToList()).ToList());
            }
        }
        // remove all children objects from player tree. they are included in prefab.
        // note: the order of operations here means that child objects aren't in the scene or player trees.
        Stack<Guid> playerChildObjects = new Stack<Guid>();
        if (playerTree == null)
            Debug.LogWarning("null player tree! wtf");
        foreach (Guid idn in playerTree) {
            if (objectDataBase.ContainsKey(idn)) {
                if (objectDataBase[idn].childObject) {
                    playerChildObjects.Push(idn);
                }
            } else {
                Debug.Log("couldn't find " + idn.ToString() + " in objectdatabase??");
            }
        }
        while (playerChildObjects.Count > 0) {
            playerTree.Remove(playerChildObjects.Pop());
        }
        if (playerTree.Count > 0) {
            using (FileStream playerStream = File.Create(playerPath)) {
                // foreach (Guid savedGuid in playerTree) {
                //     Debug.Log($"saving {savedGuid} to {playerPath}...");
                // }
                listSerializer.Serialize(playerStream, playerTree.ToList());
            }
        }

        GameManager.Instance.SaveGameData();
        if (!File.Exists(objectsPath)) {
            SaveObjectDatabase();
        }
    }
    public static void SaveObjectDatabase() {
        if (objectDataBase == null)
            return;
        var persistentSerializer = new XmlSerializer(typeof(SerializableDictionary<Guid, PersistentObject>));
        string objectsPath = GameManager.Instance.ObjectsSavePath();
        // Debug.Log("saving object database");
        Debug.LogWarning($"saving object database");
        using (FileStream objectStream = File.Create(objectsPath)) {
            persistentSerializer.Serialize(objectStream, objectDataBase);
        }
    }
    public static GameObject LoadScene(bool newDayLoad = false) {
        UINew.Instance.ClearWorldButtons();
        GameObject playerObject = null;
        loadedObjects = new Dictionary<Guid, GameObject>();

        string objectsPath = GameManager.Instance.ObjectsSavePath();
        string scenePath = GameManager.Instance.LevelSavePath();
        string playerPath = GameManager.Instance.data.lastSavedPlayerPath;

        if (File.Exists(objectsPath)) {
            // Debug.Log("loading "+objectsPath+" ...");
            if (objectDataBase == null) {
                var persistentSerializer = new XmlSerializer(typeof(SerializableDictionary<Guid, PersistentObject>));
                using (System.IO.Stream objectsStream = new FileStream(objectsPath, FileMode.Open)) {
                    objectDataBase = persistentSerializer.Deserialize(objectsStream) as SerializableDictionary<Guid, PersistentObject>;
                }
            }
        } else {
            // Debug.Log("WEIRD: no existing object database on Load!");
            objectDataBase = new SerializableDictionary<Guid, PersistentObject>();
        }

        // destroy any currently existing permanent object
        // this should only be done if there exists a savestate for the level.
        // otherwise the default unity editor scene should be loaded as is.
        if (File.Exists(scenePath)) {
            List<MyMarker> marks = new List<MyMarker>(GameObject.FindObjectsOfType<MyMarker>());
            Stack<GameObject> gameObjectsToDestroy = new Stack<GameObject>();
            for (int i = 0; i < marks.Count; i++) {
                if (marks[i] != null) {
                    // do not destroy static objects
                    if (marks[i].staticObject)
                        continue;

                    // do not destroy apartmentobject on newday
                    if (newDayLoad && marks[i].apartmentObject)
                        continue;

                    gameObjectsToDestroy.Push(marks[i].gameObject);
                }
            }
            foreach (GameObject disabledPersistent in disabledPersistents) {
                gameObjectsToDestroy.Push(disabledPersistent);
            }
            while (gameObjectsToDestroy.Count > 0) {
                GameObject.DestroyImmediate(gameObjectsToDestroy.Pop());
            }
        }
        disabledPersistents = new HashSet<GameObject>();
        List<Guid> sceneIDs = new List<Guid>();
        List<Guid> playerIDs = new List<Guid>();
        var listSerializer = new XmlSerializer(typeof(List<Guid>));
        if (File.Exists(scenePath)) {
            using (var sceneStream = new FileStream(scenePath, FileMode.Open)) {
                sceneIDs = listSerializer.Deserialize(sceneStream) as List<Guid>;
            }
            LoadObjects(sceneIDs, newDayLoad: newDayLoad);
        }
        if (File.Exists(playerPath)) {
            using (var playerStream = new FileStream(playerPath, FileMode.Open)) {
                playerIDs = listSerializer.Deserialize(playerStream) as List<Guid>;
            }
            playerObject = LoadObjects(playerIDs);
        } else {
            playerObject = GameManager.Instance.InstantiatePlayerPrefab();
        }
        HandleLoadedPersistents(sceneIDs, newDayLoad: newDayLoad);
        HandleLoadedPersistents(playerIDs, newDayLoad: newDayLoad);

        // if this is the start of a new day in the apartment, we delete apartment objects from the database
        // because instead of loading them, we will use the scene's default
        if (newDayLoad) {
            List<Guid> apartmentKeys = new List<Guid>();
            foreach (KeyValuePair<Guid, PersistentObject> kvp in objectDataBase) {
                if (kvp.Value.apartmentObject) {
                    // objectDataBase.Remove(kvp.Key);
                    apartmentKeys.Add(kvp.Key);
                }
            }
            foreach (Guid apartmentKey in apartmentKeys) {
                objectDataBase.Remove(apartmentKey);
            }
        }

        return playerObject;
    }
    public static void HandleLoadedPersistents(List<Guid> ids, bool newDayLoad = false) {
        // TODO: smarter check?
        foreach (Guid idn in ids) {
            PersistentObject persistent = null;
            if (objectDataBase.TryGetValue(idn, out persistent)) {
                if (newDayLoad && persistent.apartmentObject)
                    continue;
                // TODO: handle update instead of replacement
                // TODO: alert on load if instantiation fails
                persistent.HandleLoad(loadedObjects[idn]);
            }
        }
    }
    public static GameObject LoadObjects(List<Guid> ids, bool newDayLoad = false) {
        GameObject rootObject = null;
        foreach (Guid idn in ids) {
            PersistentObject persistent = null;
            if (objectDataBase.TryGetValue(idn, out persistent)) {
                if (newDayLoad && persistent.apartmentObject)
                    continue;
                // TODO: do something smarter to find the child object
                GameObject go = null;
                if (persistent.noPrefab) {
                    go = GameObject.Find(persistent.name);
                } else {
                    go = GameObject.Instantiate(
                    Resources.Load(persistent.prefabPath),
                    persistent.transformPosition,
                    persistent.transformRotation) as GameObject;
                }
                if (go == null) {
                    Debug.Log("WARNING: Object not found " + persistent.prefabPath);
                    continue;
                }
                // Debug.Log($"loaded object {idn} {go}");
                loadedObjects[persistent.id] = go;
                go.name = Toolbox.Instance.CloneRemover(go.name);
                if (!rootObject)
                    rootObject = go;
                MyMarker marker = go.GetComponent<MyMarker>();
                if (marker) {
                    marker.id = persistent.id;
                }
                Toolbox.GetOrCreateComponent<Intrinsics>(go);
            } else {
                Debug.LogError($"object {idn} not found in database");
            }
        }
        return rootObject;
    }
    public static bool AddToReferenceTree(GameObject parent, GameObject child) {
        if (parent == null || child == null)
            return false;
        if (savedObjects.ContainsKey(child) && savedObjects.ContainsKey(parent)) {
            AddToReferenceTree(savedObjects[parent], savedObjects[child]);
            return true;
        } else {
            Debug.LogError("failed to add object to reference tree");
            Debug.LogError(parent);
            Debug.LogError(child);
            Debug.LogError(savedObjects.ContainsKey(parent));
            Debug.LogError(savedObjects.ContainsKey(child));
            return false;
        }
    }
    public static void AddToReferenceTree(GameObject parent, Guid child) {
        if (child == Guid.Empty || parent == null) {
            Debug.LogWarning("tried to add null GUID to reference tree");
            Debug.LogWarning("child: " + child.ToString());
            Debug.LogWarning("parent: " + parent.ToString());
            return;
        }
        if (savedObjects.ContainsKey(parent)) {
            AddToReferenceTree(savedObjects[parent], child);
        }
    }
    public static void AddToReferenceTree(Guid parent, GameObject child) {
        if (parent == Guid.Empty || child == null) {
            Debug.LogWarning("tried to add null GUID to reference tree");
            Debug.LogWarning("child: " + child.ToString());
            Debug.LogWarning("parent: " + parent.ToString());
            return;
        }
        if (savedObjects.ContainsKey(child)) {
            AddToReferenceTree(parent, savedObjects[child]);
        }
    }
    public static void AddToReferenceTree(Guid parent, Guid child) {
        if (child == Guid.Empty || parent == Guid.Empty) {
            Debug.LogWarning("tried to add null GUID to reference tree");
            Debug.LogWarning("child: " + child.ToString());
            Debug.LogWarning("parent: " + parent.ToString());
            return;
        }
        // Debug.Log("adding " + child.ToString() + " under " + parent.ToString());
        if (!referenceTree.ContainsKey(parent))
            referenceTree[parent] = new List<Guid>();
        referenceTree[parent].Add(child);
    }
    public static void RecursivelyAddTree(HashSet<Guid> tree, Guid node) {
        tree.Add(node);
        if (referenceTree.ContainsKey(node)) {
            foreach (Guid idn in referenceTree[node]) {
                if (!tree.Contains(idn)) {
                    RecursivelyAddTree(tree, idn);
                }
            }
        }
    }
    public static void UpdateGameObjectReference(GameObject referent, PersistentComponent data, string key, bool overWriteWithNull = true) {
        // is the target object null?
        if (referent == null) {
            if (overWriteWithNull) {
                data.ints[key] = -1;
                return;
            } else {
                return;
            }
        }
        Guid i = Guid.Empty;
        // is the non-null object in the saved objects?
        if (savedObjects.TryGetValue(referent, out i)) {
            data.GUIDs[key] = i;
        } else {
            if (overWriteWithNull) {
                data.GUIDs[key] = Guid.Empty;
            }
        }
    }
    public static GameObject IDToGameObject(Guid idn) {
        if (idn == Guid.Empty) {
            Debug.LogWarning("tried to convert null GUID to Gameobject");
            return null;
        }
        GameObject returnObject;
        loadedObjects.TryGetValue(idn, out returnObject);
        return returnObject;
    }
    // public static int NextIDNumber() {
    //     IEnumerable<int> ids = Enumerable.Range(0, objectDataBase.Count);
    //     foreach (int idn in ids) {
    //         if (!objectDataBase.ContainsKey(idn))
    //             return idn;
    //     }
    //     return objectDataBase.Count + 1;
    // }
    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists) {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName)) {
            Directory.CreateDirectory(destDirName);
        }

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files) {
            string temppath = Path.Combine(destDirName, file.Name);
            FileInfo info = new FileInfo(temppath);
            if (info.Exists) {
                info.Delete();
            }
            file.CopyTo(temppath, false);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs) {
            foreach (DirectoryInfo subdir in dirs) {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, copySubDirs);
            }
        }
    }

    /*
    * why is this here? if a component disables an object and later destroys it, it will remain
    * as a disabled persistent, and won't be cleaned up properly in some situations.
    */
    public static void RemoveObject(GameObject gameObject) {
        MyMarker marker = gameObject.GetComponent<MyMarker>();
        if (marker != null && objectDataBase != null) {
            if (objectDataBase.ContainsKey(marker.id)) {
                Debug.Log($"removing persistent object {gameObject}");
                objectDataBase.Remove(marker.id);
            } else {
                Debug.LogWarning($"remove id {marker.id} not in object database!");
            }


            if (GameManager.Instance.data.toiletItems.Contains(marker.id)) {
                Debug.Log($"removing toilet object {gameObject}");
                GameManager.Instance.data.toiletItems.Remove(marker.id);
            }
        } else {
            Debug.LogWarning($"asked to remove saved object {gameObject} with no marker");
        }

        if (disabledPersistents.Contains(gameObject)) {
            disabledPersistents.Remove(gameObject);
            Debug.Log($"removing disabled persistent {gameObject}");
        }

    }
}
