using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Duplicatable : Interactive, ISaveable {
    public bool duplicatable = true;
    public bool nullifiable = true;
    public GameObject duplicationPrefab;
    public GameObject nullifyFX;
    public List<AudioClip> nullifySounds;
    public string adoptedName = "";
    void Start() {
        if (nullifyFX == null)
            nullifyFX = Resources.Load("particles/nullify") as GameObject;

        string prefabName = Regex.Replace(gameObject.name, @" \(.+\)", "");     // removes "Tom (1)"
        prefabName = Toolbox.Instance.CloneRemover(prefabName);                 // removes "(clone)"
        prefabName = PersistentObject.regexSpace.Replace(prefabName, "_");      // changes space to underscore
        // Debug.Log(prefabName);
        duplicationPrefab = Resources.Load($"prefabs/{prefabName}") as GameObject;
        if (nullifySounds.Count == 0) {
            nullifySounds = new List<AudioClip>();
            nullifySounds.Add(Resources.Load("sounds/absorbed") as AudioClip);
        }
    }
    public bool Nullifiable() {
        // don't nullify if it is an uncollected item in the apartment. everything else is fair game.
        if (nullifiable && SceneManager.GetActiveScene().name == "apartment") {
            if (gameObject.GetComponent<Pickup>()) {
                if (GameManager.Instance.IsItemCollected(gameObject)) {
                    return true;
                } else {
                    return false;
                }
            }
        }
        return nullifiable;
    }
    public bool PickleReady() {
        return Nullifiable() && gameObject.GetComponent<Pickup>();
    }
    public void Nullify() {
        if (GameManager.Instance.playerObject == gameObject) {
            GameManager.Instance.IncrementStat(StatType.nullifications, 1);
            GameManager.Instance.PlayerDeath();
        }
        if (gameObject.name.Contains("ghost") && SceneManager.GetActiveScene().name == "mayors_attic") {
            GameManager.Instance.data.ghostsKilled += 1;
        }
        if (nullifySounds.Count > 0) {
            Toolbox.Instance.AudioSpeaker(nullifySounds[Random.Range(0, nullifySounds.Count)], transform.position);
        }
        if (nullifyFX != null) {
            Instantiate(nullifyFX, transform.position, Quaternion.identity);
        }

        EventData dupData = EventData.Nullification(gameObject);
        Toolbox.Instance.OccurenceFlag(gameObject, dupData);

        Container container = GetComponent<Container>();
        if (container != null) {
            while (container.items.Count > 0) {
                if (container.items[0] == null) {
                    container.items.RemoveAt(0);
                } else {
                    foreach (MonoBehaviour component in container.items[0].GetComponents<MonoBehaviour>())
                        component.enabled = true;
                    Pickup removed = container.Dump(container.items[0]);
                    Destroy(removed.gameObject);
                }
            }
        }

        ClaimsManager.Instance.WasDestroyed(gameObject);
        Destroy(gameObject);
    }
    public void SaveData(PersistentComponent data) {
        data.strings["adopted"] = adoptedName;
    }
    public void LoadData(PersistentComponent data) {
        adoptedName = data.strings["adopted"];
    }
}
