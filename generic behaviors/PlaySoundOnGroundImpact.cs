using UnityEngine;
public class PlaySoundOnGroundImpact : MonoBehaviour {
    public AudioClip[] groundImpactSounds;
    void Start() {
        if (groundImpactSounds.Length > 0) {
            Toolbox.Instance.SetUpAudioSource(gameObject);
        }
    }
    public void OnGroundImpact(Physical phys) {
        if (groundImpactSounds.Length > 0) {
            Toolbox.Instance.AudioSpeaker(groundImpactSounds[Random.Range(0, groundImpactSounds.Length)], transform.position);
        }
    }
}
