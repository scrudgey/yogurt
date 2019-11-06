using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

[System.Serializable]
public enum TrackName {
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
    itemAcquired,
    congrats,
}
[System.Serializable]
public class Track {
    public TrackName trackName;
    public float playTime;
    public bool loop;
    public Track(TrackName trackName, bool loop = true) {
        this.trackName = trackName;
        this.loop = loop;
    }
}
[System.Serializable]
public class Music {
    public Stack<Track> tracks;
    // public Music(string name, List<Track> tracks) {
    //     this.name = name;
    //     this.tracks = new Stack<Track>(tracks);
    // }
}
public class MusicTitle : Music {
    public MusicTitle() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.mainTitle) });
    }
}
public class MusicNone : Music {
    public MusicNone() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.none) });
    }
}
public class MusicChamber : Music {
    public MusicChamber() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.lithophone) });
    }
}
public class MusicCreepy : Music {
    public MusicCreepy() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.creepyAmbient) });
    }
}

public class MusicSpace : Music {
    public MusicSpace() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.moonWarp) });
    }
}
public class MusicMoon : Music {
    public MusicMoon() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.moonCave) });
    }
}
public class MusicBeat : Music {
    public MusicBeat() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.fillerBeat) });
    }
}
public class MusicVamp : Music {
    public MusicVamp() {
        tracks = new Stack<Track>(new List<Track> {
            new Track(TrackName.dracLoop),
            new Track(TrackName.dracIntro, loop: false),
            });
    }
}
public class MusicTweak : Music {
    public MusicTweak() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.tweakDelay) });
    }
}
public class MusicChela : Music {
    public MusicChela() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.chela) });
    }
}
public class MusicItem : Music {
    public MusicItem() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.itemAcquired, loop: false) });
    }
}
public class MusicCongrats : Music {
    public MusicCongrats() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.congrats, loop: false) });
    }
}
public class MusicController : Singleton<MusicController> {

    static Dictionary<TrackName, string> trackFiles = new Dictionary<TrackName, string>(){
        {TrackName.mainTitle, "Main Vamp w keys YC3"},
        {TrackName.creepyAmbient, "ForestMoon Alternate Loop YC3"},
        {TrackName.lithophone, "Lithophone LOOP REMIX YC3"},
        {TrackName.jingle, "jingle1"},
        {TrackName.moonCave, "Moon Cave Loop YC3"},
        {TrackName.moonWarp, "Moon WARP Loop YC3"},
        {TrackName.fillerBeat, "Filler Beat YC3"},
        {TrackName.tweakDelay, "tweakDelay YC3"},
        {TrackName.chela, "Chela Theme YC3"},
        {TrackName.dracIntro, "Dracula Mansion INTRO YC3"},
        {TrackName.dracLoop, "Dracula Mansion LOOP YC3"},
        {TrackName.itemAcquired, "Item Acquisition YC3"},
        {TrackName.congrats, "Short CONGRATS YC3"},
    };


    static Dictionary<string, Func<Music>> sceneMusic = new Dictionary<string, Func<Music>>() {
        {"title", () => new MusicTitle()},
        {"chamber", () => new MusicChamber()},
        {"mayors_attic",() => new MusicCreepy()},
        {"space", () => new MusicSpace()},
        {"moon1", () => new MusicMoon()},
        {"moon_pool", () => new MusicMoon()},
        {"moon_town", () => new MusicMoon()},
        {"moon_cave", () => new MusicMoon()},
        {"anti_mayors_house",() => new MusicMoon()},
        {"boardroom_cutscene", () => new MusicBeat()},
        // {"morning_cutscene", musicBeat},
        {"forest", () => new MusicCreepy()},
        {"woods", () => new MusicCreepy()},
        {"potion", () => new MusicCreepy()},
        {"vampire_house", () => new MusicVamp()},
        {"dungeon", () => new MusicVamp()},
        {"house", () => new MusicNone()},
    };
    public static Dictionary<TrackName, AudioClip> tracks = new Dictionary<TrackName, AudioClip>();
    public Camera cam; // TOOD: make obsolete
    public AudioSource audioSource;
    public Track nowPlayingTrack;
    public Music nowPlayingMusic;
    public Stack<Track> stack = new Stack<Track>();
    public void Awake() {
        SetCamera();
        audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        // load tracks
        foreach (KeyValuePair<TrackName, string> kvp in trackFiles) {
            // Debug.Log("loading music/" + kvp.Value + " ...");
            AudioClip clip = Resources.Load("music/" + kvp.Value) as AudioClip;
            tracks[kvp.Key] = clip;
        }
    }
    public void Update() {
        if (cam != null)
            transform.position = cam.transform.position;
        if (nowPlayingTrack != null) {
            nowPlayingTrack.playTime += Time.unscaledDeltaTime;
        }
    }
    public void SetCamera() {
        cam = GameObject.FindObjectOfType<Camera>();
    }
    public void SceneChange(string sceneName) {
        SetCamera();
        if (sceneMusic.ContainsKey(sceneName)) {
            Music newMusic = sceneMusic[sceneName]();
            SetMusic(newMusic);
        } else {

        }
    }
    public void SetMusic(Music newMusic) {
        // replace queue with music queue, 
        // playNext
        // set music
        if (nowPlayingMusic != null && newMusic.GetType() == nowPlayingMusic.GetType()) {
            return;
        }
        CancelInvoke();
        stack = newMusic.tracks;
        nowPlayingMusic = newMusic;
        PlayNextTrack();
    }
    public void EnqueueMusic(Music music) {
        // Debug.Log("enqueue ");

        // what to do with nowPlayingMusic?
        // if (music == nowPlayingMusic) {
        //     return;
        // }

        while (music.tracks.Count > 0) {
            // Track track = music.tracks.Pop();
            // Debug.Log(track.trackName);
            // stack.Push(track);
            stack.Push(music.tracks.Pop());
            // Debug.Log(stack.Peek().trackName);
        }
        // PAUSE current track
        StopTrack();
        PlayNextTrack();
    }
    public void End() {
        // Debug.Log("end");
        // Debug.Log(nowPlayingTrack.trackName);
        // pop queue
        // stoptrack
        // playNext
        stack.Pop();
        StopTrack();
        PlayNextTrack();
    }
    public void PlayNextTrack() {
        // Debug.Log("PlayNextTrack");
        // if (nowPlayingTrack != null)
        //     Debug.Log(nowPlayingTrack.trackName);
        // stoptrack
        // peek next
        // play track
        StopTrack();
        if (stack.Count > 0)
            PlayTrack(stack.Peek());
    }
    public void PlayTrack(Track track) {
        // Debug.Log("PlayTrack");
        // Debug.Log(track.trackName);
        // if playime > 0, resume
        // else, play from start

        // start audio
        // set nowplaying
        // if not looping, invoke end with timer

        if (track.trackName == TrackName.none)
            return;

        audioSource.clip = tracks[track.trackName];
        audioSource.Play();
        nowPlayingTrack = track;
        audioSource.time = track.playTime;
        audioSource.loop = track.loop;
        if (!track.loop) {
            // TODO: unscaled time
            Invoke("End", audioSource.clip.length);
        }
    }
    public void StopTrack() {
        // Debug.Log("StopTrack");
        // if (nowPlayingTrack != null)
        //     Debug.Log(nowPlayingTrack.trackName);
        // nowPlayingTrack;
        // track is nowplaying
        // set its playtime
        // stop audio
        // remove nowplaying
        if (nowPlayingTrack == null)
            return;
        // nowPlayingTrack.playTime = 1f;
        audioSource.Stop();
        nowPlayingTrack = null;
    }

}
