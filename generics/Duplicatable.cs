using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
        // if (duplicationPrefab == null)
        duplicationPrefab = Resources.Load("prefabs/" + Toolbox.Instance.CloneRemover(gameObject.name)) as GameObject;
        if (nullifySounds.Count == 0) {
            nullifySounds = new List<AudioClip>();
            nullifySounds.Add(Resources.Load("sounds/absorbed") as AudioClip);
        }
    }
    public bool Nullifiable() {
        // don't nullify if it is an uncollected item in the apartment. everything else is fair game.
        if (SceneManager.GetActiveScene().name == "apartment") {
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
        if (nullifySounds.Count > 0) {
            Toolbox.Instance.AudioSpeaker(nullifySounds[Random.Range(0, nullifySounds.Count)], transform.position);
        }
        if (nullifyFX != null) {
            Instantiate(nullifyFX, transform.position, Quaternion.identity);
        }
        // OccurrenceEat eatData = new OccurrenceEat();
        EventData nullData = EventData.Nullification(gameObject);
        Toolbox.Instance.OccurenceFlag(gameObject, nullData);

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
