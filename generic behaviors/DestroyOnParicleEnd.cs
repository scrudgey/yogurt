using UnityEngine;
public class DestroyOnParicleEnd : MonoBehaviour {
    void Update() {
        if (!GetComponent<ParticleSystem>().IsAlive()) {
            ClaimsManager.Instance.WasDestroyed(gameObject);
            Destroy(gameObject);
        }
    }
}
