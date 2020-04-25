using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Nimrod;

public class Speech : Interactive, ISaveable {
    public string speechName;
    static string[] swearWords = new string[]{
        @"\bshit\b",
        @"\bfucked\b",
        @"\bfuck\b",
        @"\bfucking\b",
        @"\bshazbotting\b",
        @"\bshazbot\b",
        @"\bpiss\b",
        @"\bdick\b",
        @"\bass\b",
        @"\bcock\b",
        @"\bnutsack\b",
        @"\bbootyhole\b",
        @"\bbitchmade\b",
        @"\bmotherfucker\b",
        @"\basshole"};

    // asshole
    // fix whitespace matching
    static string[] swearWhiteList = new string[]{
        @"-ass",
        @"hole",
        @"sack",
        @"made",
        @"mother",
    };
    static Regex spaceMatcher = new Regex(@"\b");

    public static string ParseGender(string instring) {
        //Use named capturing groups to make life easier
        // var pattern = "(?<label>\"formatter\"): ([\"])(?<tag>.*)([\"])";
        string genderHook = @"\[\[(?<male>.+)\|(?<female>.+)\]\]";

        //Create a substitution pattern for the Replace method
        string replacePattern = "";
        if (GameManager.Instance.playerGender == Gender.male) {
            replacePattern = "${male}";
        } else if (GameManager.Instance.playerGender == Gender.female) {
            replacePattern = "${female}";
        }

        return Regex.Replace(instring, genderHook, replacePattern, RegexOptions.IgnoreCase);
    }
    public static string ProcessDialogue(string phrase, ref List<bool> swearList) {
        StringBuilder sb = new StringBuilder(ParseGender(phrase));
        char censorChar = "∎".ToCharArray()[0];
        foreach (string swear in swearWords) {
            Regex matcher = new Regex(swear);
            foreach (Match match in matcher.Matches(phrase)) {
                for (int i = 0; i < match.Length; i++) {
                    char c = swear.Substring(i, 1).ToCharArray()[0];
                    sb[match.Index + i] = censorChar;
                }
            }
        }
        foreach (string white in swearWhiteList) {
            Regex matcher = new Regex(white);
            foreach (Match match in matcher.Matches(phrase)) {
                string mask = phrase.Substring(match.Index, match.Length);
                for (int i = 0; i < match.Length; i++) {
                    sb[match.Index + i] = white.Substring(i, 1).ToCharArray()[0];
                }
            }
        }
        for (int i = 0; i < sb.Length; i++) {
            if (sb[i] != phrase[i]) {
                swearList.Add(true);
            } else {
                swearList.Add(false);
            }
        }
        return sb.ToString();
    }

