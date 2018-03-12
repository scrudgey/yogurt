using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using Nimrod;

public class Speech : Interactive, IMessagable, ISaveable {
    private string words;
    public bool speaking = false;
    public string[] randomPhrases;
    private List<string> queue = new List<string>();
    private float speakTime;
    private float speakTimeTotal;
    private GameObject bubbleParent;
    private FollowGameObjectInCamera follower;
    private GameObject flipper;
    private Text bubbleText;
    private float speakSpeed;
    private char[] chars;
    private int[] swearMask;
    public bool vomiting;
    public AudioClip speakSound;
    public AudioClip bleepSound;
    private AudioSource audioSource;
    public string flavor = "test";
    private Dictionary<BuffType, Buff> lastNetIntrinsic;
    public Controllable.HitState hitState;
    public Sprite portrait;
    public string defaultMonologue;
    public bool disableSpeakWith;
    void Awake() {
        Interaction speak = new Interaction(this, "Look", "Describe", true, false);
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
        bubbleParent = transform.Find("SpeechChild/Speechbubble").gameObject;
        bubbleText = bubbleParent.transform.Find("Text").gameObject.GetComponent<Text>();
        follower = bubbleText.GetComponent<FollowGameObjectInCamera>();
        follower.target = gameObject;
        audioSource = GetComponent<AudioSource>();
        if (flipper.transform.localScale != transform.localScale) {
            Vector3 tempscale = transform.localScale;
            flipper.transform.localScale = tempscale;
        }
        if (audioSource == null) {
            audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        }
        audioSource.loop = true;
        if (bubbleParent) {
            Canvas bubbleCanvas = bubbleParent.GetComponent<Canvas>();
            if (bubbleCanvas) {
                bubbleCanvas.worldCamera = Camera.main;
            }
        }
    }
    public DialogueMenu SpeakWith() {
        // TODO: fix commanding someone to speak with player
        DialogueMenu menu = UINew.Instance.ShowMenu(UINew.MenuType.dialogue).GetComponent<DialogueMenu>();
        if (Controller.Instance.commandTarget == null){ 
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
        // if (Controller.Instance.)
        return GameManager.Instance.playerObject != gameObject;
    }
    // TODO: allow liquids and things to self-describe; add modifiers etc.
    // maybe this functionality should be in the base object class?
    public void Describe(Item obj) {
        LiquidContainer container = obj.GetComponent<LiquidContainer>();
        MonoLiquid mono = obj.GetComponent<MonoLiquid>();
        if (container) {
            if (container.amount > 0 && container.descriptionName != "") {
                Say("It's a " + container.descriptionName + " of " + container.liquid.name + ".");
            } else {
                Say(obj.description);
            }
        } else if (mono) {
            Say("It's " + mono.liquid.name + ".");
        } else {
            Say(obj.description);
        }
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
            bubbleParent.SetActive(true);
            follower.PreemptiveUpdate();
            bubbleText.text = words;
            float charIndex = (speakTimeTotal - speakTime) * speakSpeed;
            // if the parent scale is flipped, we need to flip the flipper back to keep
            // the text properly oriented.
            if (flipper.transform.localScale != transform.localScale) {
                Vector3 tempscale = transform.localScale;
                flipper.transform.localScale = tempscale;
            }
            if (charIndex < swearMask.Length) {
                if (swearMask[(int)charIndex] == 0) {
                    if (audioSource.clip != speakSound)
                        audioSource.clip = speakSound;
                } else {
                    if (audioSource.clip != bleepSound)
                        audioSource.clip = bleepSound;
                }
                if (!audioSource.isPlaying && !vomiting) {
                    audioSource.Play();
                }
            }
        }
        if (speakTime < 0) {
            audioSource.Stop();
            if (speaking) {
                MessageHead head = new MessageHead();
                head.type = MessageHead.HeadType.speaking;
                head.value = false;
                Toolbox.Instance.SendMessage(gameObject, this, head);
            }
            speaking = false;
            bubbleParent.SetActive(false);
            speakTime = 0;
            if (queue.Count > 0) {
                words = queue[0];
                speakTime = DoubleSeat(words.Length, 2f, 5f, 12f, 2f);
                queue.RemoveAt(0);
                speaking = true;
                bubbleText.text = words;
                bubbleParent.SetActive(true);
            }
        }
    }
    public void SayRandom() {
        if (randomPhrases.Length > 0) {
            string toSay = randomPhrases[Random.Range(0, randomPhrases.Length)];
            Say(toSay);
        }
    }
    public void Say(string phrase, string swear = null, EventData data = null, GameObject insult = null, GameObject threat = null) {
        if (phrase == "")
            return;
        if (hitState >= Controllable.HitState.unconscious)
            return;
        if (speaking && phrase == words) {
            return;
        }
        OccurrenceSpeech speechData = new OccurrenceSpeech();
        speechData.speaker = gameObject;
        EventData eventData = new EventData();
        if (data != null) {
            eventData = data;
        }
        speechData.events.Add(eventData);
        if (threat != null) {
            speechData.threat = true;
            speechData.target = threat;
            eventData.noun = "threats";
        }
        if (insult != null) {
            speechData.insult = true;
            speechData.target = insult;
            eventData.noun = "insults";
        }
        string censoredPhrase = phrase;
        if (swear != null) {
            eventData.noun = "profanity";
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < swear.Length; i++) {
                builder.Append("∎");
            }
            censoredPhrase = censoredPhrase.Replace(swear, builder.ToString());
        }
        chars = phrase.ToCharArray();
        swearMask = new int[chars.Length];
        if (swear != null) {
            int index = phrase.IndexOf(swear);
            if (index != -1) {
                for (int i = index; i < index + swear.Length - 1; i++) {
                    swearMask[i] = 1;
                }
            }
            float extremity = 0f;
            foreach (int mask in swearMask) {
                extremity += mask;
            }
            eventData.ratings[Rating.chaos] += extremity * 2f;
            eventData.ratings[Rating.offensive] += extremity * 5f;
            // eventData.chaos += extremity * 2f;
            // eventData.offensive += extremity * 5f;
        }
        words = censoredPhrase;
        speakTime = DoubleSeat(phrase.Length, 2f, 50f, 5f, 2f);
        speakTimeTotal = speakTime;
        speakSpeed = phrase.Length / speakTime;

