using UnityEngine;
using System.Collections.Generic;

public enum MusicTrack {
    none,
    mainTitle,
    creepyAmbient,
    lithophone
}


// this will be a state machine eventually.
// fade audio out to play sting, fade back

// 1. intros, outros?
// 2. fade out on scene change, then fade in new track
// 3. single-play tracks
// 4. play music on UI changes
// stability vs. many sound effects
// configurable volume control
// fade audio out, play stinger, fade audio in

// glitch intro and glitch step

public class MusicController : Singleton<MusicController> {
    static Dictionary<MusicTrack, string> trackFiles = new Dictionary<MusicTrack, string>(){
        {MusicTrack.mainTitle, "Main Vamp w keys YC3"},
        {MusicTrack.creepyAmbient, "ForestMoon Alternate Loop YC3"},
        {MusicTrack.lithophone, "Lithophone LOOP YC3"},
    };
    private Dictionary<string, MusicTrack> sceneTracks = new Dictionary<string, MusicTrack>(){
        {"title", MusicTrack.mainTitle},
        {"house", MusicTrack.creepyAmbient}, // testing
        {"krazy1", MusicTrack.creepyAmbient}, // testing
        {"chamber", MusicTrack.lithophone},
    };

    public AudioSource audioSource;
    private Dictionary<MusicTrack, AudioClip> tracks = new Dictionary<MusicTrack, AudioClip>();
    public MusicTrack nowPlaying;
    public Camera cam;
    public void Start() {
        SetCamera();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);

        // populate tracks
        foreach (KeyValuePair<MusicTrack, string> kvp in trackFiles) {
            Debug.Log("loading music/" + kvp.Value + " ...");
            AudioClip clip = Resources.Load("music/" + kvp.Value) as AudioClip;
            Debug.Log(clip);
            tracks[kvp.Key] = clip;
        }
    }
    public void Update() {
        transform.position = cam.transform.position;
    }
    public void SetCamera() {
        cam = GameObject.FindObjectOfType<Camera>();
    }
    public void PlayTrack(MusicTrack track) {
        if (nowPlaying == track)
            return;

        if (audioSource == null)
            audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);

        audioSource.clip = tracks[track];
        audioSource.loop = true;
        audioSource.Play();
        Debug.Log(audioSource.clip);
        nowPlaying = track;
    }

    public void StopTrack() {
        nowPlaying = MusicTrack.none;
        if (audioSource != null) {
            audioSource.Stop();
        }
    }

    public void SceneChange(string sceneName) {
        SetCamera();
        Debug.Log("music scene change: " + sceneName);
        if (sceneTracks.ContainsKey(sceneName)) {
            PlayTrack(sceneTracks[sceneName]);
        } else {
            StopTrack();
        }
    }
}