    struct BuffMessage {
        public string On;
        public string Off;
        public BuffMessage(string on, string off) {
            this.Off = off;
            this.On = on;
        }
    }
    static Dictionary<BuffType, BuffMessage> buffMessages = new Dictionary<BuffType, BuffMessage>(){
            {BuffType.strength, new BuffMessage("I feel strong!", "I no longer feel strong!")},
            {BuffType.telepathy, new BuffMessage("I can hear thoughts!", "I can no longer hear thoughts!")},
            {BuffType.fireproof, new BuffMessage("I feel fireproof!", "I no longer feel fireproof!")},
            {BuffType.undead, new BuffMessage("I feel kinda weird!", "I feel blessed!")},
            {BuffType.poison, new BuffMessage("I don't feel so good!", "I feel much better!")},
            {BuffType.invulnerable, new BuffMessage("I am invincible!", "I feel exposed!")},
        };
    private string words;
    public bool speaking = false;
    public string[] randomPhrases;
    private List<MessageSpeech> queue = new List<MessageSpeech>();
    private float speakTime;
    private float queueTime;
    private float speakTimeTotal;
    private FollowGameObjectInCamera follower;
    private GameObject flipper;
    private Text bubbleText;
    private float speakSpeed;
    private bool[] swearMask;
    public bool vomiting;
    public string voice;
    public AudioClip[] speakSounds;
    public Vector2 pitchRange = new Vector2(0, 1);
    public Vector2 spacingRange = new Vector2(0.1f, 0.15f);
    public SoundGibberizer gibberizer;
    public AudioClip bleepSound;
    public string flavor = "test";
    public Controllable.HitState hitState;
    public Sprite[] portrait;
    public string defaultMonologue;
    public string cameraMonologue;
    public bool onCamera;
    public bool disableSpeakWith;
    public bool glibSpeakWith;
    public bool inDialogue;
    private Dictionary<BuffType, Buff> currentNetIntrinsic;
    public Dictionary<BuffType, Buff> previousNetInstrinsic;
    public bool doCompareIntrinsic;
    public bool configured = false;
    public Grammar grammar = new Grammar();
    public List<string> otherNimrodDefs;
    public void LoadGrammar() {
        grammar = new Grammar();
        grammar.Load("structure");
        // grammar.Load("flavor_test");
        grammar.Load("flavor_" + flavor);
        grammar.Load("threat");
        grammar.Load("random_phrase");
        foreach (string otherFile in otherNimrodDefs) {
            grammar.Load(otherFile);
        }
    }
    void Awake() {
        LoadGrammar();


        GameObject speechFramework = Instantiate(Resources.Load("UI/SpeechChild"), transform.position, Quaternion.identity) as GameObject;
        speechFramework.name = "SpeechChild";
        speechFramework.transform.SetParent(transform, false);
        speechFramework.transform.localPosition = Vector3.zero;
        flipper = transform.Find("SpeechChild").gameObject;
        Transform bubbleParent = transform.Find("SpeechChild/Speechbubble");
        bubbleText = bubbleParent.transform.Find("Text").gameObject.GetComponent<Text>();
        bubbleText.text = "";
        follower = bubbleText.GetComponent<FollowGameObjectInCamera>();
        follower.target = gameObject;
        if (flipper.transform.localScale != transform.localScale) {
            Vector3 tempscale = transform.localScale;
            flipper.transform.localScale = tempscale;
        }
        if (bubbleParent) {
            Canvas bubbleCanvas = bubbleParent.GetComponent<Canvas>();
            if (bubbleCanvas) {
                bubbleCanvas.worldCamera = Camera.main;
            }
        }
        if (voice != null) {
            AudioClip[] voiceSounds = Resources.LoadAll<AudioClip>("sounds/speechSets/" + voice);
            speakSounds = speakSounds.Concat(voiceSounds).ToArray();
        }
        // gibberizer = gameObject.AddComponent<SoundGibberizer>();
        gibberizer = Toolbox.GetOrCreateComponent<SoundGibberizer>(gameObject);
        gibberizer.bleepSound = bleepSound;
        gibberizer.sounds = speakSounds;
        gibberizer.pitchRange = pitchRange;
        gibberizer.spacingRange = spacingRange;
        gibberizer.Initialize();
        Toolbox.RegisterMessageCallback<MessageSpeech>(this, HandleSpeech);
        Toolbox.RegisterMessageCallback<MessageHitstun>(this, HandleHitStun);
        Toolbox.RegisterMessageCallback<MessageNetIntrinsic>(this, HandleNetIntrinsic);
        Toolbox.RegisterMessageCallback<MessageAnimation>(this, HandleAnimation);
        Toolbox.RegisterMessageCallback<MessageHead>(this, HandleHead);
        Toolbox.RegisterMessageCallback<MessageOnCamera>(this, HandleOnCamera);
    }
    void Start() {
        Interaction speak = new Interaction(this, "Look", "Describe");
        speak.unlimitedRange = true;
        speak.otherOnSelfConsent = false;
        speak.selfOnSelfConsent = false;
        speak.holdingOnOtherConsent = false;
        speak.dontWipeInterface = false;
        interactions.Add(speak);
        if (!disableSpeakWith && !glibSpeakWith) {
            Interaction speakWith = new Interaction(this, "Talk...", "SpeakWith");
            speakWith.unlimitedRange = true;
            speakWith.validationFunction = true;
            speakWith.AddDesireFunction(DesireToSpeakWith);
            interactions.Add(speakWith);
        }
        if (glibSpeakWith) {
            Interaction speakWith = new Interaction(this, "Talk", "SayRandom");
            speakWith.unlimitedRange = true;
            speakWith.selfOnOtherConsent = false;
            speakWith.selfOnSelfConsent = false;
            string myname = Toolbox.Instance.GetName(gameObject);
            interactions.Add(speakWith);
        }
    }
    void HandleOnCamera(MessageOnCamera message) {
        onCamera = message.value;
    }
    void HandleSpeech(MessageSpeech message) {
        if (message.swearTarget != null) {
            Swear(target: message.swearTarget);
            return;
        }
        if (message.randomSwear) {
            Swear();
            return;
        }
        Say(message);
    }
    void HandleHitStun(MessageHitstun message) {
        hitState = message.hitState;
        if (hitState >= Controllable.HitState.unconscious) {
            Stop();
        }
    }
    void HandleNetIntrinsic(MessageNetIntrinsic message) {
        if (GameManager.Instance.playerObject == gameObject) {
            if (!doCompareIntrinsic)
                previousNetInstrinsic = currentNetIntrinsic;
            currentNetIntrinsic = message.netBuffs;
            doCompareIntrinsic = true;
        }
    }
    void HandleAnimation(MessageAnimation message) {
        if (message.type == MessageAnimation.AnimType.punching && message.value == true) {
            MessageSpeech speech = new MessageSpeech("{punchsay}");
            speech.nimrod = true;
            speech.interrupt = true;
            Say(speech);
        }
    }
    void HandleHead(MessageHead message) {
        if (message.type == MessageHead.HeadType.vomiting) {
            vomiting = message.value;
            if (vomiting) {
                gibberizer.StopPlay();
            }
        }
    }
    public DialogueMenu SpeakWith() {
        // TODO: fix commanding someone to speak with player
        UINew.Instance.RefreshUI();
        DialogueMenu menu = UINew.Instance.ShowMenu(UINew.MenuType.dialogue).GetComponent<DialogueMenu>();
        if (InputController.Instance.commandTarget == null) {
            menu.Configure(GameManager.Instance.playerObject.GetComponent<Speech>(), this);
        } else {
            menu.Configure(InputController.Instance.commandTarget.GetComponent<Speech>(), this);
        }
        return menu;
    }
    public desire DesireToSpeakWith(Personality myPersonality, GameObject requester) {
        return desire.decline;
    }
    public string SpeakWith_desc() {
        string otherName = Toolbox.Instance.GetName(gameObject);
        return "Speak with " + otherName;
    }
    public bool SpeakWith_Validation() {
        if (disableInteractions)
            return false;
        if (GameManager.Instance.playerObject == null)
            return false;
        Speech controlSpeech = GameManager.Instance.playerObject.GetComponent<Speech>();
        if (InputController.Instance.state == InputController.ControlState.commandSelect) {
            controlSpeech = InputController.Instance.commandTarget.GetComponent<Speech>();
        }
        if (controlSpeech == null)
            return false;
        if (controlSpeech.glibSpeakWith) {
            return false;
        }
        if (InputController.Instance.state == InputController.ControlState.commandSelect) {
            return InputController.Instance.commandTarget != gameObject;
        } else {
            return GameManager.Instance.playerObject != gameObject;
        }
    }
    // TODO: allow liquids and things to self-describe; add modifiers etc.
    // maybe this functionality should be in the base object class?
    public void Describe(Item obj) {
        LiquidContainer container = obj.GetComponent<LiquidContainer>();
        MonoLiquid mono = obj.GetComponent<MonoLiquid>();
        BookPickup book = obj.GetComponent<BookPickup>();
        string phrase = "";
        if (container) {
            if (container.amount > 0 && container.descriptionName != "") {
                phrase = "It's a " + container.descriptionName + " full of " + container.liquid.name + ".";
            } else {
                phrase = obj.description;
            }
        } else if (mono) {
            phrase = "It's " + mono.liquid.name + ".";
        } else if (book) {
            phrase = book.book.Describe();
        } else {
            phrase = Monologue.replaceHooks(obj.description);
        }
        MessageSpeech message = new MessageSpeech(phrase);
        Say(message);
    }
    public string Describe_desc(Item obj) {
        string itemname = Toolbox.Instance.GetName(obj.gameObject);
        return "Look at " + itemname;
    }
    void FixedUpdate() {
        if (doCompareIntrinsic) {
            CompareIntrinsic();
        }
        configured = true;
    }
    void Update() {
        if (speakTime > 0) {
            speakTime -= Time.deltaTime;
            if (!speaking) {
                MessageHead head = new MessageHead();
                head.type = MessageHead.HeadType.speaking;
                head.value = true;
                Toolbox.Instance.SendMessage(gameObject, this, head);
            }
            speaking = true;
            follower.PreemptiveUpdate();
            bubbleText.text = words;
            float charIndex = (speakTimeTotal - speakTime) * speakSpeed;

            // Debug.Log(charIndex);
            // Debug.Log(swearMask.Length);
            if (charIndex < swearMask.Length) {
                gibberizer.bleep = swearMask[(int)charIndex];
                if (!gibberizer.play && !vomiting && !inDialogue) {
                    gibberizer.StartPlay();
                }
            }
        }
        if (speakTime < 0) {
            Stop();
        }
        if (!speaking && queue.Count > 0) {
            queueTime += Time.deltaTime;
            if (queueTime > 1f) {
                queueTime = Random.Range(-10f, 0f);
                int index = Random.Range(0, queue.Count);
                Say(queue[index]);
                queue.RemoveAt(index);
            }
        }
    }
    public void Stop() {
        speakTime = 0;
        // audioSource.Stop();
        gibberizer.StopPlay();
        if (speaking) {
            MessageHead head = new MessageHead();
            head.type = MessageHead.HeadType.speaking;
            head.value = false;
            Toolbox.Instance.SendMessage(gameObject, this, head);
        }
        speaking = false;
        // bubbleParent.SetActive(false);
        bubbleText.text = "";
        speakTime = 0;
        queueTime = 0;
    }
    public void LateUpdate() {
        // if the parent scale is flipped, we need to flip the flipper back to keep
        // the text properly oriented.
        if (flipper.transform.localScale != transform.localScale) {
            Vector3 tempscale = transform.localScale;
            flipper.transform.localScale = tempscale;
        }
    }
    public void SayRandom() {
        if (randomPhrases.Length > 0) {
            // string toSay = randomPhrases[Random.Range(0, randomPhrases.Length)];
            string toSay = grammar.Parse("{random}");
            MessageSpeech message = new MessageSpeech(toSay);
            Toolbox.Instance.SendMessage(gameObject, this, message);
        }
    }
    public string SayRandom_desc() {
        string otherName = Toolbox.Instance.GetName(gameObject);
        return "Speak with " + otherName;
    }
    public void Say(MessageSpeech message) {
        if (message.phrase == "")
            return;
        if (hitState >= Controllable.HitState.unconscious)
            return;
        if (speaking && message.phrase != words && !message.interrupt) {
            if (queue.Count >= 1)
                return;
            queue.Add(message);
            return;
        }

        OccurrenceSpeech speechData = message.ToOccurrenceSpeech(grammar);
        if (speechData == null)
            return;
        speechData.speaker = gameObject;

        if (speechData != null)
            Toolbox.Instance.OccurenceFlag(gameObject, speechData);

        if (inDialogue)
            return;

        speakTime = DoubleSeat(message.phrase.Length, 2f, 50f, 5f, 2f);
        speakTimeTotal = speakTime;
        speakSpeed = message.phrase.Length / speakTime;
        swearMask = speechData.swearList.ToArray();
        words = speechData.line;
    }

