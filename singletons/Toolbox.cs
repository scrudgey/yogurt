using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Audio;
public enum SkinColor {
    light,
    dark,
    darker,
    undead,
    clown,
    demon
}
public enum Gender {
    male,
    female
}
public static class ExtensionMethods {
    public static TKey FindKeyByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value) {
        TKey returnkey = default(TKey);
        foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            if (value.Equals(pair.Value)) returnkey = pair.Key;
        return returnkey;
    }
}

public partial class Toolbox : Singleton<Toolbox> {
    protected Toolbox() { } // guarantee this will be always a singleton only - can't use the constructor!
    private CameraControl cameraControl;
    private GameObject tom;
    public int numberOfLiveSpeakers;
    static Regex doublePunctuationRegex = new Regex(@"[/./?!][/./?!]");
    static Regex cloneFinder = new Regex(@"(.+)\(Clone\)$", RegexOptions.Multiline);
    static Regex underScoreFinder = new Regex(@"_", RegexOptions.Multiline);
    AudioMixer sfxMixer;
    void Start() {
        sfxMixer = Resources.Load("mixers/SoundEffectMixer") as AudioMixer;
    }
    static public T GetOrCreateComponent<T>(GameObject g) where T : Component {
        T component = g.GetComponent<T>();
        if (component) {
            return component;
        } else {
            component = g.AddComponent<T>();
            MySaver.AddComponentToPersistent(g, component);
            return component;
        }
    }
    public static int LevenshteinDistance(string s, string t) {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];
        if (n == 0)
            return m;
        if (m == 0)
            return n;
        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }
        for (int i = 1; i <= n; i++) {
            for (int j = 1; j <= m; j++) {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }
    public static float RGBDistance(Color x, Color y) {
        return Math.Abs(x.r - y.r) + Math.Abs(x.g - y.g) + Math.Abs(x.b - y.b);
    }
    public static float Gompertz(float x, float a = 1, float b = 1, float c = 1) {
        return (float)(a * Math.Exp(b * Math.Exp(-c * x)));
    }
    public static void ShuffleArray<T>(T[] array) {
        int n = array.Length;
        for (int i = 0; i < n; i++) {
            // Pick a new index higher than current for each item in the array
            int r = i + UnityEngine.Random.Range(0, n - i);

            // Swap item into new spot
            T t = array[r];
            array[r] = array[i];
            array[i] = t;
        }
    }
    public static string UppercaseFirst(string s) {
        if (string.IsNullOrEmpty(s)) {
            return string.Empty;
        }
        char[] a = s.ToCharArray();
        a[0] = char.ToUpper(a[0]);
        return new string(a);
    }
    static public Vector3 RandomPointInBox(Bounds bounds, Vector2 target, float radius = 1.5f) {
        int tries = 0;
        Vector3 pos = bounds.center + new Vector3(
           (UnityEngine.Random.value - 0.5f) * bounds.size.x,
           (UnityEngine.Random.value - 0.5f) * bounds.size.y,
           (UnityEngine.Random.value - 0.5f) * bounds.size.z
        );
        while (tries < 20 && Vector2.Distance(target, pos) > radius) {
            tries++;
            pos = bounds.center + new Vector3(
                (UnityEngine.Random.value - 0.5f) * bounds.size.x,
                (UnityEngine.Random.value - 0.5f) * bounds.size.y,
                (UnityEngine.Random.value - 0.5f) * bounds.size.z
            );
        }
        return pos;
    }
    public static string RemoveExtraPunctuation(string s) {
        if (s.Length < 2)
            return s;
        string lastTwo = s.Substring(s.Length - 2);
        while (doublePunctuationRegex.IsMatch(lastTwo)) {
            s = s.Remove(s.Length - 1);
            lastTwo = s.Substring(s.Length - 2);
        }
        return s;
    }
    public void OccurenceFlag(GameObject spawner, EventData data) {
        GameObject flag = Instantiate(Resources.Load("OccurrenceFlag"), spawner.transform.position, Quaternion.identity) as GameObject;
        GameObject noise = GameObject.Instantiate(Resources.Load("NoiseFlag"), spawner.transform.position, Quaternion.identity) as GameObject;
        Occurrence occurrence = flag.GetComponent<Occurrence>();
        Occurrence noiseOccurrence = noise.GetComponent<Occurrence>();
        OccurrenceData occurrenceData = new OccurrenceEvent(data) {
            involved = new HashSet<GameObject>() { spawner }
        };
        occurrence.data = occurrenceData;
        noiseOccurrence.data = occurrenceData;
        occurrenceData.CalculateDescriptions();
    }
    public void OccurenceFlag(GameObject spawner, OccurrenceData data) {
        GameObject flag = Instantiate(Resources.Load("OccurrenceFlag"), spawner.transform.position, Quaternion.identity) as GameObject;
        GameObject noise = GameObject.Instantiate(Resources.Load("NoiseFlag"), spawner.transform.position, Quaternion.identity) as GameObject;
        Occurrence occurrence = flag.GetComponent<Occurrence>();
        Occurrence noiseOccurrence = noise.GetComponent<Occurrence>();
        occurrence.data = data;
        noiseOccurrence.data = data;
        data.CalculateDescriptions();
    }
    public EventData DataFlag(GameObject spawner, string noun, string whatHappened, float chaos = 0, float disgusting = 0, float disturbing = 0, float offensive = 0, float positive = 0) {
        GameObject flag = Instantiate(Resources.Load("OccurrenceFlag"), spawner.transform.position, Quaternion.identity) as GameObject;
        Occurrence occurrence = flag.GetComponent<Occurrence>();

        EventData eventData = new EventData(chaos: chaos, disgusting: disgusting, disturbing: disturbing, offensive: offensive, positive: positive);
        eventData.noun = noun;
        eventData.whatHappened = whatHappened;
        OccurrenceEvent data = new OccurrenceEvent(eventData) {
            involved = new HashSet<GameObject>() { spawner }
        };
        data.CalculateDescriptions();

        occurrence.data = data;

        return eventData;
    }
    public AudioSource SetUpAudioSource(GameObject g) {
        AudioSource source = g.GetComponent<AudioSource>();
        if (!source) {
            source = g.AddComponent<AudioSource>();
        }
        if (sfxMixer == null) {
            sfxMixer = Resources.Load("mixers/SoundEffectMixer") as AudioMixer;
        }
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        // source.minDistance = 0.4f;
        source.minDistance = 1f;
        source.maxDistance = 5.42f;
        source.spatialBlend = 1;

        source.outputAudioMixerGroup = sfxMixer.FindMatchingGroups("Master")[0];
        return source;
    }
    public void AudioSpeaker(string clipName, Vector3 position) {
        if (numberOfLiveSpeakers > 10)
            return;
        AudioClip clip = Resources.Load("sounds/" + clipName, typeof(AudioClip)) as AudioClip;
        AudioSpeaker(clip, position);
    }
    public void AudioSpeaker(AudioClip clip, Vector3 position) {
        if (numberOfLiveSpeakers > 10)
            return;
        GameObject speaker = Instantiate(Resources.Load("Speaker"), position, Quaternion.identity) as GameObject;
        AudioSource speakerSource = speaker.GetComponent<AudioSource>();
        speakerSource.clip = clip;
        speakerSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
        speakerSource.Play();
        DestroyOnceQuiet doq = speaker.GetComponent<DestroyOnceQuiet>();
        doq.instantiatedByToolbox = true;
        numberOfLiveSpeakers += 1;
    }
    ///<summary>
    ///Spawn a droplet of liquid l at poisition pos.
    ///</summary>
    public GameObject SpawnDroplet(Vector3 pos, Liquid l) {
        // Debug.Log(l == null);
        /// this is a test
        Vector2 initialVelocity = Vector2.zero;
        initialVelocity = UnityEngine.Random.insideUnitCircle;
        if (initialVelocity.y < 0)
            initialVelocity.y = initialVelocity.y * -1;
        return SpawnDroplet(pos, l, initialVelocity);
    }
    public GameObject SpawnDroplet(Vector3 pos, Liquid l, Vector3 initialVelocity) {
        // Debug.Log(l == null);
        GameObject droplet = Instantiate(Resources.Load("prefabs/droplet"), pos, Quaternion.identity) as GameObject;
        PhysicalBootstrapper phys = droplet.GetComponent<PhysicalBootstrapper>();
        phys.initHeight = pos.z;
        phys.impactsMiss = true;
        phys.initVelocity = initialVelocity;
        phys.silentImpact = true;
        Liquid.MonoLiquidify(droplet, l);
        return droplet;
    }
    public GameObject SpawnDroplet(Liquid l, float severity, GameObject spiller) {
        // Debug.Log(l == null);
        return SpawnDroplet(l, severity, spiller, 0.01f);
    }
    public GameObject SpawnDroplet(Liquid l, float severity, GameObject spiller, float initHeight, bool noCollision = true) {
        // Debug.Log(l == null);
        Vector3 initialVelocity = Vector2.zero;
        Vector3 randomVelocity = spiller.transform.right * UnityEngine.Random.Range(-0.2f, 0.2f);
        initialVelocity.x = spiller.transform.up.x * UnityEngine.Random.Range(0.8f, 1.3f);
        initialVelocity.z = UnityEngine.Random.Range(severity, 0.2f + severity);
        initialVelocity.x += randomVelocity.x;
        initialVelocity.z += randomVelocity.y;
        return SpawnDroplet(l, severity, spiller, initHeight, initialVelocity, noCollision: noCollision);
    }
    public GameObject SpawnDroplet(Liquid l, float severity, GameObject spiller, float initHeight, Vector3 initVelocity, bool noCollision = true) {
        // Debug.Log(l == null);
        Vector3 initialVelocity = Vector2.zero;
        if (initVelocity == Vector3.zero) {
            Vector3 randomVelocity = spiller.transform.right * UnityEngine.Random.Range(-0.2f, 0.2f);
            initialVelocity.x = spiller.transform.up.x * UnityEngine.Random.Range(0.8f, 1.3f);
            initialVelocity.z = UnityEngine.Random.Range(severity, 0.2f + severity);
            initialVelocity.x += randomVelocity.x;
            initialVelocity.z += randomVelocity.y;
        } else {
            initialVelocity = initVelocity;
        }

        GameObject droplet = Instantiate(Resources.Load("prefabs/droplet"), transform.position, Quaternion.identity) as GameObject;
        PhysicalBootstrapper phys = droplet.GetComponent<PhysicalBootstrapper>();
        phys.impactsMiss = true;
        Vector2 initpos = spiller.transform.position;
        Physical pb = spiller.GetComponentInParent<Physical>();
        if (pb != null) {
            initHeight += pb.height;
        } else {
            Inventory holderInv = spiller.GetComponentInParent<Inventory>();
            if (holderInv && initHeight != 0) {
                initHeight += holderInv.dropHeight;
            }
        }
        droplet.transform.position = initpos;
        phys.noCollisions = noCollision;
        phys.doInit = false;
        phys.InitPhysical(initHeight, initialVelocity);
        phys.physical.StartFlyMode();
        Collider2D[] spillerColliders = spiller.transform.root.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in spillerColliders) {
            Physics2D.IgnoreCollision(collider, phys.physical.objectCollider, true);
            Physics2D.IgnoreCollision(collider, phys.physical.horizonCollider, true);
            if (phys.physical.groundCollider)
                Physics2D.IgnoreCollision(collider, phys.physical.groundCollider, true);
        }
        Liquid.MonoLiquidify(droplet, l);
        return droplet;
    }
    public Component CopyComponent(Component original, GameObject destination) {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        // Copied fields can be restricted with BindingFlags
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields) {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy;
    }
    public Vector2 RandomVector(Vector2 baseDir, float angleSpread) {
        float baseAngle = (float)Mathf.Atan2(baseDir.y, baseDir.x);
        float spreadRads = angleSpread * Mathf.Deg2Rad;
        float newAngle = baseAngle + UnityEngine.Random.Range(-1f * spreadRads, 1f * spreadRads);
        return new Vector2(baseDir.magnitude * Mathf.Cos(newAngle), baseDir.magnitude * Mathf.Sin(newAngle));
    }
    public Vector2 RotateZ(Vector2 v, float angle) {
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);

        float tx = v.x;
        float ty = v.y;

        v.x = (cos * tx) - (sin * ty);
        v.y = (cos * ty) + (sin * tx);

        return v;
    }
    public static List<T> Shuffle<T>(List<T> list) {
        System.Random random = new System.Random();
        List<T> newList = new List<T>(list);
        int n = newList.Count;

        for (int i = newList.Count - 1; i > 1; i--) {
            int rnd = random.Next(i + 1);

            T value = newList[rnd];
            newList[rnd] = newList[i];
            newList[i] = value;
        }
        return newList;
    }
    public float ProperAngle(float x, float y) {
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        if (angle < 0)
            angle += 360;
        return angle;
    }
    public string CloneRemover(string input) {
        string output = input;
        if (input != null) {

            MatchCollection matches = cloneFinder.Matches(input);
            if (matches.Count > 0) {                                    // the object is a clone, capture just the normal name
                foreach (Match match in matches) {
                    output = match.Groups[1].Value;
                }
            }
            // TODO: numbermatcher
        }
        return output;
    }
    public string UnderscoreRemover(string input) {
        return underScoreFinder.Replace(input, " ");
    }
    // public string GetReferent(GameObject obj) {
    //     Item item = obj.GetComponent<Item>();
    //     if (item && item.referent != "") {
    //         return item.referent;
    //     }
    //     Speech speech = obj.GetComponent<Speech>();
    //     if (speech && speech.referent != "") {
    //         return speech.referent;
    //     }
    //     return "A " + GetName(obj);
    // }
    public string GetName(GameObject obj, bool skipMainCollider = false) {
        // possibly also use intrinsics
        if (obj == null) {
            return "";
        }
        if (obj == GameManager.Instance.playerObject) {
            return GameManager.Instance.saveGameName;
        }
        Duplicatable dup = obj.GetComponent<Duplicatable>();
        if (dup && dup.adoptedName != "") {
            return dup.adoptedName;
        }
        string nameOut = obj.name;

        Item item = obj.GetComponent<Item>();
        if (item) {
            nameOut = item.itemName;
            // if (item.referent != "") {
            //     nameOut = item.referent;
            // } else nameOut = item.itemName;
        }
        LiquidContainer container = obj.GetComponent<LiquidContainer>();
        if (container) {
            nameOut = container.descriptionName;
        }
        Edible edible = obj.GetComponent<Edible>();
        if (edible) {
            if (edible.vomit)
                nameOut = "vomited-up " + nameOut;
        }
        Speech speech = obj.GetComponent<Speech>();
        if (speech) {
            if (speech.speechName != "")
                nameOut = speech.speechName;
            // if (speech.referent != "") {
            //     nameOut = speech.referent;
            // } else if (speech.speechName != "")
            //     nameOut = speech.speechName;
        }
        nameOut = CloneRemover(nameOut);
        nameOut = UnderscoreRemover(nameOut);
        if (new List<String> { "blf", "blm", "brf", "Brm", "Tom" }.Contains(nameOut)) {
            return GameManager.Instance.saveGameName;
        }
        if (nameOut == "mainCollider") {
            Debug.Log("main collider found!");
            if (skipMainCollider) {
                return GetName(obj.transform.parent.gameObject);
            }
        }
        return nameOut;
    }
    public HashSet<MessageRouter> ChildRouters(GameObject host) {
        HashSet<MessageRouter> routers = new HashSet<MessageRouter>(host.GetComponentsInChildren<MessageRouter>());
        Inventory inv = host.GetComponent<Inventory>();
        if (inv) {
            if (inv.holding != null) {
                HashSet<MessageRouter> holdingRouters = new HashSet<MessageRouter>(inv.holding.GetComponentsInChildren<MessageRouter>());
                routers.ExceptWith(holdingRouters);
            }
        }
        return routers;
    }
    public void SendMessage(GameObject host, Component messenger, Message message, bool sendUpwards = true) {
        message.messenger = messenger;
        // TODO: do not propagate all the way to held objects
        HashSet<MessageRouter> routers = ChildRouters(host);
        if (sendUpwards) {
            foreach (MessageRouter superRouter in host.GetComponentsInParent<MessageRouter>()) {
                routers.Add(superRouter);
            }
        }
        foreach (MessageRouter router in routers) {
            router.ReceiveMessage(message);
        }
    }
    public static void RegisterMessageCallback<T>(Component component, Action<T> handler) where T : Message {
        MessageRouter router = GetOrCreateComponent<MessageRouter>(component.gameObject);
        router.Subscribe<T>(handler);
    }
    public void AddLiveBuffs(GameObject host, GameObject donor) {
        Intrinsics hostIntrins = GetOrCreateComponent<Intrinsics>(host);
        Intrinsics donorIntrins = GetOrCreateComponent<Intrinsics>(donor);
        hostIntrins.CreateLiveBuffs(donorIntrins.buffs);
    }
    public void AddPromotedLiveBuffs(GameObject host, GameObject donor) {
        Intrinsics hostIntrins = GetOrCreateComponent<Intrinsics>(host);
        Intrinsics donorIntrins = GetOrCreateComponent<Intrinsics>(donor);
        hostIntrins.CreatePromotedLiveBuffs(donorIntrins.buffs);
    }
    public void AddChildIntrinsics(GameObject host, Component component, GameObject donor) {
        Intrinsics hostIntrins = GetOrCreateComponent<Intrinsics>(host);
        Intrinsics donorIntrins = GetOrCreateComponent<Intrinsics>(donor);
        hostIntrins.AddChild(component, donorIntrins);
    }
    public void RemoveChildIntrinsics(GameObject host, Component component) {
        Intrinsics hostIntrins = GetOrCreateComponent<Intrinsics>(host);
        hostIntrins.RemoveChild(component);
    }
    public string DirectionToString(Vector2 direction) {
        float angle = Toolbox.Instance.ProperAngle(direction.x, direction.y);
        string lastPressed = "right";
        // change lastpressed because this is relevant to animation
        if (angle > 315 || angle < 45) {
            lastPressed = "right";
        } else if (angle >= 45 && angle <= 135) {
            lastPressed = "up";
        } else if (angle >= 135 && angle < 225) {
            lastPressed = "left";
        } else if (angle >= 225 && angle < 315) {
            lastPressed = "down";
        }
        return lastPressed;
    }
    public void DisableAndReenable(MonoBehaviour target, float time) {
        StartCoroutine(EnableAfterSeconds(target, 1f));
    }
    public IEnumerator EnableAfterSeconds(MonoBehaviour target, float time) {
        if (target == null)
            yield break;
        target.enabled = false;
        yield return new WaitForSeconds(time);
        if (target == null)
            yield break;
        target.enabled = true;
        yield return null;
    }
    public void SwitchAudioListener(GameObject target) {
        foreach (AudioListener listener in GameObject.FindObjectsOfType<AudioListener>()) {
            listener.enabled = false;
        }
        AudioListener targetListener = GetOrCreateComponent<AudioListener>(target);
        targetListener.enabled = true;
    }
    public static Texture2D CopyTexture2D(Texture2D copiedTexture, SkinColor skinColor) {
        Dictionary<Color, Color> skinTheme = skinThemes[skinColor];
        Texture2D texture = new Texture2D(copiedTexture.width, copiedTexture.height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        int y = 0;
        while (y < texture.height) {
            int x = 0;
            while (x < texture.width) {
                Color originalColor = copiedTexture.GetPixel(x, y);
                if (skinTheme.ContainsKey(originalColor)) {
                    texture.SetPixel(x, y, skinTheme[originalColor]);
                } else {
                    texture.SetPixel(x, y, originalColor);
                }
                ++x;
            }
            ++y;
        }
        texture.name = (copiedTexture.name + "_" + skinColor.ToString());
        texture.Apply();
        return texture;
    }
    public static Func<TSource1, TSource2, TReturn> Memoize<TSource1, TSource2, TReturn>(Func<TSource1, TSource2, TReturn> func) {
        var cache = new Dictionary<string, TReturn>();
        return (s1, s2) => {
            var key = s1.GetHashCode().ToString() + s2.GetHashCode().ToString();
            if (!cache.ContainsKey(key)) {
                cache[key] = func(s1, s2);
            }
            return cache[key];
        };
    }
    static Dictionary<string, Sprite[]> skinToneCache = new Dictionary<string, Sprite[]>();
    public static Func<string, SkinColor, Sprite[]> MemoizedSkinTone = Memoize<string, SkinColor, Sprite[]>(ApplySkinToneToSpriteSheet);
    public static Sprite[] ApplySkinToneToSpriteSheet(string sheetname, SkinColor skinColor) {
        Sprite[] sprites = Resources.LoadAll<Sprite>("spritesheets/" + sheetname);
        Texture2D modTexture = Toolbox.CopyTexture2D(sprites[0].texture, skinColor);
        for (int i = 0; i < sprites.Length; i++) {
            Sprite sprite = sprites[i];
            sprites[i] = Sprite.Create(
                modTexture,
                sprite.rect,
                new Vector2(0.5f, 0.5f),
                sprite.pixelsPerUnit,
                1,
                SpriteMeshType.Tight,
                sprite.border
                );
            sprites[i].name = modTexture.name;
        }
        return sprites;
    }
    public static Sprite ApplySkinToneToSprite(Sprite sprite, SkinColor skinColor) {
        Texture2D modTexture = Toolbox.CopyTexture2D(sprite.texture, skinColor);
        Sprite newSprite = Sprite.Create(
            modTexture,
            sprite.rect,
            new Vector2(0.5f, 0.5f),
            sprite.pixelsPerUnit,
            1,
            SpriteMeshType.Tight,
            sprite.border
            );
        newSprite.name = modTexture.name;
        return newSprite;
    }
    static Color skinDefault = new Color32(245, 127, 23, 255);
    static Color skinDefaultDark = new Color32(230, 74, 25, 255);
    static Dictionary<Color, Color> skinThemeLight = new Dictionary<Color, Color>(){
        {skinDefault, skinDefault},
        {skinDefaultDark, skinDefaultDark}
    };
    static Dictionary<Color, Color> skinThemeDark = new Dictionary<Color, Color>(){
        {skinDefault, new Color32(146, 108, 49, 255)},
        {skinDefaultDark, new Color32(120, 78, 34, 255)}
    };
    static Dictionary<Color, Color> skinThemeDarker = new Dictionary<Color, Color>(){
        {skinDefault, new Color32(90, 66, 11, 255)},
        {skinDefaultDark, new Color32(77, 45, 10, 255)}
    };
    static Dictionary<Color, Color> skinThemeUndead = new Dictionary<Color, Color>(){
        {skinDefault, new Color32(197, 225, 164, 255)},
        {skinDefaultDark, new Color32(174, 213, 129, 255)}
    };
    static Dictionary<Color, Color> skinThemeClown = new Dictionary<Color, Color>(){
        {skinDefault, new Color32(238, 238, 238, 255)},
        {skinDefaultDark, new Color32(238, 238, 238, 255)}
    };
    static Dictionary<Color, Color> skinThemeDemon = new Dictionary<Color, Color>(){
        {skinDefault, new Color32(196, 20, 17, 255)},
        {skinDefaultDark, new Color32(176, 17, 10, 255)}
    };

    static Dictionary<SkinColor, Dictionary<Color, Color>> skinThemes = new Dictionary<SkinColor, Dictionary<Color, Color>>{
        {SkinColor.light, skinThemeLight},
        {SkinColor.dark, skinThemeDark},
        {SkinColor.darker, skinThemeDarker},
        {SkinColor.undead, skinThemeUndead},
        {SkinColor.clown, skinThemeClown},
        {SkinColor.demon, skinThemeDemon},
    };
    // this could be done with messages
    public static void SetSkinColor(GameObject target, SkinColor color) {
        AdvancedAnimation advancedAnimation = target.GetComponent<AdvancedAnimation>();
        HeadAnimation headAnimation = target.GetComponentInChildren<HeadAnimation>();
        if (advancedAnimation != null) {
            advancedAnimation.skinColor = color;
        }
        if (headAnimation != null) {
            headAnimation.skinColor = color;
        }
    }
    public static void SetGender(GameObject target, Gender gender, bool changeHead = true) {
        // Speech speech = target.GetComponent<Speech>();
        HeadAnimation headAnimation = target.GetComponentInChildren<HeadAnimation>();
        Outfit outfit = target.GetComponent<Outfit>();

        if (changeHead) {
            switch (gender) {
                case Gender.male:
                    headAnimation.spriteSheet = "generic3_head";
                    headAnimation.baseName = "generic3";
                    break;
                case Gender.female:
                    headAnimation.spriteSheet = "girl_head";
                    headAnimation.baseName = "girl";
                    break;
                default:
                    break;
            }
        }
        if (outfit != null) {
            outfit.gender = gender;
        }
    }
    public static Gender GetGender(GameObject target) {
        Outfit outfit = target.GetComponent<Outfit>();
        if (outfit != null) {
            return outfit.gender;
        } else return Gender.male;
    }

    public void deactivateEventually(GameObject target) {
        DestroyAfterTime dat = target.AddComponent<DestroyAfterTime>();
        dat.deactivate = true;
        dat.lifetime = 3f;
    }
}
