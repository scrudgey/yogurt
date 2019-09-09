using UnityEngine;

public class DestroyAfterTime : MonoBehaviour {
    public float lifetime;
    public bool deactivate;
    public float timer;
    void Start() {
        // TODO: implement claimsmanager wasdestroyed with timer
        ClaimsManager.Instance.WasDestroyed(gameObject);
        if (!deactivate)
            Destroy(gameObject, lifetime);
    }
    void Update() {
        if (deactivate) {
            timer += Time.deltaTime;
            if (timer > lifetime) {
                Destroy(this);
                gameObject.SetActive(false);
            }
        }
    }
}
