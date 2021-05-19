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
    satansthone,
    gravy,
    antimayor,
    ending,
    shiva,
}
[System.Serializable]
public class Track : IEquatable<Track> {
    public TrackName trackName;
    public float playTime;
    public AudioClip clip {
        get {
            return MusicController.Instance.GetTrack(trackName);
        }
        set { }
    }
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
    public List<TrackName> trackNames;
    public AudioSource audioSource;
    public IEnumerator coroutine;
    public IEnumerator PlayCoroutine(Track track) {
        if (track.loop) {
            while (true) {
                track.playTime += Time.unscaledDeltaTime;
                yield return null;
            }
        } else {
            while (track.playTime < track.clip.length) {
                track.playTime += Time.unscaledDeltaTime;
                yield return null;
            }
            StopMusic();
            yield return null;
        }
    }
    public Music(Track track) {
        this.tracks = new Stack<Track>();
        this.trackNames = new List<TrackName>();
        this.audioSource = MusicController.Instance.audioSource;
        AddTrack(track);
    }
    public Music(IEnumerable<Track> tracks) {
        this.tracks = new Stack<Track>();
        this.trackNames = new List<TrackName>();
        this.audioSource = MusicController.Instance.audioSource;
        foreach (Track track in tracks) {
            AddTrack(track);
        }
    }
    void AddTrack(Track track) {
        tracks.Push(track);
        trackNames.Add(track.trackName);
    }
    public void Play() {
        if (tracks.Count == 0)
            return;
        Track track = tracks.Peek();
        // Debug.Log($"Music play track {track.trackName}");
        if (track.trackName == TrackName.none) {
            StopMusic();
        }
        // if a track was paused, the coroutine will remain
        if (coroutine == null) {
            // Debug.Log($"setting up coroutine for track: {track.trackName}");
            coroutine = PlayCoroutine(track);
        }
        MusicController.Instance.StartCoroutine(coroutine);
        // Debug.Log($"{track.playTime} | {track.playTime % audioSource.clip.length}");
        audioSource.clip = track.clip;
        audioSource.volume = track.volume;
        audioSource.time = track.playTime % track.clip.length;
        audioSource.loop = track.loop;
        audioSource.Play();
    }
    void StopMusic() {
        // Debug.Log("Music stop");
        Pause();
        coroutine = null;
        tracks.Pop();
        if (tracks.Count > 0) {
            Play();
        } else {
            MusicController.Instance.End();
        }
    }
    public void Pause() {
        // Debug.Log("Music pause");
        audioSource.Stop();
        if (coroutine != null) {
            MusicController.Instance.StopCoroutine(coroutine);
        }
    }
    public bool Equality(Music music2) {
        // if (tracks.Count != music2.tracks.Count)
        //     return false;
        HashSet<TrackName> tracks1 = new HashSet<TrackName>(trackNames);
        HashSet<TrackName> tracks2 = new HashSet<TrackName>(music2.trackNames);
        tracks1.ExceptWith(tracks2);
        // Debug.Log(tracks1.Count);
        return tracks1.Count == 0;
    }
}
public class MusicController : Singleton<MusicController> {
    // ideally, should be a FSM with play, pause, enqueue

    static Dictionary<TrackName, string> trackFiles = new Dictionary<TrackName, string>{
        {TrackName.mainTitle, "TITLE Screen LOOP Ver#3 synth bass YC3 2021"},
        {TrackName.apartment, "MAIN LOOP YC3 Ver#3 loop there it is YC3 2021"},
        {TrackName.creepyAmbient, "ForestMoon Alternate Loop YC3"},
        {TrackName.lithophone, "Lithophone LOOP REMIX YC3"},
        {TrackName.jingle, "Yogurt JINGLE Ver. #7 Jingle All The Way YC3 2020"},
        {TrackName.moonCave, "Moon Cave Loop YC3"},
        {TrackName.moonWarp, "Moon WARP Loop YC3"},
        {TrackName.fillerBeat, "Filler Beat YC3"},
        {TrackName.tweakDelay, "tweakDelay YC3"},
        {TrackName.chela, "Chela Theme YC3"},
        {TrackName.dracIntro, "Dracula Mansion INTRO Ver#5 Hi _ Lo YC3 2021"},
        {TrackName.dracLoop, "Dracula Mansion LOOP Ver#5 Hi _ Lo YC3 2021"},
        {TrackName.itemAcquired, "Item Acquisition Ver#2 - No One Suspects The Acquisition YC3 2020"},
        {TrackName.congrats, "Short CONGRATS YC3"},
        {TrackName.mayor, "Mayor_s House #3 Housin_ Thangs YC3 2020"},
        {TrackName.mayor_attic, "Mayor's ATTIC #1 YC3"},
        {TrackName.tv_r2d2, "Vampire Assassins Ver#6 LOOP YC3 2021"},
        {TrackName.mountain, "Mountain Ambience #2 YC3 2020"},
        {TrackName.greaser, "Greaser Theme #5 ALL IN -- YC3 2020"},
        {TrackName.imp, "IMP's Theme Draft #11 CABBAGEPATCH ADAMS _ YC3 2020"},
        {TrackName.venus, "VENUS Theme1 YC3"},
        {TrackName.creeptunnel, "Creep Tunnels Draft #5 YESSIR"},
        {TrackName.lowerhell, "Lower HELL Draftz Test #5 YC3 2020"},
        {TrackName.satansthone, "Final SATAN VER#7 HAIL SATAN YC3 2020"},
        {TrackName.gravy, "Scram Gravy Commercial Ver#9 STEREO SLOP YC3 2021"},
        {TrackName.antimayor, "ANTI MAYOR Ver#6 arriba abajo al lado al centro YC3 2021"},
        {TrackName.ending, "ENDING THEME Ver#6 alls well what ends well YC3 2021"},
        {TrackName.shiva, "SHIVA Mountain Theme Ver#4 YC3 2021"},
    };

