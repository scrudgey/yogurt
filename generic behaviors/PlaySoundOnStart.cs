using UnityEngine;
public class PlaySoundOnStart : MonoBehaviour {
	public AudioClip sound;
	private AudioSource source;
	void Start () {
		source = Toolbox.Instance.SetUpAudioSource(gameObject);
		if (sound != null)
			source.PlayOneShot(sound);
	}
}
