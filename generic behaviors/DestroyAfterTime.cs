using UnityEngine;

public class DestroyAfterTime : MonoBehaviour {
    public float lifetime;
    public bool deactivate;
    public float timer;
    public GameObject spawnPrefab;
    void Start() {
        // TODO: implement claimsmanager wasdestroyed with timer
        ClaimsManager.Instance.WasDestroyed(gameObject);
        // if (!deactivate)
        //     Destroy(gameObject, lifetime);
    }
    void Update() {
        timer += Time.deltaTime;
        if (timer > lifetime) {
            Destroy(this);
            if (deactivate) {
                gameObject.SetActive(false);
            } else {
                Destroy(gameObject);
            }
            if (spawnPrefab) {
                GameObject.Instantiate(spawnPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}