    public void Insult(string phrase, GameObject target) {
        MessageSpeech message = new MessageSpeech(phrase, data: new EventData(chaos: 2, disturbing: 1, positive: -2, offensive: Random.Range(2, 3)));
        message.insultTarget = target;

        Say(message);
        MessageNoise noise = new MessageNoise(gameObject);
        Toolbox.Instance.SendMessage(target, this, noise);
    }
    public void Threaten(string phrase, GameObject target) {
        MessageSpeech message = new MessageSpeech(phrase, data: new EventData(chaos: 2, disturbing: 1, positive: -2, offensive: Random.Range(2, 3)));
        message.threatTarget = target;
        Say(message);
        MessageNoise noise = new MessageNoise(gameObject);
        Toolbox.Instance.SendMessage(target, this, noise);
    }
    void SayPhrase(string phrase) {
        MessageSpeech message = new MessageSpeech();
        message.phrase = phrase;
        Say(message);
    }
    void CompareIntrinsic() {
        doCompareIntrinsic = false;
        if (!configured)
            return;
        if (currentNetIntrinsic == null)
            return;
        if (previousNetInstrinsic == null) {
            foreach (KeyValuePair<BuffType, BuffMessage> kvp in buffMessages) {
                if (currentNetIntrinsic[kvp.Key].boolValue)
                    SayPhrase(kvp.Value.On);
            }
        } else {
            foreach (KeyValuePair<BuffType, BuffMessage> kvp in buffMessages) {
                if (currentNetIntrinsic[kvp.Key].boolValue != previousNetInstrinsic[kvp.Key].boolValue) {
                    if (currentNetIntrinsic[kvp.Key].boolValue) {
                        SayPhrase(kvp.Value.On);
                    } else {
                        SayPhrase(kvp.Value.Off);
                    }
                }
            }
        }
    }
    // double-exponential seat easing function
    public float DoubleSeat(float x, float a, float w, float max, float min) {
        float result = 0f;
        if (x / w > 1) {
            x = w;
        }
        if (x / w <= 0.5) {
            result = Mathf.Pow(2 * x / w, a) / 2 * (max - min) + min;
        } else {
            result = (1f - Mathf.Pow(2f - 2f * (x / w), a) / 2f) * (max - min) + min;
        }
        return result;
    }
    public void Swear(GameObject target = null) {
        if (!target) {
            MessageSpeech message = new MessageSpeech("shazbot!");
            Say(message);
            return;
        }

        GameObject mainTarget = InputController.Instance.GetBaseInteractive(target.transform);
        string targetname = Toolbox.Instance.GetName(mainTarget);
        Insult("that shazbotting " + targetname + "!", target);

        MessageNoise noise = new MessageNoise(gameObject);
        Toolbox.Instance.SendMessage(target, this, noise);

        MessageInsult messageInsult = new MessageInsult();
        Toolbox.Instance.SendMessage(target, this, messageInsult);

        // Controllable control = GetComponent<Controllable>();
        using (Controller control = new Controller(gameObject)) {
            control.LookAtPoint(target.transform.position);
        }

        if (GameManager.Instance.data.perks["burn"]) {
            MessageDamage burnNotice = new MessageDamage(10f, damageType.fire);
            Toolbox.Instance.SendMessage(target, this, burnNotice);
            Flammable targetFlammable = target.transform.root.GetComponentInChildren<Flammable>();
            if (targetFlammable) {
                targetFlammable.heat += 100f;
                targetFlammable.burnTimer = 1f;
                targetFlammable.fireRetardantBuffer = 0f;
                targetFlammable.onFire = true;
            }
        }
    }
    public Monologue InsultMonologue(GameObject target) {
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();
        string content = global::Insult.ComposeInsult(target);
        MessageInsult messageInsult = new MessageInsult();
        Toolbox.Instance.SendMessage(target, this, messageInsult);

        Insult(content, target);

        List<bool> swearList = new List<bool>();
        string censoredContent = ProcessDialogue(content, ref swearList);
        swearMask = swearList.ToArray();
        List<string> strings = new List<string>() { censoredContent };

        Monologue mono = new Monologue(this, strings.ToArray());

        // Controllable control = GetComponent<Controllable>();
        // control.LookAtPoint(target.transform.position);
        using (Controller control = new Controller(gameObject)) {
            control.LookAtPoint(target.transform.position);
        }
        return mono;
    }
    public Monologue ThreatMonologue(GameObject target) {
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();

        // the key 
        Gender targetGender = Toolbox.GetGender(target);
        if (targetGender == Gender.male) {
            grammar.SetSymbol("gender", "man");
        } else {
            grammar.SetSymbol("gender", "lady");
        }
        string content = grammar.Parse("{threat}");

        Threaten(content, target);
        List<bool> swearList = new List<bool>();
        string censoredContent = ProcessDialogue(content, ref swearList);
        swearMask = swearList.ToArray();
        List<string> strings = new List<string>() { censoredContent };
        Monologue mono = new Monologue(this, strings.ToArray());

        // Controllable control = GetComponent<Controllable>();
        // control.LookAtPoint(target.transform.position);
        using (Controller control = new Controller(gameObject)) {
            control.LookAtPoint(target.transform.position);
        }
        return mono;
    }

