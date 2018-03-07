using UnityEngine;
public class PlaySoundOnStart : MonoBehaviour {
    public AudioClip sound;
    private AudioSource source;
    public bool disableSpatialBlending;
    void Start() {
        source = Toolbox.Instance.SetUpAudioSource(gameObject);
        if (disableSpatialBlending)
            source.spatialBlend = 0;
        if (sound != null)
            source.PlayOneShot(sound);
    }
}