    // TODO: add studio
    Dictionary<string, Func<Music>> sceneMusic = new Dictionary<string, Func<Music>>() {
        // {"title", () => new MusicTitle()},
        {"chamber", () => new Music(new Track(TrackName.lithophone))}, // MusicChamber
        {"space", () => new Music(new Track(TrackName.moonWarp))}, // MusicSpace
        {"portal", () => new Music(new Track(TrackName.moonWarp))},
        {"portal_venus", () => new Music(new Track(TrackName.moonWarp))},
        {"portal_hell", () => new Music(new Track(TrackName.moonWarp))},
        {"portal_magic", () => new Music(new Track(TrackName.moonWarp))},
        {"moon1", () => new Music(new Track(TrackName.moonCave))}, // MusicMoon
        {"moon_pool", () => new Music(new Track(TrackName.moonCave))},
        {"moon_town", () => new Music(new Track(TrackName.moonCave))},
        {"moon_cave", () => new Music(new Track(TrackName.moonCave))},
        // {"anti_mayors_house",() => new MusicMoon()},
        {"tower", () => new Music(new Track(TrackName.antimayor))}, // MusicAntiMayor
        {"boardroom_cutscene", () => new Music(new Track(TrackName.fillerBeat))}, // MusicBeat
        // {"morning_cutscene", musicBeat},
        {"forest", () => new Music(new Track(TrackName.creepyAmbient))}, // MusicCreepy
        {"woods", () => new Music(new Track(TrackName.creepyAmbient))},
        {"potion", () => new Music(new Track(TrackName.imp, vol: 0.75f))}, // MusicImp
        {"vampire_house", () => new Music(new List<Track> {
            new Track(TrackName.dracLoop, vol:0.5f),
            new Track(TrackName.dracIntro, loop: false, vol:0.5f)})
        }, // MusicVamp
        {"dungeon", () => new Music(new List<Track> {
            new Track(TrackName.dracLoop, vol:0.5f),
            new Track(TrackName.dracIntro, loop: false, vol:0.5f)})
        },
        {"house", () => new Music(new Track(TrackName.apartment, vol: 0.5f))}, // MusicApartment
        {"apartment", () => new Music(new Track(TrackName.apartment, vol: 0.5f))},
        {"neighborhood", () => new Music(new Track(TrackName.apartment, vol: 0.5f))},
        {"mayors_house", () => new Music(new Track(TrackName.mayor))}, // MusicMayor
        {"mayors_attic", () => new Music(new Track(TrackName.mayor_attic))}, // MusicMayorAttic
        {"cave1", () => new Music(new Track(TrackName.mayor_attic))},
        {"cave2", () => new Music(new Track(TrackName.mayor_attic))},
        {"cave3", () => new Music(new Track(TrackName.mayor_attic))},
        {"cave4", () => new Music(new Track(TrackName.mayor_attic))},
        // {"anti_mayor_cutscene", () => new MusicMayorAttic()},
        {"mountain", () => new Music(new Track(TrackName.mountain, vol: 6f))}, // MusicMountain
        {"volcano", () => new Music(new Track(TrackName.mountain, vol: 6f))},
        {"venus1", () => new Music(new Track(TrackName.venus))}, // MusicVenus
        {"venus_temple", () => new Music(new Track(TrackName.venus))},
        {"hells_kitchen", () => new Music(new Track(TrackName.creeptunnel))},
        {"hells_landing", () => new Music(new Track(TrackName.creeptunnel))},
        {"lower_hell", () => new Music(new Track(TrackName.lowerhell, vol: 0.5f))}, // MusicHell
        {"devils_throneroom", () => new Music(new Track(TrackName.satansthone))}, // MusicThroneroom
        {"office_cutscene", () => new Music(new Track(TrackName.fillerBeat) )}, // MusicBeat
        {"airport_cutscene", () => new Music(new Track(TrackName.fillerBeat) )},
        {"hallucination", () => new Music(new Track(TrackName.mountain, vol: 6f))},
        {"boardroom", () => new Music(new Track(TrackName.fillerBeat) )},
        {"office", () => new Music(new Track(TrackName.fillerBeat) )},
        {"bar", () => new Music(new Track(TrackName.fillerBeat) )},
        {"gravy_studio", () => new Music(new Track(TrackName.gravy))}, // MusicGravy
        {"anti_mayor_cutscene", () => new Music(new Track(TrackName.antimayor))}, // MusicAntiMayor
        {"anti_mayors_house", () => new Music(new Track(TrackName.antimayor))},
        {"hells_chamber", () => new Music(new Track(TrackName.antimayor))},
    };
    public static Dictionary<TrackName, AudioClip> tracks = new Dictionary<TrackName, AudioClip>();
    public AudioClip GetTrack(TrackName trackName) {
        if (tracks.ContainsKey(trackName)) {
            return tracks[trackName];
        } else {
            // Debug.Log($"quickloading {trackName}");
            AudioClip clip = Resources.Load("music/" + trackFiles[trackName]) as AudioClip;
            tracks[trackName] = clip;
            return clip;
        }
    }
    public Camera cam;
    public AudioSource audioSource;
    public Stack<Music> stack = new Stack<Music>();
    public void Awake() {
        SetCamera();
        audioSource = Toolbox.GetOrCreateComponent<AudioSource>(gameObject);
        AudioMixer mixer = Resources.Load("mixers/MusicMixer") as AudioMixer;
        audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
        StartCoroutine(LoadMusic());
    }
    IEnumerator LoadMusic() {
        float time1 = Time.realtimeSinceStartup;
        foreach (KeyValuePair<TrackName, string> kvp in trackFiles) {
            // Debug.Log("loading music/" + kvp.Value + " ...");
            var request = Resources.LoadAsync("music/" + kvp.Value);
            while (!request.isDone) {
                yield return request;
            }
            tracks[kvp.Key] = request.asset as AudioClip;
        }
        float time2 = Time.realtimeSinceStartup;
        Debug.Log($"time to load music: {time2 - time1}");
    }
    public void Update() {
        if (cam != null)
            transform.position = cam.transform.position;
    }
    public void SetCamera() {
        cam = GameObject.FindObjectOfType<Camera>();
    }
    public void SceneChange(string sceneName) {
        SetCamera();
        if (sceneMusic.ContainsKey(sceneName)) {
            Music newMusic = sceneMusic[sceneName]();
            SetMusic(newMusic);
        }
    }
    /* clear currently music stack and start the provided music */
    public void SetMusic(Music newMusic) {
        // if (nowPlayingMusic != null) {
        //     Debug.Log(MusicEquality(newMusic, nowPlayingMusic));
        //     Debug.Log($"{newMusic.GetType()} {nowPlayingMusic.GetType()}");
        // }
        if (stack.Count == 0 || !newMusic.Equality(stack.Peek())) {
            StopMusic();
            stack = new Stack<Music>();
            stack.Push(newMusic);
            Play();
        }
    }
    public void StopMusic() {
        if (stack.Count == 0)
            return;
        stack.Peek().Pause();
    }
    public void RestartMusic() {
        // if there is music in the stack, restart it
        if (stack.Count > 0) {
            stack.Peek().Play();
        } else {
            SceneChange(SceneManager.GetActiveScene().name);
        }
    }
    public void EnqueueMusic(Music music) {
        if (stack.Count > 0)
            stack.Peek().Pause();
        stack.Push(music);
        Play();
    }
    public void End() {
        // Debug.Log("controller end music");
        if (stack.Count > 0)
            stack.Peek().Pause();
        stack.Pop();
        if (stack.Count > 0) {
            Play();
        }
    }
    public void Play() {
        // Debug.Log($"Controller play. stack size: {stack.Count}");
        if (stack.Count > 0 && GameManager.Instance.GetMusicState()) {
            // Debug.Log($"playing music that starts with track {stack.Peek().trackNames[0]}");
            stack.Peek().Play();
        }
    }
    private IEnumerator endRoutine(float time) {
        yield return new WaitForSecondsRealtime(time);
        End();
    }

}