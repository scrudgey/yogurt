using UnityEngine;

public class DestroyOnceQuiet : MonoBehaviour {
    void Update() {
        if (!GetComponent<AudioSource>().isPlaying) {
            ClaimsManager.Instance.WasDestroyed(gameObject);
            Destroy(gameObject);
        }
    }
}
