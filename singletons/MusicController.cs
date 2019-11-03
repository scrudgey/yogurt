using UnityEngine;
using System.Collections.Generic;
using System.Collections;
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

// for music interruption, we need finer control over what is playing
// and when. Music needs to be treated like tracks in a playlist:
// music.play
// music.pause
// music.stop
// music.end

// not all music ends (especially if looping)
// but when it does end, it calls back to music controller
// the music controller will play the next in queue, if there is one.

// in this way, we can handle intro-loop much better.

// but we need to allow scenes to specify:
// vampire, dungeon ->
// enqueue(track(intro, loop:false))
// enqueue(track(loop, loop:true))

// question: how does a single non-loop track know when it ends?
// ans: Invoke("End", length(track))

// question: how do we track now playing, again?
// scenes point to unique identifiers, point to unique start

public abstract class Music {
    public abstract void Play(AudioSource audioSource);
    public abstract bool Equals(Music other);
}
public class MusicSingle : Music {
    public MusicTrack track;
    public MusicSingle(MusicTrack track) {
        this.track = track;
    }
    public override void Play(AudioSource audioSource) {
        audioSource.clip = MusicController.tracks[track];
        audioSource.loop = true;
        audioSource.Play();
    }
    public override bool Equals(Music other) {
        MusicSingle otherSingle = other as MusicSingle;
        if (otherSingle != null) {
            return otherSingle.track == track;
        } else return false;
    }
}
public class MusicIntroLoop : Music {
    public MusicTrack intro;
    public MusicTrack loop;
    private AudioClip introClip;
    private AudioClip loopClip;
    public MusicIntroLoop(MusicTrack intro, MusicTrack loop) {
        this.intro = intro;
        this.loop = loop;
        introClip = MusicController.tracks[intro];
        loopClip = MusicController.tracks[loop];
    }
    override public void Play(AudioSource audioSource) {
        MusicController.Instance.StartCoroutine(playThenLoop(audioSource));
    }
    IEnumerator playThenLoop(AudioSource audioSource) {
        audioSource.loop = false;
        audioSource.clip = introClip;
        audioSource.Play();
        yield return new WaitForSecondsRealtime(introClip.length);
        if (GameManager.settings.musicOn) {
            audioSource.loop = true;
            audioSource.clip = loopClip;
            audioSource.Play();
        }
    }
    public override bool Equals(Music other) {
        MusicIntroLoop otherMusic = other as MusicIntroLoop;
        if (otherMusic != null) {
            return otherMusic.intro == intro && otherMusic.loop == loop;
        } else return false;
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
    private static Dictionary<string, Music> sceneTracks;
    public AudioSource audioSource;
    public static Dictionary<MusicTrack, AudioClip> tracks = new Dictionary<MusicTrack, AudioClip>();
    public Music nowPlaying;
    public Queue<Music> queue;
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
        sceneTracks = new Dictionary<string, Music>(){
        {"title", new MusicSingle(MusicTrack.mainTitle)},
        // {"house", MusicTrack.creepyAmbient}, // testing
        // {"krazy1", MusicTrack.creepyAmbient}, // testing
        {"chamber", new MusicSingle(MusicTrack.lithophone)},
        {"mayors_attic", new MusicSingle(MusicTrack.creepyAmbient)},
        {"space", new MusicSingle(MusicTrack.moonWarp)},
        {"moon1", new MusicSingle(MusicTrack.moonCave)},
        {"moon_pool", new MusicSingle(MusicTrack.moonCave)},
        {"moon_town", new MusicSingle(MusicTrack.moonCave)},
        {"moon_cave", new MusicSingle(MusicTrack.moonCave)},
        {"anti_mayors_house", new MusicSingle(MusicTrack.moonCave)},
        {"boardroom_cutscene", new MusicSingle(MusicTrack.fillerBeat)},
        {"morning_cutscene", new MusicSingle(MusicTrack.none)},
        {"forest", new MusicSingle(MusicTrack.creepyAmbient)},
        {"woods", new MusicSingle(MusicTrack.creepyAmbient)},
        {"potion", new MusicSingle(MusicTrack.creepyAmbient)},
        {"vampire_house", new MusicIntroLoop(MusicTrack.dracIntro, MusicTrack.dracLoop)},
        {"dungeon", new MusicIntroLoop(MusicTrack.dracIntro, MusicTrack.dracLoop)},
    };
    }
    public void Update() {
        transform.position = cam.transform.position;
    }
    public void SetCamera() {
        cam = GameObject.FindObjectOfType<Camera>();
    }
    public void PlayMusic(Music music) {
        if (music == null)
            return;
        if (nowPlaying != null && nowPlaying.Equals(music))
            return;
        if (audioSource == null)
            audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        StopMusic();
        music.Play(audioSource);
        nowPlaying = music;
    }
    public void StopMusic() {
        nowPlaying = null;
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
        if (scene == null)
            return;
        if (GameManager.settings.musicOn) {
            if (sceneTracks.ContainsKey(scene.name)) {
                PlayMusic(sceneTracks[scene.name]);
            } else {
                StopMusic();
            }
        } else {
            StopMusic();
        }
        lastLoadedSceneName = scene.name;
    }
    public void PlayTrack(MusicTrack track) {
        MusicSingle music = new MusicSingle(track);
        PlayMusic(music);
    }
}
