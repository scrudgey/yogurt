using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class PersistentObject {
    private static readonly Regex regexClone = new Regex(@"(.+)\(Clone\)$", RegexOptions.Multiline);
    private static readonly Regex regexNumber = new Regex(@"(.+)\(\d+\)$", RegexOptions.Multiline);
    public static readonly Regex regexSpace = new Regex("\\s+", RegexOptions.Multiline);
    public string name;
    public string prefabPath;
    public bool noPrefab;
    public bool childObject;
    public bool apartmentObject;
    public System.Guid id;
    public Vector3 transformPosition;
    public Vector3 transformScale;
    public Quaternion transformRotation;
    public SerializableDictionary<string, PersistentComponent> persistentComponents = new SerializableDictionary<string, PersistentComponent>();
    public SerializableDictionary<string, PersistentObject> persistentChildren = new SerializableDictionary<string, PersistentObject>();
    public string parentObject;
    public int creationDate;
    public string sceneName;
    public float spriteColorRed;
    public float spriteColorGreen;
    public float spriteColorBlue;
    public float spriteColorAlpha;
    public PersistentObject() {
        // needed for XML serialization
    }
    public void Update(GameObject gameObject) {
        transformPosition = gameObject.transform.localPosition;
        transformRotation = gameObject.transform.localRotation;
        transformScale = gameObject.transform.localScale;
        sceneName = SceneManager.GetActiveScene().name;
    }
    public PersistentObject(GameObject gameObject) {
        // id = MySaver.NextIDNumber();
        // id = System.Guid.NewGuid();
        creationDate = GameManager.Instance.data.days;

        name = gameObject.name;
        MatchCollection matches = regexClone.Matches(name);
        if (matches.Count > 0) {                                    // the object is a clone, capture just the normal name
            name = matches[0].Groups[1].Value;
        }
        matches = regexNumber.Matches(name);
        if (matches.Count > 0) {
            name = matches[0].Groups[1].Value;
        }
        // Debug.Log(name);
        name = name.Trim();
        // set up the persistent transform
        Update(gameObject);
        MyMarker marker = gameObject.GetComponent<MyMarker>();
        if (marker != null) {
            // id = marker.id;
            apartmentObject = marker.apartmentObject;
            foreach (GameObject childObject in marker.persistentChildren) {
                if (childObject == null)
                    continue;
                PersistentObject persistentChildObject = new PersistentObject(childObject);
                persistentChildObject.parentObject = childObject.name;
                persistentChildren[childObject.name] = persistentChildObject;
                persistentChildObject.childObject = true;
            }
            id = marker.id;
        } else {
            // Debug.LogWarning($"creating persistent object for object with no marker!");
            // Debug.LogWarning($"{gameObject}");
            id = System.Guid.NewGuid();
        }

        // prefabPath = @"prefabs/" + name;
        // prefabPath = regexSpace.Replace(prefabPath, "_");
        prefabPath = Toolbox.GetPrefabPath(gameObject);
        if (Resources.Load(prefabPath) == null) {
            noPrefab = true;
            name = gameObject.name;
        }
        foreach (Component component in gameObject.GetComponents<Component>()) {
            if (component is ISaveable) {
                PersistentComponent persist = new PersistentComponent(this);
                persistentComponents[component.GetType().ToString()] = persist;
            }
        }
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            spriteColorAlpha = spriteRenderer.color.a;
            spriteColorRed = spriteRenderer.color.r;
            spriteColorGreen = spriteRenderer.color.g;
            spriteColorBlue = spriteRenderer.color.b;
        }
        // spriteColor = spriteRenderer.color;
    }
    public void HandleSave(GameObject parentObject) {
        foreach (Component component in parentObject.GetComponents<Component>()) {
            ISaveable saveable = component as ISaveable;
            if (saveable != null) {
                // TODO: update each component, don't override.
                if (!persistentComponents.ContainsKey(component.GetType().ToString())) {
                    Debug.Log("broken persistentComponent reference");
                    Debug.Log(component.GetType());
                    Debug.Log(parentObject.name);
                    continue;
                }
                saveable.SaveData(persistentComponents[component.GetType().ToString()]);
            }
        }
        foreach (KeyValuePair<string, PersistentObject> kvp in persistentChildren) {
            if (kvp.Value == this)
                continue;
            Transform childTransform = parentObject.transform.Find(kvp.Key);
            if (childTransform == null)
                continue;
            GameObject childObject = childTransform.gameObject;
            kvp.Value.HandleSave(parentObject.transform.Find(kvp.Key).gameObject);
        }
    }
    public void HandleLoad(GameObject parentObject) {
        MyMarker parentMarker = parentObject.GetComponent<MyMarker>();
        if (parentMarker != null) {
            parentMarker.id = id;
        } else {
            // Debug.LogWarning($"loaded object {parentObject} has no marker id: {id}");
        }

        parentObject.transform.localRotation = transformRotation;
        parentObject.transform.localScale = transformScale;
        SpriteRenderer spriteRenderer = parentObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            spriteRenderer.color = new Color(spriteColorRed, spriteColorGreen, spriteColorBlue, spriteColorAlpha);
        }
        List<Component> loadedComponents = new List<Component>(parentObject.GetComponents<Component>());
        loadedComponents.Sort(MySaver.CompareComponent);
        foreach (Component component in loadedComponents) {
            ISaveable saveable = component as ISaveable;
            if (saveable != null) {
                if (persistentComponents.ContainsKey(component.GetType().ToString())) {
                    saveable.LoadData(persistentComponents[component.GetType().ToString()]);
                } else {
                    Debug.Log("on load could not find persistent component " + component.GetType().ToString() + " on object " + name);
                }
            }
        }
        foreach (KeyValuePair<string, PersistentObject> kvp in persistentChildren) {
            if (kvp.Value == this)
                continue;
            GameObject childObject = parentObject.transform.Find(kvp.Key).gameObject;
            if (childObject != null) {

                kvp.Value.HandleLoad(childObject);
            } else {
                Debug.Log("on load could not find child object " + kvp.Key);
            }
        }

    }
}
