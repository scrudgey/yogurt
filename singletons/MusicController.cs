using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using UnityEngine.Audio;

[System.Serializable]
public enum TrackName {
    none,
    mainTitle,
    apartment,
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
    mayor,
    mayor_attic,
    tv_r2d2,
    mountain,
    greaser,
    imp,
    venus,
    creeptunnel,
    lowerhell,
    satansthone
}
[System.Serializable]
public class Track : IEquatable<Track> {
    public TrackName trackName;
    public float playTime;
    public bool loop;
    public float volume = 1;
    public Track(TrackName trackName, bool loop = true, float vol = 1) {
        this.trackName = trackName;
        this.loop = loop;
        this.volume = vol;
    }
    public override bool Equals(object obj) {
        return this.Equals(obj as Track);
    }

    public bool Equals(Track p) {
        // If parameter is null, return false.
        if (System.Object.ReferenceEquals(p, null)) {
            return false;
        }

        // Optimization for a common success case.
        if (System.Object.ReferenceEquals(this, p)) {
            return true;
        }

        // If run-time types are not exactly the same, return false.
        if (this.GetType() != p.GetType()) {
            return false;
        }

        // Return true if the fields match.
        // Note that the base class is not invoked because it is
        // System.Object, which defines Equals as reference equality.
        return (p.trackName == trackName);
    }
    public override int GetHashCode() {
        return trackName.GetHashCode();
    }
}
[System.Serializable]
public class Music {
    public Stack<Track> tracks;
}
public class MusicTitle : Music {
    public MusicTitle() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.mainTitle) });
    }
}
public class MusicApartment : Music {
    public MusicApartment() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.apartment, vol: 0.5f) });
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
public class MusicMayor : Music {
    public MusicMayor() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.mayor) });
    }
}
public class MusicMayorAttic : Music {
    public MusicMayorAttic() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.mayor_attic) });
    }
}
public class MusicVamp : Music {
    public MusicVamp() {
        tracks = new Stack<Track>(new List<Track> {
            new Track(TrackName.dracLoop, vol:0.5f),
            new Track(TrackName.dracIntro, loop: false, vol:0.5f),
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
public class MusicTVR2 : Music {
    public MusicTVR2() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.tv_r2d2) });
    }
}
public class MusicMountain : Music {
    public MusicMountain() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.mountain, vol: 6f) });
    }
}
public class MusicImp : Music {
    public MusicImp() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.imp, vol: 0.75f) });
    }
}
public class MusicGreaser : Music {
    public MusicGreaser() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.greaser) });
    }
}
public class MusicVenus : Music {
    public MusicVenus() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.venus) });
    }
}
public class MusicCreep : Music {
    public MusicCreep() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.creeptunnel) });
    }
}
public class MusicHell : Music {
    public MusicHell() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.lowerhell) });
    }
}
public class MusicThroneroom : Music {
    public MusicThroneroom() {
        tracks = new Stack<Track>(new List<Track> { new Track(TrackName.satansthone) });
    }
}
public class MusicController : Singleton<MusicController> {

    static Dictionary<TrackName, string> trackFiles = new Dictionary<TrackName, string>(){
        {TrackName.mainTitle, "Title Screen LOOP #2 YC3 2020"},
        {TrackName.apartment, "Main Vamp w keys YC3"},
        {TrackName.creepyAmbient, "ForestMoon Alternate Loop YC3"},
        {TrackName.lithophone, "Lithophone LOOP REMIX YC3"},
        {TrackName.jingle, "Yogurt JINGLE Ver. #7 Jingle All The Way YC3 2020"},
        {TrackName.moonCave, "Moon Cave Loop YC3"},
        {TrackName.moonWarp, "Moon WARP Loop YC3"},
        {TrackName.fillerBeat, "Filler Beat YC3"},
        {TrackName.tweakDelay, "tweakDelay YC3"},
        {TrackName.chela, "Chela Theme YC3"},
        {TrackName.dracIntro, "Dracula Mansion INTRO YC3"},
        {TrackName.dracLoop, "Dracula Mansion LOOP YC3"},
        {TrackName.itemAcquired, "Item Acquisition Ver#2 - No One Suspects The Acquisition YC3 2020"},
        {TrackName.congrats, "Short CONGRATS YC3"},
        {TrackName.mayor, "Mayor_s House #3 Housin_ Thangs YC3 2020"},
        {TrackName.mayor_attic, "Mayor's ATTIC #1 YC3"},
        {TrackName.tv_r2d2, "TV-r2d2"},
        {TrackName.mountain, "Mountain Ambience #2 YC3 2020"},
        {TrackName.greaser, "Greaser Theme #5 ALL IN -- YC3 2020"},
        {TrackName.imp, "IMP's Theme Draft #11 CABBAGEPATCH ADAMS _ YC3 2020"},
        {TrackName.venus, "VENUS Theme1 YC3"},
        {TrackName.creeptunnel, "Creep Tunnels Draft #5 YESSIR"},
        {TrackName.lowerhell, "Lower HELL Draftz Test #5 YC3 2020"},
        {TrackName.satansthone, "Final SATAN VER#1 Vox _ No Bass YC3 2020"},
    };

