using UnityEngine;

public class Eater : Interactive {
	public float nutrition;
	public enum preference{neutral, likes, dislikes}
	enum nauseaStatement{none, warning, imminent}
	nauseaStatement lastStatement;
	public preference vegetablePreference;
	public preference meatPreference;
	public preference immoralPreference;
	public preference offalPreference;
	private AudioSource audioSource;
	private float _nausea;
	// private float lastNausea;
	public float nausea{
		get { 
			return _nausea;
		}
		set {
			_nausea = value;
			CheckNausea();
			}
	}
    private bool poisonNausea;
	private GameObject eaten;
	private bool LoadInitialized = false;
	private void CheckNausea(){
		//TODO: this is spawning lots of flags
		if (nausea > 15 && nausea < 30 && lastStatement != nauseaStatement.warning){
			lastStatement = nauseaStatement.warning;
			// lastNausea = nausea;
			Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("I don't feel so good!"));

			// Toolbox.Instance.DataFlag(gameObject, 0f, 5f, 10f, 0f, 0f);
			Toolbox.Instance.SpeechFlag(gameObject, "I don't feel so good!", chaos:2f, disturbing:10f, positive:-25f);
		}
		if (nausea > 30 && lastStatement != nauseaStatement.imminent){
			lastStatement = nauseaStatement.imminent;
			// lastNausea = nausea;
			Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("I'm gonna puke!"));
			// Toolbox.Instance.DataFlag(gameObject, 0f, 10f, 10f, 0f, 0f);
			Toolbox.Instance.SpeechFlag(gameObject, "I'm gonna puke!", chaos:5f, disturbing:13f, positive:-30f);
		}
		// if (nausea < 50){
		// 	lastNausea = 0;
		// }
	}
	void Start () {
		if (!LoadInitialized)
			LoadInit();
	}
	public void LoadInit(){
		// reversibleActions = false;
		Interaction eatAction = new Interaction(this, "Eat", "Eat");
		eatAction.defaultPriority = 1;
		eatAction.dontWipeInterface = false;
		// eatAction.otherConsent = false;
		eatAction.otherOnPlayerConsent = false;
		interactions.Add(eatAction);
		LoadInitialized = true;
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
	}
	void Update () {
        if (poisonNausea){
            nausea += Time.deltaTime * 50f;
        }
		if (nausea > 50){
			Vomit();
            poisonNausea = false;
		}
		if (nutrition > 100){
			nausea += Time.deltaTime * 2;
		}
	}
	public int CheckReaction(Edible food){
		int reaction = 0;
		//i can clean this section up with reflection-
		// might be necessary if food types get out of control
		bool[] types = new bool[] {food.vegetable, food.meat, food.immoral, food.offal};
		preference[] prefs = new preference[] {vegetablePreference, meatPreference, immoralPreference, 	offalPreference};
		for (int i =0; i< prefs.Length; i++){
			if (types[i]){
				switch (prefs[i]){
				case preference.dislikes:
					reaction -= 2;
					break;
				case preference.likes:
					reaction++;
					break;
				default:
					break;
				}
			}
		}
		return reaction;
	}
	public string Eat_desc(Edible food){
		string foodname = Toolbox.Instance.GetName(food.gameObject);
		return "Eat "+foodname;
	}
	public void Eat (Edible food){
		string phrase ="";
		int reaction;
		nutrition += food.nutrition;
        if (food.poison)
            poisonNausea = true;
		MessageHead head = new MessageHead();
		head.type = MessageHead.HeadType.eating;
		head.value = true;
		head.crumbColor = food.pureeColor;
		Toolbox.Instance.SendMessage(gameObject, this, head);
		//randomly store a clone of the object for later vomiting
        if (!food.poison){
            if (eaten){
				ClaimsManager.Instance.WasDestroyed(eaten);
                Destroy(eaten);
            }
            eaten = Instantiate(food.gameObject) as GameObject;
            eaten.SetActive(false);
        }
		//update our status based on our reaction to the food
		reaction = CheckReaction(food);
		if(reaction > 0){
			phrase = "Yummy!";
			// OccurrenceSpeech speech = new OccurrenceSpeech(positive:25f);
			// Toolbox.Instance.OccurrenceFlag
			// Toolbox.Instance.DataFlag(gameObject, 0f, 0f, 0f, 0f, 25f);
			Toolbox.Instance.SpeechFlag(gameObject, phrase, positive:25f);
		}
		if(reaction < 0){
			phrase = "Yuck!";
			nausea += 30;
			// Toolbox.Instance.DataFlag(gameObject, 0f, 0f, 0f, 0f, -25f);
			Toolbox.Instance.SpeechFlag(gameObject, phrase, positive:-25f);
		}
		// if we can speak, say the thing
		if (phrase != ""){
			Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech(phrase));
		}
		if (nutrition > 50){
			Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("I'm full!"));
		}
		if (nutrition > 75){
			Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("I can't eat another bite!"));
		}
		Toolbox.Instance.AddIntrinsic(gameObject, food.gameObject);
		
        // set up an occurrence flag for this eating!
        OccurrenceEat eatData = new OccurrenceEat();
		// eatData.edible = food;
        eatData.food = food.name;
        eatData.amount = food.nutrition;
		eatData.vomit = food.vomit;
		if (food.human){
			eatData.cannibalism = true;
		}
		if (food.vomit){
			eatData.disgusting += 100f;
			eatData.disturbing += 75f;
			eatData.chaos += 125f;
		}
		MonoLiquid mliquid = food.GetComponent<MonoLiquid>();
		if (mliquid){
			eatData.liquid = mliquid.liquid;
			if (eatData.liquid != null){
				if (eatData.liquid.vomit){
					eatData.disgusting += 100f;
					eatData.chaos += 200f;
					eatData.disturbing += 105f;
				}
				if (eatData.liquid.offal){
					eatData.disgusting += 75f;
					eatData.chaos += 50f;
				}
				if (eatData.liquid.immoral){
					eatData.disturbing += 100f;
					eatData.chaos += 150f;
					eatData.offensive += 500f;
				}
				if (eatData.liquid.name == "yogurt"){
					GameManager.Instance.data.achievementStats.yogurtEaten += 1;
					GameManager.Instance.CheckAchievements();
				}
			}
		}
		// if (food.vomit){
		// 	eatData.disgusting += 100f;
		// 	eatData.disturbing += 75f;
		// 	eatData.chaos += 125f;
		// }
        // MonoLiquid mliquid = food.GetComponent<MonoLiquid>();
        // if (mliquid){
        //     eatData.liquid = mliquid.liquid;
        //     if (mliquid.liquid.vomit){
        //         eatData.disgusting += 100f;
        //         eatData.chaos += 200f;
		// 		eatData.disturbing += 105f;
        //     }
        //     if (mliquid.liquid.offal){
        //         eatData.disgusting += 75f;
        //         eatData.chaos += 50f;
        //     }
        //     if (mliquid.liquid.immoral){
        //         eatData.disturbing += 100f;
        //         eatData.chaos += 150f;
		// 		eatData.offensive += 500f;
        //     }
		// 	if (mliquid.liquid.name == "yogurt"){
		//         GameManager.Instance.data.achievementStats.yogurtEaten += 1;
		// 		GameManager.Instance.CheckAchievements();
		// 	}
        // }
		Toolbox.Instance.OccurenceFlag(gameObject, eatData);
		if (food.eatSound != null){
			Toolbox.Instance.AudioSpeaker(food.eatSound, transform.position);
		}
		GameManager.Instance.CheckItemCollection(food.gameObject, gameObject);
		food.BeEaten();
	}
	void Vomit(){
        GameManager.Instance.data.achievementStats.vomit += 1;
		GameManager.Instance.CheckAchievements();

		nausea = 0;
		nutrition = 0;
                
        OccurrenceVomit data = new OccurrenceVomit();
		data.vomiter = Toolbox.Instance.CloneRemover(gameObject.name);
        // data.disgusting = 100f;
		if (eaten){
            data.vomit = eaten.name;
			eaten.SetActive(true);
			eaten.transform.position = transform.position;
            MonoLiquid mono = eaten.GetComponent<MonoLiquid>();
            if (mono){
                GameObject droplet = Toolbox.Instance.SpawnDroplet(transform.position, mono.liquid);
                mono.liquid.vomit = true;
                mono.edible.vomit = true;
                data.liquid = mono.liquid;
				if (data.liquid.name == "yogurt"){
			        GameManager.Instance.data.achievementStats.yogurtVomit += 1;
					GameManager.Instance.CheckAchievements();
				}
				CircleCollider2D dropCollider = droplet.GetComponent<CircleCollider2D>();
				foreach(Collider2D collider in GetComponentsInChildren<Collider2D>()){
					Physics2D.IgnoreCollision(dropCollider, collider, true);
				}
            }
            Edible edible = eaten.GetComponent<Edible>();
            if (edible){
                edible.vomit = true;
            }
			eaten = null;
		}
		Toolbox.Instance.OccurenceFlag(gameObject, data);

		MessageHead head = new MessageHead();
		head.type = MessageHead.HeadType.vomiting;
		head.value = true;
		Toolbox.Instance.SendMessage(gameObject, this, head);
		Toolbox.Instance.SendMessage(gameObject, this, new MessageSpeech("Blaaaaargh!"));
		if (audioSource){
			audioSource.PlayOneShot(Resources.Load("sounds/vomit", typeof(AudioClip)) as AudioClip);
		}
	}
}
