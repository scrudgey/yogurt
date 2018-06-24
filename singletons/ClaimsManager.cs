using UnityEngine;
using System.Collections.Generic;

public class ClaimsManager : Singleton<ClaimsManager> {
    public Dictionary<GameObject, IExcludable> claimedItems = new Dictionary<GameObject, IExcludable>();
    public void ListObjects() {
        foreach (GameObject o in claimedItems.Keys) {
            Debug.Log(o.name);
        }
    }
    public void ClaimObject(GameObject obj, IExcludable owner) {
        if (obj == null || owner == null)
            return;
        // if someone else owns the object, tell them that it's being taken.
        if (claimedItems.ContainsKey(obj)) {
            claimedItems[obj].DropMessage(obj);
        }
        claimedItems[obj] = owner;
    }
    public void DisclaimObject(GameObject obj, IExcludable owner) {
        if (obj == null || owner == null)
            return;
        if (claimedItems.ContainsKey(obj))
            if (claimedItems[obj] == owner)
                claimedItems.Remove(obj);
    }
    public void WasDestroyed(GameObject obj) {
        if (obj == null)
            return;
        if (claimedItems.ContainsKey(obj)) {
            claimedItems[obj].WasDestroyed(obj);
            claimedItems.Remove(obj);
        }
    }
}