    static Dictionary<string, Func<Music>> sceneMusic = new Dictionary<string, Func<Music>>() {
        // {"title", () => new MusicTitle()},
        {"chamber", () => new MusicChamber()},
        {"space", () => new MusicSpace()},
        {"portal", () => new MusicSpace()},
        {"portal_venus", () => new MusicSpace()},
        {"portal_hell", () => new MusicSpace()},
        {"moon1", () => new MusicMoon()},
        {"moon_pool", () => new MusicMoon()},
        {"moon_town", () => new MusicMoon()},
        {"moon_cave", () => new MusicMoon()},
        {"anti_mayors_house",() => new MusicMoon()},
        {"tower", () => new MusicMoon()},
        {"boardroom_cutscene", () => new MusicBeat()},
        // {"morning_cutscene", musicBeat},
        {"forest", () => new MusicCreepy()},
        {"woods", () => new MusicCreepy()},
        {"potion", () => new MusicImp()},
        {"vampire_house", () => new MusicVamp()},
        {"dungeon", () => new MusicVamp()},
        {"house", () => new MusicApartment()},
        {"apartment", () => new MusicApartment()},
        {"neighborhood", () => new MusicApartment()},
        {"mayors_house", () => new MusicMayor()},
        {"mayors_attic", () => new MusicMayorAttic()},
        {"cave1", () => new MusicMayorAttic()},
        {"cave2", () => new MusicMayorAttic()},
        {"cave3", () => new MusicMayorAttic()},
        {"cave4", () => new MusicMayorAttic()},
        {"anti_mayor_cutscene", () => new MusicMayorAttic()},
        {"gravy_studio", () => new MusicMoon()},
        {"mountain", () => new MusicMountain()},
        {"volcano", () => new MusicMountain()},
        {"venus1", () => new MusicVenus()},
        {"venus_temple", () => new MusicVenus()},
        {"hells_kitchen", () => new MusicCreep()},
        {"hells_landing", () => new MusicCreep()},
        {"lower_hell", () => new MusicHell()},
        {"devils_throneroom", () => new MusicThroneroom()},
        {"office_cutscene", () => new MusicBeat()},
        {"airport_cutscene", () => new MusicBeat()},
        {"hallucination", () => new MusicMountain()},
        {"boardroom", () => new MusicBeat()},
        {"office", () => new MusicBeat()},
        {"bar", () => new MusicBeat()},
    };
    // TODO: add studio
    public static Dictionary<TrackName, AudioClip> tracks = new Dictionary<TrackName, AudioClip>();
    public Camera cam; // TOOD: make obsolete
    public AudioSource audioSource;
    public Track nowPlayingTrack;
    public Music nowPlayingMusic;
    public Stack<Track> stack = new Stack<Track>();
    public Coroutine endCoroutine;
    public void Awake() {
        SetCamera();
        audioSource = Toolbox.GetOrCreateComponent<AudioSource>(gameObject);
        AudioMixer mixer = Resources.Load("mixers/MusicMixer") as AudioMixer;
        audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
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
        // if (nowPlayingMusic != null) {
        //     Debug.Log(MusicEquality(newMusic, nowPlayingMusic));
        //     Debug.Log($"{newMusic.GetType()} {nowPlayingMusic.GetType()}");
        // }
        if (nowPlayingMusic != null && MusicEquality(newMusic, nowPlayingMusic)) {
            return;
        }
        if (endCoroutine != null)
            StopCoroutine(endCoroutine);
        stack = newMusic.tracks;
        nowPlayingMusic = newMusic;
        PlayNextTrack();
    }
    public bool MusicEquality(Music music1, Music music2) {
        if (music1.GetType() != music2.GetType())
            return false;
        if (music1.tracks.Count != music2.tracks.Count)
            return false;
        if (music1.tracks.Count > 0 && music2.tracks.Count > 0)
            if (music1.tracks.Peek().trackName != music2.tracks.Peek().trackName)
                return false;
        return true;
    }
    public void RestartMusic() {
        if (endCoroutine != null)
            StopCoroutine(endCoroutine);
        if (nowPlayingMusic != null) {
            stack = nowPlayingMusic.tracks;
        } else {
            Scene scene = SceneManager.GetActiveScene();
            Music newMusic = sceneMusic[scene.name]();
            SetMusic(newMusic);
        }
        PlayNextTrack();
    }
    public void EnqueueMusic(Music music) {
        while (music.tracks.Count > 0) {
            stack.Push(music.tracks.Pop());
        }
        // PAUSE current track
        StopTrack();
        PlayNextTrack();
    }
    public void End() {
        stack.Pop();
        StopTrack();
        PlayNextTrack();
    }
    public void PlayNextTrack() {
        StopTrack();
        if (stack.Count > 0)
            PlayTrack(stack.Peek());
    }
    public void PlayTrack(Track track) {
        if (!GameManager.Instance.GetMusicState()) {
            StopTrack();
            return;
        }
        if (track.trackName == TrackName.none)
            return;

        audioSource.clip = tracks[track.trackName];
        audioSource.volume = track.volume;
        audioSource.Play();
        nowPlayingTrack = track;
        audioSource.time = track.playTime;
        audioSource.loop = track.loop;
        if (!track.loop) {
            endCoroutine = StartCoroutine(endRoutine(audioSource.clip.length));
        }
    }
    private IEnumerator endRoutine(float time) {
        yield return new WaitForSecondsRealtime(time);
        End();
    }
    public void StopTrack() {
        if (nowPlayingTrack == null)
            return;
        audioSource.Stop();
        nowPlayingTrack = null;
    }
}
