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
            GameObject speaker = Instantiate(Resources.Load("Speaker"), transform.position, Quaternion.identity) as GameObject;
            speaker.GetComponent<AudioSource>().clip = groundImpactSounds[Random.Range(0, groundImpactSounds.Length)];
            speaker.GetComponent<AudioSource>().Play();
        }
    }
}
