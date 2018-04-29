using UnityEngine;

public class DestroyOnceQuiet : MonoBehaviour {
    public bool instantiatedByToolbox;
    void Update() {
        if (!GetComponent<AudioSource>().isPlaying) {
            ClaimsManager.Instance.WasDestroyed(gameObject);
            Destroy(gameObject);
            if (instantiatedByToolbox)
                Toolbox.Instance.numberOfLiveSpeakers -= 1;
        }
    }
}
