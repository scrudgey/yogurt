using UnityEngine;
using System.Collections.Generic;
public class MyMarker : MonoBehaviour, IExcludable {
    public System.Guid id = System.Guid.Empty;
    public List<GameObject> persistentChildren;
    public bool staticObject;
    public bool apartmentObject;
    void Awake() {
        if (id == System.Guid.Empty)
            id = System.Guid.NewGuid();
        // Debug.Log($"{gameObject} {id}");
    }
    void OnDisable() {
        MySaver.disabledPersistents.Add(gameObject);
    }
    void OnEnable() {
        MySaver.disabledPersistents.Remove(gameObject);
    }
    public void addChild(GameObject target) {
        ClaimsManager.Instance.ClaimObject(target, this);
        persistentChildren.Add(target);
    }
    void OnDestroy() {
        if (MySaver.disabledPersistents.Contains(gameObject)) {
            MySaver.disabledPersistents.Remove(gameObject);
        }
    }
    public void DropMessage(GameObject obj) {
        if (persistentChildren.Contains(obj)) {
            persistentChildren.Remove(obj);
        }
    }
    public void WasDestroyed(GameObject obj) {
        if (!persistentChildren.Contains(obj)) {
            persistentChildren.Remove(obj);
        }
    }
}