    public Monologue Ellipsis() {
        return new Monologue(this, new string[] { "..." });
    }
    public Monologue Riposte(bool say = false) {
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();
        Monologue mono = new Monologue(this, new string[] { "How dare you!" });
        if (say) {
            MessageSpeech message = new MessageSpeech("How dare you!", data: new EventData(chaos: 1, disturbing: 0, positive: -1, offensive: 0));
            Say(message);
        }
        return mono;
    }
    public Monologue RespondToThreat(bool say = false) {
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();
        Monologue mono = new Monologue(this, new string[] { "Mercy!" });
        if (say) {
            MessageSpeech message = new MessageSpeech("Mercy!", data: new EventData(chaos: 1, disturbing: 0, positive: -1, offensive: 0));
            Say(message);
        }
        return mono;
    }

    public void SaveData(PersistentComponent data) {
        data.ints["hitstate"] = (int)hitState;
        data.strings["name"] = speechName;
        data.floats["pitchLow"] = pitchRange.x;
        data.floats["pitchHigh"] = pitchRange.y;
        data.floats["spacingLow"] = spacingRange.x;
        data.floats["spacingHigh"] = spacingRange.y;
        data.strings["speechSet"] = voice;
        data.bools["glibSpeakWith"] = glibSpeakWith;
    }
    public void LoadData(PersistentComponent data) {
        hitState = (Controllable.HitState)data.ints["hitstate"];
        speechName = data.strings["name"];

        pitchRange.x = data.floats["pitchLow"];
        pitchRange.y = data.floats["pitchHigh"];
        spacingRange.x = data.floats["spacingLow"];
        spacingRange.y = data.floats["spacingHigh"];
        voice = data.strings["speechSet"];
        glibSpeakWith = data.bools["glibSpeakWith"];
    }
}

