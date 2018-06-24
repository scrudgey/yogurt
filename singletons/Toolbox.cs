using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class ExtensionMethods {
    public static TKey FindKeyByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value) {
        TKey returnkey = default(TKey);
        foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            if (value.Equals(pair.Value)) returnkey = pair.Key;
        return returnkey;
    }
}

public class Toolbox : Singleton<Toolbox> {
    protected Toolbox() { } // guarantee this will be always a singleton only - can't use the constructor!
    private CameraControl cameraControl;
    private GameObject tom;
    public int numberOfLiveSpeakers;
    static public T GetOrCreateComponent<T>(GameObject g) where T : Component {
        T component = g.GetComponent<T>();
        if (component) {
            return component;
        } else {
            component = g.AddComponent<T>();
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
    // TODO: fix mispelling
    public void OccurenceFlag(GameObject spawner, OccurrenceData data) {
        GameObject flag = Instantiate(Resources.Load("OccurrenceFlag"), spawner.transform.position, Quaternion.identity) as GameObject;
        Occurrence occurrence = flag.GetComponent<Occurrence>();
        occurrence.data.Add(data);
        data.CalculateDescriptions();
    }
    public EventData DataFlag(GameObject spawner, float chaos = 0, float disgusting = 0, float disturbing = 0, float offensive = 0, float positive = 0) {
        GameObject flag = Instantiate(Resources.Load("OccurrenceFlag"), spawner.transform.position, Quaternion.identity) as GameObject;
        Occurrence occurrence = flag.GetComponent<Occurrence>();
        OccurrenceData data = new OccurrenceData();
        occurrence.data.Add(data);

        EventData eventData = new EventData(chaos: chaos, disgusting: disgusting, disturbing: disturbing, offensive: offensive, positive: positive);
        data.events.Add(eventData);
        return eventData;
    }
    public AudioSource SetUpAudioSource(GameObject g) {
        AudioSource source = g.GetComponent<AudioSource>();
        if (!source) {
            source = g.AddComponent<AudioSource>();
        }
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        // source.minDistance = 0.4f;
        source.minDistance = 1f;
        source.maxDistance = 5.42f;
        source.spatialBlend = 1;
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
        speakerSource.Play();
        DestroyOnceQuiet doq = speaker.GetComponent<DestroyOnceQuiet>();
        doq.instantiatedByToolbox = true;
        numberOfLiveSpeakers += 1;
    }
    ///<summary>
    ///Spawn a droplet of liquid l at poisition pos.
    ///</summary>
    public GameObject SpawnDroplet(Vector3 pos, Liquid l) {
        /// this is a test
        Vector2 initialVelocity = Vector2.zero;
        initialVelocity = UnityEngine.Random.insideUnitCircle;
        if (initialVelocity.y < 0)
            initialVelocity.y = initialVelocity.y * -1;
        return SpawnDroplet(pos, l, initialVelocity);
    }
    public GameObject SpawnDroplet(Vector3 pos, Liquid l, Vector3 initialVelocity) {
        GameObject droplet = Instantiate(Resources.Load("droplet"), pos, Quaternion.identity) as GameObject;
        PhysicalBootstrapper phys = droplet.GetComponent<PhysicalBootstrapper>();
        phys.initHeight = pos.z;
        phys.impactsMiss = true;
        phys.initVelocity = initialVelocity;
        Liquid.MonoLiquidify(droplet, l);
        return droplet;
    }
    public GameObject SpawnDroplet(Liquid l, float severity, GameObject spiller) {
        return SpawnDroplet(l, severity, spiller, 0.01f);
    }
    public GameObject SpawnDroplet(Liquid l, float severity, GameObject spiller, float initHeight, bool noCollision=true) {
        Vector3 initialVelocity = Vector2.zero;
        Vector3 randomVelocity = Vector2.zero;
        randomVelocity = spiller.transform.right * UnityEngine.Random.Range(-0.2f, 0.2f);

        initialVelocity.x = spiller.transform.up.x * UnityEngine.Random.Range(0.8f, 1.3f);
        initialVelocity.z = UnityEngine.Random.Range(severity, 0.2f + severity);
        initialVelocity.x += randomVelocity.x;
        initialVelocity.z += randomVelocity.y;

        GameObject droplet = Instantiate(Resources.Load("droplet"), transform.position, Quaternion.identity) as GameObject;
        PhysicalBootstrapper phys = droplet.GetComponent<PhysicalBootstrapper>();
        phys.impactsMiss = true;
        Vector2 initpos = spiller.transform.position;
        Physical pb = spiller.GetComponentInParent<Physical>();
        if (pb != null) {
            initHeight += pb.height;
        } else {
            Inventory holderInv = spiller.GetComponentInParent<Inventory>();
            if (holderInv){
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
    public float ProperAngle(float x, float y) {
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        if (angle < 0)
            angle += 360;
        return angle;
    }
    public string CloneRemover(string input) {
        string output = input;
        if (input != null) {
            Regex cloneFinder = new Regex(@"(.+)\(Clone\)$", RegexOptions.Multiline);
            MatchCollection matches = cloneFinder.Matches(input);
            if (matches.Count > 0) {                                    // the object is a clone, capture just the normal name
                foreach (Match match in matches) {
                    output = match.Groups[1].Value;
                }
            }
        }
        return output;
    }
    public string GetName(GameObject obj) {
        // TODO: include extra description, like "vomited up"
        // possibly also use intrinsics
        // use player name, speech name
        string nameOut = "";
        if (obj == null) {
            Debug.Log("null name");
            return nameOut;
        }
        Item item = obj.GetComponent<Item>();
        if (item) {
            nameOut = item.itemName;
        } else {
            nameOut = obj.name;
        }
        Edible edible = obj.GetComponent<Edible>();
        if (edible) {
            if (edible.vomit)
                nameOut = "vomited-up " + nameOut;
        }
        Speech speech = obj.GetComponent<Speech>();
        if (speech) {
            nameOut = speech.name;
        }
        nameOut = CloneRemover(nameOut);
        return nameOut;
    }
    public void SendMessage(GameObject host, Component messenger, Message message, bool sendUpwards = true) {
        message.messenger = messenger;
        HashSet<MessageRouter> routers = new HashSet<MessageRouter>(host.GetComponentsInChildren<MessageRouter>());
        if (sendUpwards) {
            foreach (MessageRouter superRouter in host.GetComponentsInParent<MessageRouter>()) {
                routers.Add(superRouter);
            }
        }
        foreach (MessageRouter router in routers) {
            router.ReceiveMessage(message);
        }
    }
    public static void RegisterMessageCallback<T>(Component component, Action<T> handler) where T: Message {
        MessageRouter router = GetOrCreateComponent<MessageRouter>(component.gameObject);
        router.Subscribe<T>(handler);
    }
    public void AddLiveBuffs(GameObject host, GameObject donor) {
        Intrinsics hostIntrins = GetOrCreateComponent<Intrinsics>(host);
        Intrinsics donorIntrins = GetOrCreateComponent<Intrinsics>(donor);
        hostIntrins.CreateLiveBuffs(donorIntrins.buffs);
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
}
