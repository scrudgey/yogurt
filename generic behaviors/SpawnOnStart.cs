using UnityEngine;

public class SpawnOnStart : MonoBehaviour {
    public GameObject prefab;
    void Start() {
        Instantiate(prefab, transform.position, Quaternion.identity);
    }
}
