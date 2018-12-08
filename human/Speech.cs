using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Nimrod;

public class Speech : Interactive, ISaveable {
    static string[] swearWords = new string[]{
        @"\bshit\b", 
        @"\bfucked\b", 
        @"\bfuck\b", 
        @"\bshazbotting\b",
        @"\bshazbot\b",
        @"\bpiss\b", 
        @"\bdick\b", 
        @"\bass\b"};
    private string words;
    public bool speaking = false;
    public string[] randomPhrases;
    private List<MessageSpeech> queue = new List<MessageSpeech>();
    private float speakTime;
    private float queueTime;
    private float speakTimeTotal;
    // private GameObject bubbleParent;
    private FollowGameObjectInCamera follower;
    private GameObject flipper;
    private Text bubbleText;
    private float speakSpeed;
    private bool[] swearMask;
    public bool vomiting;
    // public AudioClip speakSound;
    public string voice = "none";
    public AudioClip[] speakSounds;
    public Vector2 pitchRange = new Vector2(0, 1);
    public Vector2 spacingRange = new Vector2(0.1f, 0.15f);
    public SoundGibberizer gibberizer;
    public AudioClip bleepSound;
    public string flavor = "test";
    private Dictionary<BuffType, Buff> lastNetIntrinsic;
    public Controllable.HitState hitState;
    public Sprite[] portrait;
    public string defaultMonologue;
    public bool disableSpeakWith;
    public bool inDialogue;
    void Awake() {
        Interaction speak = new Interaction(this, "Look", "Describe");
        speak.hideInManualActions = true;
        speak.limitless = true;
        speak.otherOnPlayerConsent = false;
        speak.playerOnOtherConsent = false;
        speak.inertOnPlayerConsent = false;
        speak.dontWipeInterface = false;
        interactions.Add(speak);
        if (!disableSpeakWith) {
            Interaction speakWith = new Interaction(this, "Talk...", "SpeakWith");
            speakWith.limitless = true;
            speakWith.validationFunction = true;
            interactions.Add(speakWith);
        }
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
        if (voice != "none") {
            AudioClip[] voiceSounds = Resources.LoadAll<AudioClip>("sounds/speechSets/"+voice);
            speakSounds = speakSounds.Concat(voiceSounds).ToArray();
        }
        gibberizer = gameObject.AddComponent<SoundGibberizer>();
        gibberizer.bleepSound = bleepSound;
        gibberizer.sounds = speakSounds;
        gibberizer.pitchRange = pitchRange;
        gibberizer.spacingRange = spacingRange;
        Toolbox.RegisterMessageCallback<MessageSpeech>(this, HandleSpeech);
        Toolbox.RegisterMessageCallback<MessageHitstun>(this, HandleHitStun);
        Toolbox.RegisterMessageCallback<MessageNetIntrinsic>(this, HandleNetIntrinsic);
        Toolbox.RegisterMessageCallback<MessageAnimation>(this, HandleAnimation);
        Toolbox.RegisterMessageCallback<MessageHead>(this, HandleHead);
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
        if (message.randomSpeech) {
            SayRandom();
            return;
        }
        Say(message);
    }
    void HandleHitStun(MessageHitstun message) {
        hitState = message.hitState;
    }
    void HandleNetIntrinsic(MessageNetIntrinsic message) {
        if (GameManager.Instance.playerObject == gameObject)
            CompareLastNetIntrinsic(message.netBuffs);
        lastNetIntrinsic = message.netBuffs;
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
        if (Controller.Instance.commandTarget == null) {
            menu.Configure(GameManager.Instance.playerObject.GetComponent<Speech>(), this);
        } else {
            menu.Configure(Controller.Instance.commandTarget.GetComponent<Speech>(), this);
        }
        return menu;
    }
    public string SpeakWith_desc() {
        string otherName = Toolbox.Instance.GetName(gameObject);
        return "Speak with " + otherName;
    }
    public bool SpeakWith_Validation() {
        if (Controller.Instance.state == Controller.ControlState.commandSelect){
            return Controller.Instance.commandTarget != gameObject;
        } else {
            return GameManager.Instance.playerObject != gameObject;
        }
    }
    // TODO: allow liquids and things to self-describe; add modifiers etc.
    // maybe this functionality should be in the base object class?
    public void Describe(Item obj) {
        LiquidContainer container = obj.GetComponent<LiquidContainer>();
        MonoLiquid mono = obj.GetComponent<MonoLiquid>();
        MessageSpeech message = new MessageSpeech();
        if (container) {
            if (container.amount > 0 && container.descriptionName != "") {
                message.phrase = "It's a " + container.descriptionName + " of " + container.liquid.name + ".";
            } else {
                message.phrase = obj.description;
            }
        } else if (mono) {
            message.phrase = "It's " + mono.liquid.name + ".";
        } else {
            message.phrase = obj.description;
        }
        Say(message);
    }
    public string Describe_desc(Item obj) {
        string itemname = Toolbox.Instance.GetName(obj.gameObject);
        return "Look at " + itemname;
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
            
            if (charIndex < swearMask.Length) {
                gibberizer.bleep = swearMask[(int)charIndex];
                if (!gibberizer.play && !vomiting){
                    gibberizer.StartPlay();
                }
            }
        }
        if (speakTime < 0) {
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
        if (!speaking && queue.Count > 0){
            queueTime += Time.deltaTime;
            if (queueTime > 1f){
                queueTime = Random.Range(-10f, 0f);
                int index = Random.Range(0, queue.Count);
                Say(queue[index]);
                queue.RemoveAt(index);
            }
        }
    }
    public void LateUpdate(){
        // if the parent scale is flipped, we need to flip the flipper back to keep
        // the text properly oriented.
        if (flipper.transform.localScale != transform.localScale) {
            Vector3 tempscale = transform.localScale;
            flipper.transform.localScale = tempscale;
        }
    }
    public void SayRandom() {
        if (randomPhrases.Length > 0) {
            string toSay = randomPhrases[Random.Range(0, randomPhrases.Length)];
            MessageSpeech message = new MessageSpeech(toSay);
            Toolbox.Instance.SendMessage(gameObject, this, message);
        }
    }
    public OccurrenceSpeech Say(MessageSpeech message) {
        if (message.phrase == "")
            return null;
        if (hitState >= Controllable.HitState.unconscious)
            return null;
        if (speaking && message.phrase != words && !message.interrupt) {
            if (queue.Count >= 1)
                return null;
            queue.Add(message);
            return null;
        }
        if (message.nimrod){
            Grammar grammar = new Grammar();
            grammar.Load("structure");
            grammar.Load("flavor_test");
            grammar.Load("flavor_" + flavor);
            message.phrase = grammar.Parse(message.phrase);
            if (message.phrase == "")
                return null;
        }
        OccurrenceSpeech speechData = new OccurrenceSpeech();
        speechData.speaker = gameObject;
        if (message.eventData == null)
            message.eventData = new EventData();
        speechData.events.Add(message.eventData);
        string censoredPhrase = CensorSwears(message.phrase);
        speechData.profanity = Toolbox.LevenshteinDistance(message.phrase, censoredPhrase);
        swearMask = new bool[message.phrase.Length];
        for (int i = 0; i < message.phrase.Length; i++){
            swearMask[i] = message.phrase[i] != censoredPhrase[i];
        }
        
        message.eventData.ratings[Rating.chaos] += speechData.profanity * 2f;
        message.eventData.ratings[Rating.offensive] += speechData.profanity * 5f;
        
        speechData.line = censoredPhrase;
        if (inDialogue)
            return null;
        words = censoredPhrase;
        speakTime = DoubleSeat(message.phrase.Length, 2f, 50f, 5f, 2f);
        speakTimeTotal = speakTime;
        speakSpeed = message.phrase.Length / speakTime;
        HashSet<GameObject> involvedParties = new HashSet<GameObject>(){gameObject};
        if (message.insultTarget != null){
            speechData.insult = true;
            speechData.target = message.insultTarget;
            involvedParties.Add(message.insultTarget);
        }
        if (message.threatTarget != null){
            speechData.threat = true;
            speechData.target = message.threatTarget;
            involvedParties.Add(message.threatTarget);
        }
        involvedParties.UnionWith(message.involvedParties);
        Toolbox.Instance.OccurenceFlag(gameObject, speechData, involvedParties);
        return speechData;
    }
    public string CensorSwears(string phrase){
        string censoredPhrase = phrase;
        foreach(string swear in swearWords){
            StringBuilder builder = new StringBuilder();
            foreach(char c in swear.Substring(2, swear.Length-4)) {
                builder.Append("∎");
            }
            string mask = builder.ToString();
            censoredPhrase = Regex.Replace(censoredPhrase, swear, mask);
        }
        return censoredPhrase;
    }
    public void Insult(string phrase, GameObject target, EventData data = null){
        MessageSpeech message = new MessageSpeech(phrase);
        message.eventData = new EventData();
        if (data != null) {
            message.eventData = data;
        }
        message.eventData.noun = "insults";
        Say(message);
        MessageNoise noise = new MessageNoise(gameObject);
        Toolbox.Instance.SendMessage(target, this, noise);
    }
    public void Threaten(string phrase, GameObject target, EventData data = null){
        MessageSpeech message = new MessageSpeech(phrase);
        message.eventData = new EventData();
        if (data != null) {
            message.eventData = data;
        }
        message.eventData.noun = "threats";
        Say(message);
        MessageNoise noise = new MessageNoise(gameObject);
        Toolbox.Instance.SendMessage(target, this, noise);
    }
    public void CompareLastNetIntrinsic(Dictionary<BuffType, Buff> net) {
        if (lastNetIntrinsic == null)
            return;
        if (lastNetIntrinsic[BuffType.fireproof].boolValue != net[BuffType.fireproof].boolValue) {
            MessageSpeech message = new MessageSpeech();
            if (net[BuffType.fireproof].boolValue) {
                message.phrase = "I feel fireproof!";
            } else {
                message.phrase = "I no longer feel fireproof!";
            }
            Say(message);
        }
        if (lastNetIntrinsic[BuffType.telepathy].boolValue != net[BuffType.telepathy].boolValue) {
            MessageSpeech message = new MessageSpeech();
            if (net[BuffType.telepathy].boolValue)
                message.phrase = "I can hear thoughts!";
            Say(message);
        }
        if (lastNetIntrinsic[BuffType.strength].boolValue != net[BuffType.strength].boolValue) {
            MessageSpeech message = new MessageSpeech();
            if (net[BuffType.strength].boolValue) {
                message.phrase = "I feel strong!";
            } else {
                message.phrase = "I no longer feel strong!";
            }
            Say(message);
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
    // TODO: this function will change to incorporate Nimrod and flavor
    public void Swear(GameObject target = null) {
        if (!target) {
            MessageSpeech message = new MessageSpeech("shazbot!");
            Say(message);
            return;
        }
        GameObject mainTarget = Controller.Instance.GetBaseInteractive(target.transform);
        string targetname = Toolbox.Instance.GetName(mainTarget);
        Insult("that shazbotting " + targetname + "!", target);
        MessageNoise noise = new MessageNoise(gameObject);
        Toolbox.Instance.SendMessage(target, this, noise);
        Controllable control = GetComponent<Controllable>();
        control.LookAtPoint(target.transform.position);
    }
    public Monologue InsultMonologue(GameObject target) {
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();
        string content = Insults.ComposeInsult(target);
        MessageInsult messageInsult = new MessageInsult();
        Toolbox.Instance.SendMessage(target, this, messageInsult);

        EventData data = new EventData(chaos: 2, disturbing: 1, positive: -2, offensive: Random.Range(2, 3));
        Insult(content, target, data: data);
        string censoredContent = CensorSwears(content);
        List<string> strings = new List<string>() { censoredContent };
        Monologue mono = new Monologue(this, strings.ToArray());

        Controllable control = GetComponent<Controllable>();
        control.LookAtPoint(target.transform.position);
        return mono;
    }
    public Monologue ThreatMonologue(GameObject target) {
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();

        Grammar grammar = new Grammar();
        grammar.Load("structure");
        grammar.Load("flavor_" + flavor);
        string content = grammar.Parse("{threat}");

        EventData data = new EventData(chaos: 2, disturbing: 1, positive: -2, offensive: Random.Range(2, 3));
        Threaten(content, target, data: data);
        string censoredContent = CensorSwears(content);
        List<string> strings = new List<string>() { censoredContent };
        Monologue mono = new Monologue(this, strings.ToArray());

        Controllable control = GetComponent<Controllable>();
        control.LookAtPoint(target.transform.position);
        return mono;
    }

    public Monologue Ellipsis() {
        return new Monologue(this, new string[] { "..." });
    }
    public Monologue Riposte(bool say = false) {
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();
        Monologue mono = new Monologue(this, new string[] { "How dare you!" });
        EventData data = new EventData(chaos: 1, disturbing: 0, positive: -1, offensive: 0);
        if (say){
            MessageSpeech message = new MessageSpeech("how dare you!");
            message.eventData = data;
            Say(message);
            // MessageNoise noise = new MessageNoise(gameObject);
            // Toolbox.Instance.SendMessage(target, this, noise);
        }
        return mono;
    }
    public Monologue RespondToThreat(bool say = false) {
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();
        Monologue mono = new Monologue(this, new string[] { "Mercy!" });
        EventData data = new EventData(chaos: 1, disturbing: 0, positive: -1, offensive: 0);
        if (say){
            MessageSpeech message = new MessageSpeech("Mercy!");
            message.eventData = data;
            Say(message);
        }
        return mono;
    }

    public void SaveData(PersistentComponent data) {
        data.ints["hitstate"] = (int)hitState;
    }
    public void LoadData(PersistentComponent data) {
        hitState = (Controllable.HitState)data.ints["hitstate"];
    }
}