        speechData.line = words;
        Toolbox.Instance.OccurenceFlag(gameObject, speechData);
    }
    public void ReceiveMessage(Message incoming) {
        if (incoming is MessageSpeech) {
            MessageSpeech message = (MessageSpeech)incoming;
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
            if (message.nimrodKey) {
                SayFromNimrod(message.phrase);
                return;
            }
            if (message.swear != "") {
                Say(message.phrase, swear: message.swear, data: message.eventData);
            } else {
                Say(message.phrase, data: message.eventData);
            }
        }
        if (incoming is MessageHitstun) {
            MessageHitstun hits = (MessageHitstun)incoming;
            hitState = hits.hitState;
        }
        if (incoming is MessageNetIntrinsic) {
            MessageNetIntrinsic message = (MessageNetIntrinsic)incoming;
            if (GameManager.Instance.playerObject == gameObject)
                CompareLastNetIntrinsic(message.netBuffs);
            lastNetIntrinsic = message.netBuffs;
        }
        if (incoming is MessageAnimation) {
            MessageAnimation message = (MessageAnimation)incoming;
            if (message.type == MessageAnimation.AnimType.punching && message.value == true) {
                SayFromNimrod("punchsay");
            }
        }
        if (incoming is MessageOccurrence) {
            MessageOccurrence occur = (MessageOccurrence)incoming;
            foreach (EventData data in occur.data.events)
                ReactToOccurrence(data);
        }
        if (incoming is MessageHead) {
            MessageHead message = (MessageHead)incoming;
            if (message.type == MessageHead.HeadType.vomiting) {
                vomiting = message.value;
                if (vomiting) {
                    audioSource.Stop();
                }
            }
        }
    }
    void ReactToOccurrence(EventData od) {
        if (od.ratings[Rating.disgusting] > 1)
            SayFromNimrod("grossreact");
    }
    public void CompareLastNetIntrinsic(Dictionary<BuffType, Buff> net) {
        if (lastNetIntrinsic == null)
            return;
        if (lastNetIntrinsic[BuffType.fireproof].boolValue != net[BuffType.fireproof].boolValue) {
            if (net[BuffType.fireproof].boolValue) {
                Say("I feel fireproof!");
            } else {
                Say("I no longer feel fireproof!");
            }
        }
        if (lastNetIntrinsic[BuffType.telepathy].boolValue != net[BuffType.telepathy].boolValue) {
            if (net[BuffType.telepathy].boolValue)
                Say("I can hear thoughts!");
        }
        if (lastNetIntrinsic[BuffType.strength].boolValue != net[BuffType.strength].boolValue) {
            if (net[BuffType.strength].boolValue) {
                Say("I feel strong!");
            } else {
                Say("I no longer feel strong!");
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
    // TODO: this function will change to incorporate Nimrod and flavor
    public void Swear(GameObject target = null) {
        if (!target) {
            Say("shazbot!", "shazbot");
            return;
        }
        GameObject mainTarget = Controller.Instance.GetBaseInteractive(target.transform);
        string targetname = Toolbox.Instance.GetName(mainTarget);
        Say("that shazbotting " + targetname + "!", "shazbotting", insult: target);
    }
    public Monologue Insult(GameObject target) {
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();
        string content = Insults.ComposeInsult(target);
        MessageInsult messageInsult = new MessageInsult();
        Toolbox.Instance.SendMessage(target, this, messageInsult);

        EventData data = new EventData(chaos: 2, disturbing: 1, positive: -2, offensive: Random.Range(2, 3));
        Say(content, data: data, insult: target);

        List<string> strings = new List<string>() { content };
        Monologue mono = new Monologue(this, strings.ToArray());
        return mono;
    }
    public Monologue Threaten(GameObject target) {
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();

        Grammar grammar = new Grammar();
        grammar.Load("structure");
        grammar.Load("flavor_" + flavor);
        string content = grammar.Parse("{threat}");

        EventData data = new EventData(chaos: 2, disturbing: 1, positive: -2, offensive: Random.Range(2, 3));
        Say(content, data: data, threat: target);

        List<string> strings = new List<string>() { content };
        Monologue mono = new Monologue(this, strings.ToArray());
        return mono;
    }

    public Monologue Ellipsis() {
        return new Monologue(this, new string[] { "..." });
    }
    public Monologue Riposte() {
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();
        Monologue mono = new Monologue(this, new string[] { "How dare you!" });
        EventData data = new EventData(chaos: 1, disturbing: 0, positive: -1, offensive:0);
        Say("how dare you!", data: data);
        return mono;
    }
    public Monologue RespondToThreat() {
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();
        Monologue mono = new Monologue(this, new string[] { "Mercy!" });
        EventData data = new EventData(chaos: 1, disturbing: 0, positive: -1, offensive:0);
        Say("Mercy!", data: data);
        return mono;
    }
    public void SayFromNimrod(string key) {
        Grammar grammar = new Grammar();
        grammar.Load("structure");
        grammar.Load("flavor_" + flavor);
        Say(grammar.Parse("{" + key + "}"));
    }
    public void SaveData(PersistentComponent data) {
        data.bools["speaking"] = speaking;
        data.ints["hitstate"] = (int)hitState;
    }
    public void LoadData(PersistentComponent data) {
        speaking = data.bools["speaking"];
        hitState = (Controllable.HitState)data.ints["hitstate"];
    }
}

