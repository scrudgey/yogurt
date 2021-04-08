using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class StomachDisplayManager : MonoBehaviour {
    public Text stomachText;
    public Dictionary<System.Guid, StomachContentsIndicator> items = new Dictionary<System.Guid, StomachContentsIndicator>();

    // void Start() {
    //     Debug.Log("starting");
    //     items = new Dictionary<System.Guid, StomachContentsIndicator>();
    // }
    // when update is called, determine if something must be added or removed.

    // figure out if there are new items. if so, create new item indicator.
    // figure out if there are item indicators that no longer apply. if so, delete them.
    public void UpdateContents(Eater eater) {
        // Debug.Log("updating stomach contents");
        Dictionary<System.Guid, GameObject> eatenObjects = new Dictionary<System.Guid, GameObject>();
        foreach (GameObject eatenObject in eater.eatenQueue) {
            MyMarker marker = eatenObject.GetComponent<MyMarker>();
            if (marker == null) {
                // Debug.LogWarning($"eaten object with no marker: {eatenObject}");
                continue;
            }
            eatenObjects[marker.id] = eatenObject;
            // Debug.Log($"{eatenObject}: {marker.id}");
        }
        HashSet<System.Guid> missingIds = new HashSet<System.Guid>(eatenObjects.Keys);
        missingIds.ExceptWith(items.Keys);

        HashSet<System.Guid> extraIds = new HashSet<System.Guid>(items.Keys);
        extraIds.ExceptWith(eatenObjects.Keys);

        foreach (System.Guid missingId in missingIds) {
            // Debug.Log($"new id: {missingId}");
            CreateNewStomachIndicator(eatenObjects[missingId], missingId);
        }
        foreach (System.Guid extraId in extraIds) {
            // Debug.Log($"extra id: {extraId}");
            RemoveStomachIndicator(extraId);
        }

        stomachText.enabled = items.Count > 0;
        stomachText.transform.SetAsFirstSibling();
    }

    public void CreateNewStomachIndicator(GameObject item, System.Guid id) {
        GameObject indicator = GameObject.Instantiate(Resources.Load("UI/StomachIndicator")) as GameObject;
        indicator.transform.SetParent(transform, false);
        indicator.transform.SetAsFirstSibling();
        StomachContentsIndicator script = indicator.GetComponent<StomachContentsIndicator>();
        items[id] = script;
        script.Configure(item, id);
    }
    public void RemoveStomachIndicator(System.Guid id) {
        items[id].Remove();
        items.Remove(id);
    }
}
