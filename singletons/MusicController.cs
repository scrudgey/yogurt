using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum MusicTrack {
    none,
    mainTitle,
    creepyAmbient,
    lithophone,
    jingle,
    moonCave,
    moonWarp,
    fillerBeat,
    tweakDelay,
    dracIntro,
    dracLoop,
    chela,
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

// tweakDelay -> you died through main menu

struct MusicSpec {
    public MusicTrack track;
    public bool stopMusicOnEnter;
    public MusicSpec(MusicTrack track, bool stopMusicOnEnter) {
        this.track = track;
        this.stopMusicOnEnter = stopMusicOnEnter;
    }
    public MusicSpec(MusicTrack track) {
        this.track = track;
        this.stopMusicOnEnter = true;
    }
}

public class MusicController : Singleton<MusicController> {
    static Dictionary<MusicTrack, string> trackFiles = new Dictionary<MusicTrack, string>(){
        {MusicTrack.mainTitle, "Main Vamp w keys YC3"},
        {MusicTrack.creepyAmbient, "ForestMoon Alternate Loop YC3"},
        {MusicTrack.lithophone, "Lithophone FULL YC3"},
        {MusicTrack.jingle, "jingle1"},
        {MusicTrack.moonCave, "Moon Cave Loop YC3"},
        {MusicTrack.moonWarp, "Moon WARP Loop YC3"},
        {MusicTrack.fillerBeat, "Filler Beat YC3"},
        {MusicTrack.tweakDelay, "tweakDelay YC3"},
        {MusicTrack.chela, "Chela Theme YC3"},
        {MusicTrack.dracIntro, "Dracula Mansion INTRO YC3"},
        {MusicTrack.dracLoop, "Dracula Mansion LOOP YC3"},
    };
    private Dictionary<string, MusicSpec> sceneTracks = new Dictionary<string, MusicSpec>(){
        {"title", new MusicSpec(MusicTrack.mainTitle)},
        // {"house", MusicTrack.creepyAmbient}, // testing
        // {"krazy1", MusicTrack.creepyAmbient}, // testing
        {"chamber", new MusicSpec(MusicTrack.lithophone)},
        {"mayors_attic", new MusicSpec(MusicTrack.creepyAmbient)},
        {"space", new MusicSpec(MusicTrack.moonWarp)},
        {"moon1", new MusicSpec(MusicTrack.moonCave)},
        {"moon_pool", new MusicSpec(MusicTrack.moonCave)},
        {"moon_town", new MusicSpec(MusicTrack.moonCave)},
        {"moon_cave", new MusicSpec(MusicTrack.moonCave)},
        {"boardroom_cutscene", new MusicSpec(MusicTrack.fillerBeat)},
        {"morning_cutscene", new MusicSpec(MusicTrack.none)},
    };

    public AudioSource audioSource;
    private Dictionary<MusicTrack, AudioClip> tracks = new Dictionary<MusicTrack, AudioClip>();
    public MusicTrack nowPlaying;
    public Camera cam;
    public bool deathMusic;
    public string lastLoadedSceneName;
    public void Awake() {
        SetCamera();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        // populate tracks
        foreach (KeyValuePair<MusicTrack, string> kvp in trackFiles) {
            // Debug.Log("loading music/" + kvp.Value + " ...");
            AudioClip clip = Resources.Load("music/" + kvp.Value) as AudioClip;
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
        if (track == MusicTrack.none)
            return;
        if (audioSource == null)
            audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);

        audioSource.clip = tracks[track];
        audioSource.loop = true;
        audioSource.Play();
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
        UpdateTrack();
    }
    public void UpdateTrack() {
        Scene scene = SceneManager.GetActiveScene();
        if (GameManager.settings.musicOn) {
            MusicSpec music = new MusicSpec();
            if (sceneTracks.TryGetValue(scene.name, out music)) {
                PlayTrack(music.track);
            } else {
                StopTrack();
            }
        } else {
            StopTrack();
        }
        lastLoadedSceneName = scene.name;
    }
}
