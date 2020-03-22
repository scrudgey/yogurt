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
    public static List<GameObject> disabledPersistents = new List<GameObject>();
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
            if (file.Name != "apartment_state.xml")
                File.Delete(file.FullName);
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
            // var playerStream = new FileStream(playerPath, FileMode.Open);
            // playerIDs = listSerializer.Deserialize(playerStream) as List<int>;
            // playerStream.Close();
            if (objectDataBase != null) {
                foreach (KeyValuePair<Guid, PersistentObject> kvp in objectDataBase) {
                    if (kvp.Value.sceneName != "apartment") {
                        if (!playerIDs.Contains(kvp.Key))
                            removeEntries.Push(kvp.Key);
                    }
                }
            }
        } else {
            if (objectDataBase != null) {
                foreach (KeyValuePair<Guid, PersistentObject> kvp in objectDataBase) {
                    if (kvp.Value.sceneName != "apartment") {
                        removeEntries.Push(kvp.Key);
                    }
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
        if (File.Exists(objectsPath)) {
            if (objectDataBase == null) {
                var persistentSerializer = new XmlSerializer(typeof(SerializableDictionary<Guid, PersistentObject>));
                Debug.Log("loading existing " + objectsPath + " ...");
                // System.IO.Stream objectsStream = new FileStream(objectsPath, FileMode.Open);
                using (System.IO.Stream objectsStream = new FileStream(objectsPath, FileMode.Open)) {
                    objectDataBase = persistentSerializer.Deserialize(objectsStream) as SerializableDictionary<Guid, PersistentObject>;
                }
                // objectDataBase = persistentSerializer.Deserialize(objectsStream) as SerializableDictionary<int, PersistentObject>;
                // objectsStream.Close();
                // Debug.Log(objectDataBase.Count.ToString() +" entries found");
            }
        } else {
            Debug.Log("NOTE: creating new object database!");
            objectDataBase = new SerializableDictionary<Guid, PersistentObject>();
        }
        // retrieve all persistent objects
        HashSet<GameObject> objectList = new HashSet<GameObject>();
        Dictionary<GameObject, PersistentObject> persistents = new Dictionary<GameObject, PersistentObject>();
        // add those objects which are disabled and would therefore not be found by our first search
        foreach (GameObject disabledPersistent in disabledPersistents) {
            objectList.Add(disabledPersistent);
        }
        foreach (MyMarker mark in GameObject.FindObjectsOfType<MyMarker>()) {
            objectList.Add(mark.gameObject);
        }
        Dictionary<GameObject, Guid> objectIDs = new Dictionary<GameObject, Guid>();
        HashSet<Guid> savedIDs = new HashSet<Guid>();
        // create a persistent for each gameobject with the appropriate data
        foreach (GameObject gameObject in objectList) {
            MyMarker marker = gameObject.GetComponent<MyMarker>();
            PersistentObject persistent;
            // either get the existing persistent in the database, or make a new one
            if (objectDataBase.ContainsKey(marker.id)) {
                persistent = objectDataBase[marker.id];
                persistent.Update(gameObject);
            } else {
                persistent = new PersistentObject(gameObject);
                marker.id = persistent.id;
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
                listSerializer.Serialize(sceneStream, savedIDs.ToList().Except(playerTree.ToList()).ToList());
            }
        }
        // remove all children objects from player tree. they are included in prefab.
        // note: the order of operations here means that child objects aren't in the scene or player trees.
        Stack<Guid> playerChildObjects = new Stack<Guid>();
        if (playerTree == null)
            Debug.Log("null player tree! wtf");
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
                listSerializer.Serialize(playerStream, playerTree.ToList());
            }
        }

        // close the XML serialization stream
        // sceneStream.Close();
        // playerStream.Close();
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

        // Debug.Log("saving "+objectsPath+" ...");
        // Debug.Log(objectDataBase.Count);
        using (FileStream objectStream = File.Create(objectsPath)) {
            persistentSerializer.Serialize(objectStream, objectDataBase);
        }
        // objectStream.Close();
    }
    public static GameObject LoadScene() {
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
            Debug.Log("WEIRD: no existing object database on Load!");
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
                    if (marks[i].staticObject)
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
        disabledPersistents = new List<GameObject>();
        List<Guid> sceneIDs = new List<Guid>();
        List<Guid> playerIDs = new List<Guid>();
        var listSerializer = new XmlSerializer(typeof(List<Guid>));
        if (File.Exists(scenePath)) {
            using (var sceneStream = new FileStream(scenePath, FileMode.Open)) {
                sceneIDs = listSerializer.Deserialize(sceneStream) as List<Guid>;
            }
            LoadObjects(sceneIDs);
        }
        if (File.Exists(playerPath)) {
            using (var playerStream = new FileStream(playerPath, FileMode.Open)) {
                playerIDs = listSerializer.Deserialize(playerStream) as List<Guid>;
            }
            playerObject = LoadObjects(playerIDs);
        } else {
            playerObject = GameManager.Instance.InstantiatePlayerPrefab();
        }
        HandleLoadedPersistents(sceneIDs);
        HandleLoadedPersistents(playerIDs);

        return playerObject;
    }
    public static void HandleLoadedPersistents(List<Guid> ids) {
        // TODO: smarter check?
        foreach (Guid idn in ids) {
            PersistentObject persistent = null;
            if (objectDataBase.TryGetValue(idn, out persistent)) {
                // TODO: handle update instead of replacement
                // TODO: alert on load if instantiation fails
                persistent.HandleLoad(loadedObjects[idn]);
            }
        }
    }
    public static GameObject LoadObjects(List<Guid> ids) {
        GameObject rootObject = null;
        foreach (Guid idn in ids) {
            PersistentObject persistent = null;
            if (objectDataBase.TryGetValue(idn, out persistent)) {
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
                Debug.LogError("object " + idn.ToString() + " not found in database");
            }
        }
        return rootObject;
    }
    public static void AddToReferenceTree(GameObject parent, GameObject child) {
        if (parent == null || child == null)
            return;
        if (savedObjects.ContainsKey(child) && savedObjects.ContainsKey(parent)) {
            AddToReferenceTree(savedObjects[parent], savedObjects[child]);
        } else {
            Debug.LogError("failed to add object to reference tree");
            Debug.LogError(parent);
            Debug.LogError(child);
            Debug.LogError(savedObjects.ContainsKey(parent));
            Debug.LogError(savedObjects.ContainsKey(child));
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
}
