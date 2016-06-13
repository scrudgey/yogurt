using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Speech : Interactive {
	private string words;
	public bool speaking = false;
	public string[] randomPhrases;
	private List<string> queue = new List<string>();
	private float speakTime;
    private float speakTimeTotal;
	private GameObject bubbleParent;
    private GameObject flipper;
	private Text bubbleText;
    private float speakSpeed;
    private char[] chars;
    private int[] swearMask;
    
    public AudioClip speakSound;
    public AudioClip bleepSound;
    private AudioSource audioSource;
	private bool LoadInitialized = false;

	void Start () {
        if (!LoadInitialized)
			LoadInit();
	}
    
    void LoadInit(){
        LoadInitialized = true;
        Interaction speak = new Interaction(this, "Look", "Describe", true, false);
		speak.limitless = true;
		speak.dontWipeInterface = false;
		interactions.Add(speak);
        flipper = transform.FindChild("SpeechChild").gameObject;
		bubbleParent = transform.FindChild("SpeechChild/Speechbubble").gameObject;
		bubbleText = bubbleParent.transform.FindChild("Text").gameObject.GetComponent<Text>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null){
            audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
        }
        audioSource.loop=true;
        if (bubbleParent){
            Canvas bubbleCanvas = bubbleParent.GetComponent<Canvas>();
            if (bubbleCanvas){
                // bubbleCanvas.worldCamera = GameManager.Instance.cam;
                bubbleCanvas.worldCamera = GameObject.FindObjectOfType<Camera>();
            }
        }    
    }
    

    // TODO: allow liquids and things to self-describe; add modifiers etc.
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
			speaking = true;
			bubbleParent.SetActive(true);
			bubbleText.text = words;
            float charIndex = (speakTimeTotal - speakTime) * speakSpeed;
            // if the parent scale is flipped, we need to flip the flipper back to keep
            // the text properly oriented.
            if (flipper.transform.localScale != transform.localScale){
                Vector3 tempscale = transform.localScale;
                flipper.transform.localScale = tempscale; 
			}
            // if (!audioSource.isPlaying){
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

            // }
		}
		if (speakTime < 0){
            audioSource.Stop();
            if (speaking){
                Occurrence flag = Toolbox.Instance.OccurenceFlag(gameObject);
                OccurrenceSpeech data = new OccurrenceSpeech();
                data.speaker = gameObject;
                data.line = words;
                flag.data.Add(data);
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
			string toSay = randomPhrases[Random.Range(0,randomPhrases.Length)];
			Say (toSay);
		}
	}

	public void Say(string phrase, string swear=null){
		if(speaking && phrase == words){
			return;
		}
        // string censor = "∎";
        string censoredPhrase = phrase;
        chars = phrase.ToCharArray();
        swearMask = new int[chars.Length];
        if (swear != null){
           int index = phrase.IndexOf(swear);
           if (index != -1){
               for (int i = index; i < index + swear.Length-1; i++){
                   swearMask[i] = 1;
                //    censoredPhrase
                   Debug.Log(phrase[i]);
                //    censoredPhrase[i] = censor;
               }
           }
        }
        // Toolbox.Instance.ReplaceAt();
		words = censoredPhrase;
        speakTime = DoubleSeat(phrase.Length, 2f, 50f, 5f, 2f);
        speakTimeTotal = speakTime;
        speakSpeed = phrase.Length / speakTime;
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
    
    public void Swear(GameObject target=null){
        if (!target){
            Say("shazbot!", "shazbot");
            return;
        }
        string targetname = Toolbox.Instance.GetName(target);
        Say("that shazbotting "+targetname+"!", "shazbotting");
    }

}

