using UnityEngine;

public interface IExcludable {
    void DropMessage(GameObject obj);
    void WasDestroyed(GameObject obj);
}