using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundGibberizer : MonoBehaviour {
    private bool _play;
    public bool play {
        set {
            if (value && !_play && !bleep){        
                // start play sound
                Play();
            }
            _play = value;
        }
        get {
            return _play;
        }
    }
    public Vector2 pitchRange = new Vector2(0.6f, 0.7f);
    public Vector2 spacingRange = new Vector2(0.2f, 0.25f);
    public AudioClip[] sounds;
    public AudioSource audioSource;
    public AudioClip bleepSound;
    private bool _bleep;
    public bool bleep{
        get {
            return _bleep;
        }
        set {
            if (value && !_bleep){
                audioSource.Stop();
                audioSource.loop = true;
                audioSource.clip = bleepSound;
                audioSource.pitch = 1;
                audioSource.Play();
            }
            if (!value && _bleep){
                audioSource.Stop();
                audioSource.loop = false;
                spacingTimer = currentSpace;
            }
            _bleep = value;
        }
    }
    public float spacingTimer = 0;
    private float currentPitch;
    private float currentSpace;
    
	void Start () {
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        currentPitch = Random.Range(pitchRange.x, pitchRange.y);
        currentSpace = Random.Range(spacingRange.x, spacingRange.y);
        // StartPlay();
	}
	
	void Update () {
        // if (bleep){

        //     if (!audioSource.isPlaying){
        //         audioSource.PlayOneShot(bleepSound);
        //     }
        // }
        if (bleep)
            return;
		if (play){
            if (audioSource.isPlaying)
                return;
            spacingTimer += Time.deltaTime;
            if (spacingTimer > currentSpace){
                Play();
            }
        }
	}
    public void Play(){
        // play with current values
        AudioClip clip = sounds[Random.Range(0, sounds.Length)];
        audioSource.pitch = currentPitch;
        audioSource.PlayOneShot(clip);

        // reset timer
        spacingTimer = 0;

        // select new values
        currentPitch = Random.Range(pitchRange.x, pitchRange.y);
        currentSpace = Random.Range(spacingRange.x, spacingRange.y);
    }
    public void StartPlay(){
        play = true;
    }
    public void StopPlay(){
        play = false;
        audioSource.Stop();
        spacingTimer = 0;
    }
}
