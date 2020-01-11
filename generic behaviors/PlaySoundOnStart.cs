using UnityEngine;
public class PlaySoundOnStart : MonoBehaviour {
    public AudioClip sound;
    private AudioSource source;
    public AudioClip[] randomSounds;
    public bool disableSpatialBlending;
    void Start() {
        if (randomSounds.Length > 0) {
            sound = randomSounds[Random.Range(0, randomSounds.Length)];
        }
        source = Toolbox.Instance.SetUpAudioSource(gameObject);
        if (disableSpatialBlending)
            source.spatialBlend = 0;
        if (sound != null)
            source.PlayOneShot(sound);
    }
}
