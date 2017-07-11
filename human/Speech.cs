﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using Nimrod;

public class Speech : Interactive, IMessagable {
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
    
    public AudioClip speakSound;
    public AudioClip bleepSound;
    private AudioSource audioSource;
	private bool LoadInitialized = false;
    public string flavor = "test";
    private Intrinsic lastNetIntrinsic;
    public Controllable.HitState hitState;
    public Sprite portrait;
    public string defaultMonologue;
    public bool disableSpeakWith;
	void Start () {
        if (!LoadInitialized)
			LoadInit();
	}
    
    void LoadInit(){
        LoadInitialized = true;
        Interaction speak = new Interaction(this, "Look", "Describe", true, false);
		speak.limitless = true;
        speak.reversible = false;
		speak.dontWipeInterface = false;
		interactions.Add(speak);
        if (!disableSpeakWith){
            Interaction speakWith = new Interaction(this, "Talk...", "SpeakWith");
            speakWith.limitless = true;
            speakWith.validationFunction = true;
            interactions.Add(speakWith);
        }
        flipper = transform.FindChild("SpeechChild").gameObject;
		bubbleParent = transform.FindChild("SpeechChild/Speechbubble").gameObject;
		bubbleText = bubbleParent.transform.FindChild("Text").gameObject.GetComponent<Text>();
        follower = bubbleParent.GetComponentInChildren<FollowGameObjectInCamera>();
        audioSource = GetComponent<AudioSource>();
        if (flipper.transform.localScale != transform.localScale){
                Vector3 tempscale = transform.localScale;
                flipper.transform.localScale = tempscale; 
        }
        if (audioSource == null){
            audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        }
        audioSource.loop=true;
        if (bubbleParent){
            Canvas bubbleCanvas = bubbleParent.GetComponent<Canvas>();
            if (bubbleCanvas){
                bubbleCanvas.worldCamera = GameManager.Instance.cam;
            }
        }
    }
    public DialogueMenu SpeakWith(){
        DialogueMenu menu = UINew.Instance.ShowMenu(UINew.MenuType.dialogue).GetComponent<DialogueMenu>();
        menu.Configure(GameManager.Instance.playerObject.GetComponent<Speech>(), this);
        return menu;
    }
    public string SpeakWith_desc(){
		string otherName = Toolbox.Instance.GetName(gameObject);
        return "Speak with "+otherName;
    }
    public bool SpeakWith_Validation(){
        return GameManager.Instance.playerObject != gameObject;
    }
    // TODO: allow liquids and things to self-describe; add modifiers etc.
    // maybe this functionality should be in the base object class?
	public void Describe(Item obj){
        LiquidContainer container = obj.GetComponent<LiquidContainer>();
        MonoLiquid mono = obj.GetComponent<MonoLiquid>();
        if (container){
            if (container.amount > 0 && container.containerName != ""){
                Say("It's a "+container.containerName+" of "+container.liquid.name+".");
            } else {
                Say (obj.description);
            }
        } else if (mono){
            Say("It's "+mono.liquid.name+".");
        } else {
            Say (obj.description);
        }
	}
    public string Describe_desc(Item obj){
        string itemname = Toolbox.Instance.GetName(obj.gameObject);
        return "Look at "+itemname;
    }
	void Update () {
		if (speakTime > 0){
			speakTime -= Time.deltaTime;
            if (!speaking){
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
            if (flipper.transform.localScale != transform.localScale){
                Vector3 tempscale = transform.localScale;
                flipper.transform.localScale = tempscale; 
			}
            if (charIndex < swearMask.Length){
                if (swearMask[(int)charIndex] == 0){
                    if (audioSource.clip != speakSound)
                        audioSource.clip = speakSound;
                } else {
                    if (audioSource.clip != bleepSound)
                        audioSource.clip = bleepSound;
                }
                if (!audioSource.isPlaying){
                    audioSource.Play();
                }
            }
		}
		if (speakTime < 0){
            audioSource.Stop();
            if (speaking){
                MessageHead head = new MessageHead();
                head.type = MessageHead.HeadType.speaking;
                head.value = false;
                Toolbox.Instance.SendMessage(gameObject, this, head);

                OccurrenceSpeech data = new OccurrenceSpeech();
                data.speaker = gameObject;
                data.line = Toolbox.Instance.GetName(gameObject)+": "+words;
                Toolbox.Instance.OccurenceFlag(gameObject, data);
            }
			speaking = false;
			bubbleParent.SetActive(false);
			speakTime = 0;
			if (queue.Count > 0){
				words = queue[0];
                speakTime = DoubleSeat(words.Length, 2f, 5f, 12f, 2f);
				queue.RemoveAt(0);
				speaking = true;
				bubbleText.text = words;
				bubbleParent.SetActive(true);
			}
		}
	}
	public void SayRandom(){
		if(randomPhrases.Length > 0 ){
			string toSay = randomPhrases[Random.Range(0, randomPhrases.Length)];
			Say (toSay);
		}
	}
    public void Say(string phrase, string swear=null){
        if (phrase == "")
            return;
        if (hitState >= Controllable.HitState.unconscious)
            return;
        if(speaking && phrase == words){
            return;
        }
        if (speaking){
            OccurrenceSpeech data = new OccurrenceSpeech();
            data.speaker = gameObject;
            data.line = Toolbox.Instance.GetName(gameObject)+": "+words;
            Toolbox.Instance.OccurenceFlag(gameObject, data);
        }
        string censoredPhrase = phrase;
        if (swear != null){
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < swear.Length; i++)
            {
                // builder.Append("*");
                builder.Append("∎");
            }
            censoredPhrase = censoredPhrase.Replace(swear, builder.ToString());
        }
        chars = phrase.ToCharArray();
        swearMask = new int[chars.Length];
        if (swear != null){
            int index = phrase.IndexOf(swear);
            if (index != -1){
                for (int i = index; i < index + swear.Length-1; i++){
                    swearMask[i] = 1;
                }
            }
            float extremity = 0f;
            foreach (int mask in swearMask){
                extremity += mask;
            }
            Toolbox.Instance.DataFlag(gameObject, extremity *2f, 0f, 0f, extremity * 5f, 0f);
        }
		words = censoredPhrase;
        speakTime = DoubleSeat(phrase.Length, 2f, 50f, 5f, 2f);
        speakTimeTotal = speakTime;
        speakSpeed = phrase.Length / speakTime;
	}
    public void ReceiveMessage(Message incoming){
        if (incoming is MessageSpeech){
            MessageSpeech message = (MessageSpeech)incoming;
            if (message.swearTarget != null){
                Swear(message.swearTarget);
                return;
            }
            if (message.randomSwear){
                Swear();
                return;
            }
            if (message.randomSpeech){
                SayRandom();
                return;
            }
            if (message.sayLine){
                ScriptDirector director = FindObjectOfType<ScriptDirector>();
                if (!director){
                    Say("What's my line?"); 
                } else {
                    Say(director.NextTomLine());
                }
                return;
            }
            if (message.nimrodKey){
                SayFromNimrod(message.phrase);
                return;
            }
            if (message.swear != ""){
                Say(message.phrase, message.swear);
            } else {
                Say(message.phrase);
            }
        }
        if (incoming is MessageHitstun){
			MessageHitstun hits = (MessageHitstun)incoming;
            hitState = hits.hitState;
		}
        if (incoming is MessageNetIntrinsic){
            MessageNetIntrinsic message = (MessageNetIntrinsic)incoming;
            if (GameManager.Instance.playerObject == gameObject)
                CompareLastNetIntrinsic(message.netIntrinsic);
            lastNetIntrinsic = message.netIntrinsic;
        }
        if (incoming is MessageAnimation){
            MessageAnimation message = (MessageAnimation)incoming;
            if (message.type == MessageAnimation.AnimType.punching && message.value == true){
                SayFromNimrod("punchsay");
            }
        }
        if (incoming is MessageOccurrence){
            MessageOccurrence occur = (MessageOccurrence)incoming;
            // foreach (OccurrenceData data in occur.data)
            ReactToOccurrence(occur.data);
        }
    }
    void ReactToOccurrence(OccurrenceData od){
        if (od is OccurrenceVomit){
            SayFromNimrod("grossreact");
        }
        if (od is OccurrenceEat){
            if (od.disgusting > 10)
                SayFromNimrod("grossreact");
        }
    }
    public void CompareLastNetIntrinsic(Intrinsic net){
        if (lastNetIntrinsic == null)
            return;
        if (lastNetIntrinsic.fireproof.boolValue != net.fireproof.boolValue){
            if (net.fireproof.boolValue)
                Say("I feel fireproof!");
        }
        if (lastNetIntrinsic.telepathy.boolValue != net.telepathy.boolValue){
            if (net.telepathy.boolValue)
                Say("I can hear thoughts!");
        }
    }
    // double-exponential seat easing function
    public float DoubleSeat(float x, float a, float w, float max, float min){
        float result = 0f;
        if (x/w > 1){
            x = w;
        }
        if (x/w <= 0.5){
            result = Mathf.Pow(2*x/w, a) / 2 * (max - min) + min;
        } else {
            result = (1f - Mathf.Pow(2f - 2f *(x / w), a) / 2f ) * (max - min) + min;
        }
        return result;
    }
    // this function will change to incorporate Nimrod and flavor
    public void Swear(GameObject target=null){
        if (!target){
            Say("shazbot!", "shazbot");
            return;
        }
        string targetname = Toolbox.Instance.GetName(target);
        Say("that shazbotting "+targetname+"!", "shazbotting");
    }
    public Monologue Insult(GameObject target){
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();
        List<string> strings = new List<string>();

        Grammar grammar = new Grammar();
        grammar.Load("structure");
        grammar.Load("flavor_"+flavor);
        strings.Add(grammar.Parse("{insult}"));

        OccurrenceSpeech data = new OccurrenceSpeech();
		data.chaos = 10;
		data.offensive = 20;
		data.positive = -20;
        data.line = strings[0];
        Toolbox.Instance.OccurenceFlag(gameObject, data);

        Monologue mono = new Monologue(this, strings.ToArray());
        return mono;
    }
    public Monologue Threaten(GameObject target){
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();
        List<string> strings = new List<string>();

        Grammar grammar = new Grammar();
        grammar.Load("structure");
        grammar.Load("flavor_"+flavor);
        strings.Add(grammar.Parse("{threat}"));

        OccurrenceSpeech data = new OccurrenceSpeech();
		data.chaos = 15;
		data.offensive = 10;
		data.positive = -20;
        data.line = strings[0];
        Toolbox.Instance.OccurenceFlag(gameObject, data);

        Monologue mono = new Monologue(this, strings.ToArray());
        return mono;
    }

    public Monologue Ellipsis(){
        return new Monologue(this, new string[]{"..."});
    }
    public Monologue Riposte(){
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();
        Monologue mono = new Monologue(this, new string[]{"How dare you!"});
        return mono;
    }
    public Monologue RespondToThreat(){
        if (hitState >= Controllable.HitState.stun)
            return Ellipsis();
        Monologue mono = new Monologue(this, new string[]{"Mercy!"});
        return mono;
    }
    public void SayFromNimrod(string key){
        Grammar grammar = new Grammar();
        grammar.Load("structure");
        grammar.Load("flavor_"+flavor);
        Say(grammar.Parse("{"+key+"}"));
    }

}

